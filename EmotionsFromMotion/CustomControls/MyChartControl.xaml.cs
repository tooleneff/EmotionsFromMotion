using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using EmotionsFromMotion.Model;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EmotionsFromMotion.CustomControls
{
    public sealed partial class MyChartControl : UserControl
    {
        public List<EmotionAndTime> itemSource { get; set; }
        public MyChartControl()
        {
            this.InitializeComponent();
        }
        public void PopulateData()
        {
            (LineChart.Series[0] as LineSeries).ItemsSource = itemSource;
            (LineChart.Series[1] as LineSeries).ItemsSource = itemSource;
            (LineChart.Series[2] as LineSeries).ItemsSource = itemSource;
            (LineChart.Series[3] as LineSeries).ItemsSource = itemSource;
            (LineChart.Series[4] as LineSeries).ItemsSource = itemSource;
            (LineChart.Series[5] as LineSeries).ItemsSource = itemSource;
            (LineChart.Series[6] as LineSeries).ItemsSource = itemSource;
            (LineChart.Series[7] as LineSeries).ItemsSource = itemSource;

            this.Visibility = Visibility.Visible;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
