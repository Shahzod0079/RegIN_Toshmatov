using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace RegIN_Toshmatov.Pages
{
    /// <summary>
    /// Логика взаимодействия для Regin.xaml
    /// </summary>
    public partial class Regin : Page
    {
        //переменные
        OpenFileDialog FileDialogImage = new OpenFileDialog();
        bool BCorrectLogin = false;
        bool BCorrectPassword = false;
        bool BCorrectConfirmPassword = false;
        bool BSetImages = false;

        public Regin()
        {
            InitializeComponent();
            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            FileDialogImage.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg";
            FileDialogImage.RestoreDirectory = true;
            FileDialogImage.Title = "Choose a photo for your avatar";
        }
        //метод правильно введённого логина и неправильно:
        private void CorrectLogin()
        {
            SetNotification("Login already in use", Brushes.Red);
            BCorrectLogin = false;
        }

        private void InCorrectLogin() =>
            SetNotification("", Brushes.Black);
    
    //метод и его перезагрузки для ввода логина пользователя:
    private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetLogin();
            }
        }

        private void SetLogin(object sender, System.Windows.RoutedEventArgs e) =>
            SetLogin();

        public void SetLogin()
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9._-]{4,}[@][a-zA-Z0-9._-]{2,}\.[a-zA-Z0-9._-]{2,}$");
            BCorrectLogin = regex.IsMatch(TbLogin.Text);

            if (regex.IsMatch(TbLogin.Text) == true)
            {
                SetNotification("", Brushes.Black);
                MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);
            }
            else
            {
                SetNotification("Invalid login", Brushes.Red);
            }

            OnRegin();
        }
        private void SetLogin(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем буквы, цифры и символы для email: @ . _ -
            e.Handled = !(Char.IsLetterOrDigit(e.Text, 0) ||
                          e.Text == "@" ||
                          e.Text == "." ||
                          e.Text == "_" ||
                          e.Text == "-");
        }
        //метод и его перезагрузки для ввода пароля пользователя:

        #region SetPassword
        private void SetPassword(object sender, System.Windows.RoutedEventArgs e) =>
            SetPassword();

        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetPassword();
        }

        public void SetPassword()
        {
            Regex regex = new Regex(@"(?=.*[0-9])(?=.*[!@#$%&?*\-_=])(?=.*[a-z])(?=.*[A-Z])[0-9a-zA-Z!@#$%&?*\-_=]{10,}");

            BCorrectPassword = regex.IsMatch(TbPassword.Password);

            if (regex.IsMatch(TbPassword.Password) == true)
            {
                SetNotification("", Brushes.Black);

                if (TbConfirmPassword.Password.Length > 0)
                    ConfirmPassword(true);

                OnRegin();
            }
            else
                SetNotification("Invalid password", Brushes.Red);
        }
        #endregion
        //метод и его перезагрузки для ввода подтверждения пароля пользователя:
        #region SetConfirmPassword
        private void ConfirmPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmPassword();
            }
        }

        private void ConfirmPassword(object sender, System.Windows.RoutedEventArgs e) =>
            ConfirmPassword();

        public void ConfirmPassword(bool Pass = false)
        {
            BCorrectConfirmPassword = TbConfirmPassword.Password == TbPassword.Password;

            if (TbConfirmPassword.Password != TbPassword.Password)
            {
                SetNotification("Passwords do not match", Brushes.Red);
            }
            else
            {
                SetNotification("", Brushes.Black);

                if (!Pass)
                {
                    SetPassword();
                }
            }
        }
        #endregion

        //етод регистрации пользователя:
        void OnRegin()
        {
            if (!BCorrectLogin)
            {
                return;
            }

            if (TbName.Text.Length == 0)
            {
                return;
            }

            if (!BCorrectPassword)
            {
                return;
            }

            if (!BCorrectConfirmPassword)
            {
                return;
            }

            MainWindow.mainWindow.UserLogIn.Login = TbLogin.Text;
            MainWindow.mainWindow.UserLogIn.Password = TbPassword.Password;
            MainWindow.mainWindow.UserLogIn.Name = TbName.Text;

            if (BSetImages)
            {
                MainWindow.mainWindow.UserLogIn.Image = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\IUser.jpg");
                MainWindow.mainWindow.UserLogIn.DateTimeUpdate = DateTime.Now;
                MainWindow.mainWindow.UserLogIn.DateTimeCreate = DateTime.Now;
                MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Regin));
            }
        }
        //метод проверки ввода только букв:
        private void SetName(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsLetter(e.Text, 0));
        }
        //етод SetNotification
        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            LNameUser.Content = Message;
            LNameUser.Foreground = _Color;
        }
        private void SelectImage(object sender, MouseButtonEventArgs e)
        {
            if (FileDialogImage.ShowDialog() == true)
            {
                string tempFile = Path.GetTempFileName() + ".jpg";
                string finalFile = Path.Combine(Directory.GetCurrentDirectory(), "IUser.jpg");

                try
                {
                    if (File.Exists(finalFile))
                    {
                        File.Delete(finalFile);
                        Thread.Sleep(100);
                    }

                    using (var image = Aspose.Imaging.Image.Load(FileDialogImage.FileName))
                    {
                        int newWidth, newHeight;

                        if (image.Width > image.Height)
                        {
                            newWidth = (int)(image.Width * (256f / image.Height));
                            newHeight = 256;
                        }
                        else
                        {
                            newWidth = 256;
                            newHeight = (int)(image.Height * (256f / image.Width));
                        }

                        image.Resize(newWidth, newHeight);
                        image.Save(tempFile);
                    }

                    using (var rasterImage = (Aspose.Imaging.RasterImage)Aspose.Imaging.Image.Load(tempFile))
                    {
                        if (!rasterImage.IsCached)
                            rasterImage.CacheData();

                        int x = 0, y = 0;

                        if (rasterImage.Width > rasterImage.Height)
                            x = (int)((rasterImage.Width - 256f) / 2);
                        else
                            y = (int)((rasterImage.Height - 256f) / 2);

                        var rectangle = new Aspose.Imaging.Rectangle(x, y, 256, 256);
                        rasterImage.Crop(rectangle);
                        rasterImage.Save(finalFile);
                    }

                    var bitmap = new BitmapImage();
                    using (var stream = new FileStream(finalFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    bitmap.Freeze(); 

                    var startAnimation = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.6)
                    };

                    startAnimation.Completed += (s, args) =>
                    {
                        User.Source = bitmap;
                        var endAnimation = new DoubleAnimation
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromSeconds(1.2)
                        };
                        User.BeginAnimation(Image.OpacityProperty, endAnimation);
                    };

                    User.BeginAnimation(Image.OpacityProperty, startAnimation);
                    BSetImages = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке изображения: {ex.Message}");
                    BSetImages = false;
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        try { File.Delete(tempFile); } catch { }
                    }
                }
            }
            else
            {
                BSetImages = false;
            }
        }
        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
