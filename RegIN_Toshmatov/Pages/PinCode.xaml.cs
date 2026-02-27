using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RegIN_Toshmatov.Classes;

namespace RegIN_Toshmatov.Pages
{
    public partial class PinCode : Page
    {
        private bool isFirstTime;
        private string firstPin = "";
        private User _currentUser;

        public PinCode(User user, bool isFirstTime = true)
        {
            InitializeComponent();
            _currentUser = user;
            this.isFirstTime = isFirstTime;

            if (isFirstTime)
            {
                TitleText.Text = "Create 4-digit PIN code";
                ForgotPinLabel.Visibility = Visibility.Collapsed; 
            }
            else
            {
                TitleText.Text = "Enter your PIN code";
                ForgotPinLabel.Visibility = Visibility.Visible; 
            }
        }

        private void PinBox_KeyUp(object sender, KeyEventArgs e)
        {
            var box = sender as PasswordBox;

            if (box.Password.Length == 1)
            {
                if (box == PinBox1) PinBox2.Focus();
                else if (box == PinBox2) PinBox3.Focus();
                else if (box == PinBox3) PinBox4.Focus();
                else if (box == PinBox4) CheckPinCode();
            }
            else if (e.Key == Key.Back && box.Password.Length == 0)
            {
                if (box == PinBox2) PinBox1.Focus();
                else if (box == PinBox3) PinBox2.Focus();
                else if (box == PinBox4) PinBox3.Focus();
            }
        }

        private void CheckPinCode()
        {
            string pin = PinBox1.Password + PinBox2.Password +
                        PinBox3.Password + PinBox4.Password;

            if (pin.Length != 4)
            {
                MessageLabel.Content = "Enter 4 digits";
                return;
            }

            if (isFirstTime)
            {
                if (string.IsNullOrEmpty(firstPin))
                {
                    firstPin = pin;
                    ClearBoxes();
                    MessageLabel.Content = "Confirm PIN code";
                }
                else
                {
                    if (firstPin == pin)
                    {
                        _currentUser.SavePinCode(pin);
                        MessageBox.Show("PIN code saved!");
                        MainWindow.mainWindow.OpenPage(new Login());
                    }
                    else
                    {
                        MessageLabel.Content = "PINs don't match";
                        firstPin = "";
                        ClearBoxes();
                    }
                }
            }
            else
            {
                if (_currentUser.CheckPinCode(pin))
                {
                    MessageBox.Show("Quick login successful!");
                    MainWindow.mainWindow.OpenPage(new Login());
                }
                else
                {
                    MessageLabel.Content = "Wrong PIN";
                    ClearBoxes();
                }
            }
        }

        private void ClearBoxes()
        {
            PinBox1.Password = PinBox2.Password =
            PinBox3.Password = PinBox4.Password = "";
            PinBox1.Focus();
        }
        private void ForgotPin(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.mainWindow.UserLogIn != null &&
                !string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn.Login))
            {
                if (!string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn.PinCode))
                {
                    SendMail.SendMessage($"Your PIN code is: {MainWindow.mainWindow.UserLogIn.PinCode}",
                                         MainWindow.mainWindow.UserLogIn.Login);
                    MessageBox.Show("PIN code sent to your email!");
                }
                else
                {
                    MessageBox.Show("No PIN code found. Please login with password.");
                    MainWindow.mainWindow.OpenPage(new Login());
                }
            }
            else
            {
                MessageBox.Show("Please login first.");
                MainWindow.mainWindow.OpenPage(new Login());
            }
        }
    }
    }
