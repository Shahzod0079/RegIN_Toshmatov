using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;
using RegIN_Toshmatov.Classes;

namespace RegIN_Toshmatov.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }

        public byte[] Image = new byte[0];

        public DateTime DateTimeUpdate { get; set; }

        public DateTime DateTimeCreate { get; set; }

        public CorrectLogin HandlerCorrectLogin;

        public InCorrectLogin HandlerInCorrectLogin;

        public delegate void CorrectLogin();

        public delegate void InCorrectLogin();


        public void GetUserLogin(string Login)
        {
            this.Id = -1;
            this.Login = string.Empty;
            this.Password = string.Empty;
            this.Name = string.Empty;
            this.Image = new byte[0];
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                MySqlDataReader userQuery = WorkingDB.Query($"SELECT * FROM `users` WHERE `Login` = '{Login}'", mySqlConnection);
                if (userQuery.HasRows)
                {
                    userQuery.Read();
                    this.Id = userQuery.GetInt32(0);
                    this.Login = userQuery.GetString(1);
                    this.Password = userQuery.GetString(2);
                    this.Name = userQuery.GetString(3);
                    if (!userQuery.IsDBNull(4))
                    {
                        this.Image = new byte[64 * 1024];
                        userQuery.GetBytes(4, 0, Image, 0, Image.Length);
                    }
                    this.DateTimeUpdate = userQuery.GetDateTime(5);
                    this.DateTimeCreate = userQuery.GetDateTime(6);
                    HandlerCorrectLogin.Invoke();
                }
                else
                {
                    HandlerInCorrectLogin.Invoke();
                }
            }
            else
            {
                HandlerInCorrectLogin.Invoke();
            }
            WorkingDB.CloseConnection(mySqlConnection);
        }
        public void SetUser()
        {
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                MySqlCommand mySqlCommand = new MySqlCommand("INSERT INTO `users`(`Login`, `Password`, `Name`, `Image`, `DateUpdate`, `DateCreate`) VALUES (@Login, @Password, @Name, @Image, @DateUpdate, @DateCreate); SELECT LAST_INSERT_ID();", mySqlConnection);

                mySqlCommand.Parameters.AddWithValue("@Login", this.Login);
                mySqlCommand.Parameters.AddWithValue("@Password", this.Password);
                mySqlCommand.Parameters.AddWithValue("@Name", this.Name);
                mySqlCommand.Parameters.AddWithValue("@Image", this.Image);
                mySqlCommand.Parameters.AddWithValue("@DateUpdate", this.DateTimeUpdate);
                mySqlCommand.Parameters.AddWithValue("@DateCreate", this.DateTimeCreate);

                // Получаем ID нового пользователя
                this.Id = Convert.ToInt32(mySqlCommand.ExecuteScalar());
            }

            WorkingDB.CloseConnection(mySqlConnection);
        }

        public void CreateNewPassword()
        {
            if (Login != String.Empty)
            {
                string newPassword = GeneratePass();

                MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

                if (WorkingDB.OpenConnection(mySqlConnection))
                {
                    WorkingDB.Query($"UPDATE `users` SET `Password`='{newPassword}' WHERE `Login`='{this.Login}'", mySqlConnection);
                    WorkingDB.CloseConnection(mySqlConnection);
                    SendMail.SendMessage($"Your account password has been changed.\nNew password: {newPassword}", this.Login);
                }
            }
        }

        private string GeneratePass()
        {
            List<char> NewPassword = new List<char>();
            Random rnd = new Random();

            char[] ArrNumbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] ArrSymbols = { '@', '#', '%', '&', '*', '_', '+', '(', ')', '[', ']', '{', '}', '|', '\\', '^', '~' };
            char[] ArrUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] ArrLowercase = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

            NewPassword.Add(ArrNumbers[rnd.Next(0, ArrNumbers.Length)]);

            NewPassword.Add(ArrSymbols[rnd.Next(0, ArrSymbols.Length)]);

            for (int k = 0; k < 2; k++)
            {
                NewPassword.Add(ArrUppercase[rnd.Next(0, ArrUppercase.Length)]);
            }

            for (int l = 0; l < 6; l++)
            {
                NewPassword.Add(ArrLowercase[rnd.Next(0, ArrLowercase.Length)]);
            }

            for (int i = 0; i < NewPassword.Count; i++)
            {
                int randomIndex = rnd.Next(0, NewPassword.Count);
                char temp = NewPassword[i];
                NewPassword[i] = NewPassword[randomIndex];
                NewPassword[randomIndex] = temp;
            }

            string NPASSWORD = "";
            foreach (char c in NewPassword)
            {
                NPASSWORD += c;
            }

            return NPASSWORD;
        }
        public string PinCode { get; set; }

        public void SavePinCode(string pinCode)
        {
            // Проверка ДО сохранения!
            if (this.Id <= 0)
            {
                MessageBox.Show("Ошибка: ID пользователя не загружен!");
                return;
            }

            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                // Деактивируем старые пинкоды
                MySqlCommand deactivateCmd = new MySqlCommand("UPDATE pincodes SET IsActive = FALSE WHERE UserId = @UserId", mySqlConnection);
                deactivateCmd.Parameters.AddWithValue("@UserId", this.Id);
                deactivateCmd.ExecuteNonQuery();

                // Сохраняем новый пинкод
                MySqlCommand cmd = new MySqlCommand("INSERT INTO pincodes (UserId, PinCode) VALUES (@UserId, @PinCode)", mySqlConnection);
                cmd.Parameters.AddWithValue("@UserId", this.Id);
                cmd.Parameters.AddWithValue("@PinCode", pinCode);
                cmd.ExecuteNonQuery();

                this.PinCode = pinCode;
            }
            WorkingDB.CloseConnection(mySqlConnection);
        }


        public bool CheckPinCode(string pinCode)
        {
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
            bool result = false;

            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM pincodes WHERE UserId = @UserId AND PinCode = @PinCode AND IsActive = TRUE", mySqlConnection);
                cmd.Parameters.AddWithValue("@UserId", this.Id);
                cmd.Parameters.AddWithValue("@PinCode", pinCode);
                result = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            WorkingDB.CloseConnection(mySqlConnection);
            return result;
        }
    }
}