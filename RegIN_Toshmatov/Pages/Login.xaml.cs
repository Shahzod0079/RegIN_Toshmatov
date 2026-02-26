using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RegIN_Toshmatov.Classes;

namespace RegIN_Toshmatov.Pages
{


    public partial class Login : Page
    {
        string OldLogin;
        int CountSetPassword = 2;
        bool IsCapture = false;

        public Login()
        {
            InitializeComponent();
            //созданные методы на события:
            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            Capture.HandlerCorrectCapture += CorrectCapture;
        }
        //метод успешной авторизации,
        public void CorrectLogin()
        {
            if (OldLogin != tBlogin.Text)
            {
                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);

                try
                {
                    BitmapImage biImg = new BitmapImage();
                    MemoryStream ms = new MemoryStream(MainWindow.mainWindow.UserLogIn.Image);
                    biImg.BeginInit();
                    biImg.StreamSource = ms;
                    biImg.EndInit();
                    ImageSource imgSrc = biImg;

                    DoubleAnimation StartAnimation = new DoubleAnimation();
                    StartAnimation.From = 1;
                    StartAnimation.To = 0;
                    StartAnimation.Duration = TimeSpan.FromSeconds(0.6);
                    StartAnimation.Completed += delegate
                    {
                        User.Source = imgSrc;
                        DoubleAnimation EndAnimation = new DoubleAnimation();
                        EndAnimation.From = 0;
                        EndAnimation.To = 1;
                        EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                        User.BeginAnimation(Image.OpacityProperty, EndAnimation);
                    };
                    User.BeginAnimation(Image.OpacityProperty, StartAnimation);
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp.Message);
                }

                OldLogin = tBlogin.Text;
            }
        }
        //метод неуспешной авторизации, 
        public void InCorrectLogin()
        {
            if (lNameUserName.Content != "")
            {
                lNameUserName.Content = "";

                DoubleAnimation StartAnimation = new DoubleAnimation();
                StartAnimation.From = 1;
                StartAnimation.To = 0;
                StartAnimation.Duration = TimeSpan.FromSeconds(0.6);
                StartAnimation.Completed += delegate
                {
                    User.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic-user.png"));
                    DoubleAnimation EndAnimation = new DoubleAnimation();
                    EndAnimation.From = 0;
                    EndAnimation.To = 1;
                    EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                    User.BeginAnimation(OpacityProperty, EndAnimation);
                };
                User.BeginAnimation(OpacityProperty, StartAnimation);
            }

            if (tBlogin.Text.Length > 0)
                SetNotification("Login is incorrect", Brushes.Red);
        }
        //метод правильного ввода капчи:
        public void CorrectCapture()
        {
            Capture.IsEnabled = false;
            IsCapture = true;
        }

        //прописать метод ввода пароля, который при нажатии «Enter», вызывает перегруженный метод ввода пароля.
        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetPassword();
            }
        }

        public void SetPassword()
        {
            if (MainWindow.mainWindow.UserLogIn.Password != String.Empty)
            {
                if (IsCapture)
                {
                    if (MainWindow.mainWindow.UserLogIn.Password == tbPassword.Password)
                    {
                        MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Login));
                    }
                    else
                    {
                        if (CountSetPassword > 0)
                        {
                            SetNotification($"Password is incorrect, {CountSetPassword} attempts left", Brushes.Red);
                            CountSetPassword--;
                        }
                        else
                        {
                            Thread TBlockAutorization = new Thread(BlockAutorization);
                            TBlockAutorization.Start();
                        }

                        SendMail.SendMessage("An attempt was made to log into your account.", MainWindow.mainWindow.UserLogIn.Login);
                    }
                }
                else
                {
                    SetNotification("Enter capture", Brushes.Red);
                }
            }

            //метод блокировки пользователя.
            public void BlockAuthorization()
        {
            DateTime StartBlock = DateTime.Now.AddMinutes(3);

            Dispatcher.Invoke(() => {
                tBlogin.IsEnabled = false;
                tbPassword.IsEnabled = false;
                Capture.IsEnabled = false;
            });

            for (int i = 0; i < 180; i++)
            {
                TimeSpan TimeIdle = StartBlock.Subtract(DateTime.Now);
                string s_minutes = TimeIdle.Minutes.ToString();

                if (TimeIdle.Minutes < 10)
                {
                    s_minutes = "0" + TimeIdle.Minutes;
                }

                string s_seconds = TimeIdle.Seconds.ToString();

                if (TimeIdle.Seconds < 10)
                {
                    s_seconds = "0" + TimeIdle.Seconds;
                }

                Dispatcher.Invoke(() => {
                    SetNotification($"Reauthorization available in: {s_minutes}:{s_seconds}", Brushes.Red);
                });

                Thread.Sleep(1000);
            }

            Dispatcher.Invoke(() => {
                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);
                tBlogin.IsEnabled = true;
                tbPassword.IsEnabled = true;
                Capture.IsEnabled = true;
                Capture.CreateCapture();
                IsCapture = false;
                CountSetPassword = 2;
            });
        }

        //метод ввода логина пользователя, и перезагруженный метод ввода логина пользователя.
        private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.mainWindow.UserLogIn.GetUserLogin(tBlogin.Text);

                if (tbPassword.Password.Length > 0)
                    SetPassword();
            }
        }

        private void SetLogin(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.UserLogIn.GetUserLogin(tBlogin.Text);

            if (tbPassword.Password.Length > 0)
                SetPassword();
        }
        //метод вывода уведомлений.
        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            lNameUserName.Content = Message;
            lNameUserName.Foreground = _Color;
        }
        // метод открытия страницы восстановления пароля и регистрации.

        private void RecoveryPassword(object sender, MouseButtonEventArgs e) =>
    MainWindow.mainWindow.OpenPage(new Recovery());

        private void OpenRegion(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Regin());
    }
}

