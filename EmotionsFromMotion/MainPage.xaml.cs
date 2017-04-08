using System;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Media.Editing;
using Windows.UI.Popups;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;
using Windows.UI.Xaml.Controls.Primitives;
using EmotionsFromMotion.Model;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Common;
using Newtonsoft.Json;
using Microsoft.ProjectOxford.Face.Contract;
using System.Diagnostics;
using System.Collections.Generic;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmotionsFromMotion
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private enum PlayStates
        {
            play,
            pause
        }
        private PlayStates playState { get; set; }
        private FileOpenPicker filePicker;
        FaceDetector faceDetector;
        MediaSource mediaSource;
        MediaPlaybackItem playBackItem;
        MediaPlayer mediaPlayer;
        StorageFile videoFile;
        MediaClip mediaClip;
        MediaComposition mediaComposition;
        StorageFile VideoFile
        {
            get
            {
                return videoFile;
            }
            set
            {
                videoFile = value;
                
            }
        }
        public async Task UpdateState()
        {
            StorageFolder vf = await ApplicationData.Current.LocalFolder.CreateFolderAsync("video",CreationCollisionOption.OpenIfExists);
            if ((await vf.GetFilesAsync()).Count(x=>x.Name.ToLower()=="emotions.txt")>0)
            {
                buttonDrawChart.IsEnabled = true;
            }
            else
            {
                buttonDrawChart.IsEnabled = false;
            }
            var emotionsFile = await vf.GetFileAsync("emotions.txt");

            if (VideoFile == null)
            {
                ClearVideoResources();
                playSlider.IsEnabled = false;
                playButton.IsEnabled = false;
                await Task.Delay(10);
                return ;
            }
            else
            {
                ClearVideoResources();
                mediaSource = MediaSource.CreateFromStorageFile(VideoFile);
                playBackItem = new MediaPlaybackItem(mediaSource);
                mediaPlayer = new MediaPlayer();
                mediaPlayer.Source = playBackItem;
                media2.SetMediaPlayer(mediaPlayer);
                mediaClip = await MediaClip.CreateFromFileAsync(VideoFile);
                mediaComposition = new MediaComposition();
                mediaComposition.Clips.Add(mediaClip);
                //mediaClip.TrimTimeFromEnd = mediaClip.OriginalDuration - TimeSpan.FromMinutes(3);
                await Task.Delay(10);
                playButton.IsEnabled = true;
                playSlider.IsEnabled = true;
                var mpSession = mediaPlayer.PlaybackSession;
                await Task.Delay(1000);
                playSlider.Maximum = mpSession.NaturalDuration.TotalSeconds;
                playSlider.StepFrequency = 0.5;
                media2.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
                return;
            }
        }

        private void ClearVideoResources()
        {
            if (mediaPlayer != null) mediaPlayer.Dispose();
            if (playBackItem != null) playBackItem = null;
            if (mediaSource != null) mediaSource.Dispose();
            mediaSource = null;
            playBackItem = null;
            mediaClip = null;
            mediaComposition = null;
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            var a = sender.Position.TotalSeconds;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                playSlider.Value = a;
            });
        }

        public MainPage()
        {
            this.InitializeComponent();
            playSlider.IsEnabled = false;
            playButton.IsEnabled = false;
            filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;            
            filePicker.FileTypeFilter.Add(".mp4");
            filePicker.FileTypeFilter.Add(".mpg4");
            filePicker.FileTypeFilter.Add(".avi");
            
            UpdatePersonGroupsData();
            personGroupForm.ButtonSaveTapped += PersonGroupForm_ButtonSaveTapped;
            personForm.ButtonSaveTapped += PersonForm_ButtonSaveTapped;
        }

        private async void PersonForm_ButtonSaveTapped(string groupName)
        {
            waitScreen.Show();
            using (var fl = new fLayer())
            {
                var a = await fl.AddPerson(textBlockPersonGroup.Text, groupName);
                textBlockPerson.Text = a.PersonId.ToString();
                UpdatePersonGroupsData();
            }
        }

        private async void PersonGroupForm_ButtonSaveTapped(string groupName)
        {
            waitScreen.Show();
            using (var fl = new fLayer())
            {
                await fl.AddPersonGroup(groupName);
                UpdatePersonGroupsData();
            }
        }

        async void UpdatePersonGroupsData()
        {
            waitScreen.Show();
            using (var fl = new fLayer())
            {
                var a = await fl.GetMyPersonGroups();
                
                if (a.Length>0)
                {
                    textBlockPersonGroup.Text = a[0].PersonGroupId;
                    buttonCreateGroup.IsEnabled = false;
                    var b = await fl.GetMyPersons(a[0].PersonGroupId);
                    if (b.Length>0)
                    {
                        textBlockPerson.Text = b[0].PersonId.ToString();
                        buttonCreatePerson.IsEnabled = false;
                        foreach (var item in b[0].PersistedFaceIds)
                        {
                            textBlockFaces.Text += item.ToString() + Environment.NewLine;
                        }
                        if (b[0].PersistedFaceIds.Length>0)
                        {
                            buttonTrainGroup.IsEnabled = true;
                        }
                        else
                        {
                            buttonTrainGroup.IsEnabled = false;
                        }
                    }
                    else
                    {
                        buttonCreatePerson.IsEnabled = true;
                    }
                }
                else
                {
                    buttonCreateGroup.IsEnabled = true;
                }
            }
            waitScreen.Hide();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await UpdateState();
        }
        private async void AddFace()
        {
            string exError = "";
            waitScreen.Show();
            var personGroupID = textBlockPersonGroup.Text;
            var personID = textBlockPerson.Text;
            if (string.IsNullOrWhiteSpace(personGroupID)|string.IsNullOrWhiteSpace(personID)|mediaComposition==null)
            {
                return;
            }
            using (var a = await mediaComposition.GetThumbnailAsync(TimeSpan.FromSeconds(playSlider.Value), 0, 0, VideoFramePrecision.NearestFrame))
            {
                using (var fl = new fLayer())
                {
                    using (var s = a.AsStream())
                    {
                        
                        try
                        {
                            var b = await fl.AddFaseToPerson(personGroupID, Guid.Parse(personID), s);
                            textBlockFaces.Text += b.PersistedFaceId.ToString() + Environment.NewLine;
                        }
                        catch (Exception ex)
                        {
                            var d = ex as FaceAPIException;
                            exError = d.ErrorCode + d.ErrorMessage;
                        }
                        
                    }
                }
            }
            waitScreen.Hide();
            if (!string.IsNullOrWhiteSpace(exError))
            {
                var md = new MessageDialog(exError);
                await md.ShowAsync();
            }
        }
        
        public async Task<IInputStream> GetThumbnailAsync(StorageFile file)
        {
            var mediaClip = await MediaClip.CreateFromFileAsync(file);
            var mediaComposition = new MediaComposition();
            mediaComposition.Clips.Add(mediaClip);
            return await mediaComposition.GetThumbnailAsync(
                TimeSpan.FromSeconds(2), 0, 0, VideoFramePrecision.NearestFrame);
        }
        public async Task CutIntoFrames()
        {
            waitScreen.Show();
            StorageFolder sf = ApplicationData.Current.LocalFolder;
            var emotions = new List<EmotionAndTime>();
            var files = new List<FileWithFace>();
            string personGroupID = textBlockPersonGroup.Text;
            Guid personID = Guid.Parse(textBlockPerson.Text);
            var vf = await sf.CreateFolderAsync("video", CreationCollisionOption.OpenIfExists);
            mediaClip.TrimTimeFromEnd = mediaClip.OriginalDuration - TimeSpan.FromMinutes(10);
            for (int i = 2; i < mediaClip.EndTimeInComposition.TotalSeconds; i=i+2)
            {
                bool hasFace = false;
                //Получаем кадр из видео
                using (var screenShot = await mediaComposition.GetThumbnailAsync(TimeSpan.FromSeconds(i), 0, 0, VideoFramePrecision.NearestFrame))
                {
                    //Декодируем кадр в Gray8 или Nv12 и если необходимо уменьшаем размер
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(screenShot);
                    BitmapTransform transform = new BitmapTransform();
                    const float sourceImageHeightLimit = 1280;
                    if (decoder.PixelHeight > sourceImageHeightLimit)
                    {
                        float scalingFactor = (float)sourceImageHeightLimit / (float)decoder.PixelHeight;
                        transform.ScaledWidth = (uint)Math.Floor(decoder.PixelWidth * scalingFactor);
                        transform.ScaledHeight = (uint)Math.Floor(decoder.PixelHeight * scalingFactor);
                    }
                    SoftwareBitmap sourceBitmap = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
                    const BitmapPixelFormat faceDetectionPixelFormat = BitmapPixelFormat.Gray8;
                    SoftwareBitmap convertedBitmap;
                    if (sourceBitmap.BitmapPixelFormat != faceDetectionPixelFormat)
                    {
                        convertedBitmap = SoftwareBitmap.Convert(sourceBitmap, faceDetectionPixelFormat);
                    }
                    else
                    {
                        convertedBitmap = sourceBitmap;
                    }
                    //Определяем, есть ли лица на кадре
                    if (faceDetector == null)
                    {
                        faceDetector = await FaceDetector.CreateAsync();
                    }

                    var detectedFaces = await faceDetector.DetectFacesAsync(convertedBitmap);
                    if (detectedFaces.Count>0)
                    {
                        hasFace = true;
                    }
                }
                if (hasFace)
                {
                    using (var fl = new fLayer())
                    {
                        using (var s = (await mediaComposition.GetThumbnailAsync(TimeSpan.FromSeconds(i), 0, 0, VideoFramePrecision.NearestFrame)).AsStream())
                        {
                            var facesOnImage = await fl.DetectFace(s);
                            if (facesOnImage.Length>0)
                            {
                                var identResults = await fl.Identify(personGroupID, facesOnImage.Select(x => x.FaceId).ToArray());
                                if (identResults.Count(x => x.Candidates.Count(y => y.PersonId == personID) > 0) > 0)
                                {
                                    var a = identResults.Where(x => x.Candidates.Count(y => y.PersonId == personID) > 0).ToArray();
                                    var b = facesOnImage.Where(x => a.Select(y => y.FaceId).Contains(x.FaceId)).ToArray();
                                    if (b.Length > 0)
                                    {
                                        var el = new eLayer();
                                        foreach (var item in b)
                                        {
                                            var rect = new Rectangle
                                            {
                                                Height = item.FaceRectangle.Height,
                                                Width = item.FaceRectangle.Width,
                                                Left = item.FaceRectangle.Left,
                                                Top = item.FaceRectangle.Top
                                            };
                                            using (var s2 = (await mediaComposition.GetThumbnailAsync(TimeSpan.FromSeconds(i), 0, 0, VideoFramePrecision.NearestFrame)).AsStream())
                                            {
                                                var c = await el.GetEmotions(s2, new Rectangle[] { rect });
                                                StorageFile imageFile = await vf.CreateFileAsync(Guid.NewGuid().ToString() + ".jpg", CreationCollisionOption.ReplaceExisting);
                                                var coverStream = await imageFile.OpenAsync(FileAccessMode.ReadWrite); // IRandomAccessStream
                                                using (var coverOutputStream = coverStream.AsStreamForWrite())
                                                {
                                                    using (var s3 = (await mediaComposition.GetThumbnailAsync(TimeSpan.FromSeconds(i), 0, 0, VideoFramePrecision.NearestFrame)).AsStream())
                                                    {
                                                        await s3.CopyToAsync(coverOutputStream);
                                                        await coverOutputStream.FlushAsync();
                                                        files.Add(new FileWithFace
                                                        {
                                                            FaceID = item.FaceId,
                                                            SecondsFromBegining = i,
                                                            Path = imageFile.Path
                                                        });
                                                    }
                                                }
                                                foreach (var item1 in c)
                                                {
                                                    emotions.Add(new EmotionAndTime
                                                    {
                                                        EmotionResult = item1,
                                                        PersonGroupID = personGroupID,
                                                        PersonID = personID,
                                                        SecondsFromBegining = i
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var tempF1 = await vf.CreateFileAsync("imageFiles.txt",CreationCollisionOption.ReplaceExisting);
            using (var s1 = await tempF1.OpenStreamForWriteAsync())
            {
                var tempS1 = JsonConvert.SerializeObject(files);
                
                using (var s2 = GenerateStreamFromString(tempS1))
                {
                    await s2.CopyToAsync(s1);
                    await s1.FlushAsync();                }
            }
            var tempF2 = await vf.CreateFileAsync("emotions.txt", CreationCollisionOption.ReplaceExisting);
            using (var s1 = await tempF2.OpenStreamForWriteAsync())
            {
                var tempS1 = JsonConvert.SerializeObject(emotions);

                using (var s2 = GenerateStreamFromString(tempS1))
                {
                    await s2.CopyToAsync(s1);
                    await s1.FlushAsync();
                }
            }
            await UpdateState();
            waitScreen.Hide();
            var md = new MessageDialog(tempF1.Path + Environment.NewLine + tempF2.Path);
            await md.ShowAsync();

        }

        private async void Button_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            if (VideoFile == null)
            {
                var md = new MessageDialog("Сначала выберите файл");
                await md.ShowAsync();
                return;
            }
            else
            {
                await CutIntoFrames();
            }
        }

        private void playButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (playSymbol.Symbol.Equals(Symbol.Play))
            {
                playSlider.Maximum = mediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                playSymbol.Symbol = Symbol.Pause;
                media2.MediaPlayer.Play();
                playState = PlayStates.play;
            }
            else if (playSymbol.Symbol.Equals(Symbol.Pause))
            {
                playSymbol.Symbol = Symbol.Play;
                media2.MediaPlayer.Pause();
                playState = PlayStates.pause;
            }
        }

        
        private void ResumePlayBack()
        {
            if (mediaPlayer == null)
            {
                return;
            }
            if (playState == PlayStates.pause)
            {
                return;
            }
            media2.MediaPlayer.Play();
        }
        private void HoldPlayBack()
        {
            if (mediaPlayer == null)
            {
                return;
            }
            if (playState == PlayStates.pause)
            {
                return;
            }
            media2.MediaPlayer.Pause();
        }

        private void playSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            HoldPlayBack();
        }

        private void playSlider_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            media2.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(playSlider.Value);
        }

        private void playSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ResumePlayBack();
        }

        private void playSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var a = e.NewValue - e.OldValue;
            if(a>=0&&a<=1)
            {
                return;
            }
            else
            {
                media2.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(playSlider.Value);
            }
        }

        private async void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VideoFile = await filePicker.PickSingleFileAsync();
            await UpdateState();
        }

        private void buttonCreateGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            personGroupForm.Show();
        }

        private void buttonCreatePerson_Tapped(object sender, TappedRoutedEventArgs e)
        {
            personForm.Show();
        }

        private async void buttonTrainGroup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            waitScreen.Show();
            using (var fl = new fLayer())
            {
                await fl.TrainGroup(textBlockPersonGroup.Text);
            }
            waitScreen.Hide();
        }

        private void buttonAddFace_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AddFace();
        }
        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private async void buttonDrawChart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            waitScreen.Show();
            var dFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("video", CreationCollisionOption.OpenIfExists);
            var dFile = await dFolder.GetFileAsync("emotions.txt");
            var dFileContent = await FileIO.ReadTextAsync(dFile);
            var emotionsData = await Task.Factory.StartNew(()=> JsonConvert.DeserializeObject<List<EmotionAndTime>>(dFileContent));
            myChart.itemSource = emotionsData;
            myChart.PopulateData();
            myChart.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //var a = emotionsData[0].EmotionResult.Scores.Happiness
            waitScreen.Hide();
        }
    }

}
