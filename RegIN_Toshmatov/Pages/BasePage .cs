using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace RegIN_Toshmatov.Pages
{
    public abstract class BasePage : Page
    {
        public string OldLogin;
        public bool IsCapture;

        public void AnimateImageChange(Image image, ImageSource newSource)
        {
            var startAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.6)
            };

            startAnimation.Completed += (s, e) =>
            {
                image.Source = newSource;
                var endAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1.2)
                };
                image.BeginAnimation(Image.OpacityProperty, endAnimation);
            };

            image.BeginAnimation(Image.OpacityProperty, startAnimation);
        }

        public ImageSource LoadUserImage(byte[] imageData)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var biImg = new BitmapImage();
                    using (var ms = new MemoryStream(imageData))
                    {
                        biImg.BeginInit();
                        biImg.StreamSource = ms;
                        biImg.CacheOption = BitmapCacheOption.OnLoad;
                        biImg.EndInit();
                    }
                    return biImg;
                }
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
            return null;
        }

        public abstract void SetNotification(string message, SolidColorBrush color);
    }
}