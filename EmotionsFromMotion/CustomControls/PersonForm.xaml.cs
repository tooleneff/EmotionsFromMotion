using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EmotionsFromMotion.CustomControls
{
    public sealed partial class PersonForm : UserControl
    {
        public delegate void ButtonEventsHandler(string groupName);
        public event ButtonEventsHandler ButtonSaveTapped;

        public PersonForm()
        {
            this.InitializeComponent();
        }
        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }
        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var s = textBoxPersonName.Text.Trim();
            if (string.IsNullOrWhiteSpace(s))
            {
                return;
            }
            ButtonSaveTapped?.Invoke(textBoxPersonName.Text.Trim());
            textBoxPersonName.Text = "";
            Hide();
        }

        private void Button_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            textBoxPersonName.Text = "";
            Hide();
        }
    }

}
