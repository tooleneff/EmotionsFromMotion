using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System.IO;

namespace EmotionsFromMotion
{
    public class eLayer
    {
        readonly string myKey = "113e0a44087442ca8d2543a8ac9a9da5";
        EmotionServiceClient eClient;
        public eLayer()
        {
            eClient = new EmotionServiceClient(myKey);
        }
        public async Task<Emotion[]> GetEmotions(Stream imageStream, Rectangle[] faceRectangles)
        {
            return await eClient.RecognizeAsync(imageStream, faceRectangles);
        }
    }
}
