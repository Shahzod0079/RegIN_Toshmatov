using System.Net;
using System.Net.Mail;

namespace RegIN_Toshmatov.Classes
{
    public class SendMail
    {
        public static void SendMessage(string Message, string To)
        {
            var smtpClient = new SmtpClient("smtp.yandex.ru")
            {
                Port = 587,
                Credentials = new NetworkCredential("yandex@yandex.ru", "zewvquznuxphtyvg"),
                EnableSsl = true,
            };
            smtpClient.Send("landaxer@yandex.ru", To, "Проект RegIn", Message);
        }
    }
}