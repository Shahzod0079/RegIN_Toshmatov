using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                using (Aspose.Imaging.Image image = Aspose.Imaging.Image.Load(FileDialogImage.FileName))
                {
                    int NewWidth = 0;
                    int NewHeight = 0;

                    if (image.Width > image.Height)
                    {
                        NewWidth = (int)(image.Width * (256f / image.Height));
                        NewHeight = 256;
                    }
                    else
                    {
                        NewWidth = 256;
                        NewHeight = (int)(image.Height * (256f / image.Width));
                    }

                    image.Resize(NewWidth, NewHeight);
                    image.Save("IUser.jpg");
                }

                using (Aspose.Imaging.RasterImage rasterImage = (Aspose.Imaging.RasterImage)Aspose.Imaging.Image.Load("IUser.jpg"))
                {
                    if (!rasterImage.IsCached)
                    {
                        rasterImage.CacheData();
                    }

                    int X = 0;
                    int Width = 256;
                    int Y = 0;
                    int Height = 256;

                    if (rasterImage.Width > rasterImage.Height)
                        X = (int)((rasterImage.Width - 256f) / 2);
                    else
                        Y = (int)((rasterImage.Height - 256f) / 2);

                    Aspose.Imaging.Rectangle rectangle = new Aspose.Imaging.Rectangle(X, Y, Width, Height);
                    rasterImage.Crop(rectangle);
                    rasterImage.Save("IUser.jpg");
                }

                DoubleAnimation StartAnimation = new DoubleAnimation();
                StartAnimation.From = 1;
                StartAnimation.To = 0;
                StartAnimation.Duration = TimeSpan.FromSeconds(0.6);
                StartAnimation.Completed += delegate
                {
                    User.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\IUser.jpg"));
                    DoubleAnimation EndAnimation = new DoubleAnimation();
                    EndAnimation.From = 0;
                    EndAnimation.To = 1;
                    EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                    User.BeginAnimation(Image.OpacityProperty, EndAnimation);
                };

                User.BeginAnimation(Image.OpacityProperty, StartAnimation);
                BSetImages = true;
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
