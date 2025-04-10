using System.Configuration;
using EASendMail;
using GNAgeneraltools;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using GNA_CommercialLicenseValidator;




namespace EmailSMScheck
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var config = ConfigurationManager.AppSettings;
            var gnaT = new gnaTools();

            string strFreezeScreen = config["freezeScreen"] ?? string.Empty;
            string strProjectTitle = config["ProjectTitle"] ?? "Project";
            string strEmailLogin = config["EmailLogin"] ?? string.Empty;
            string strEmailPassword = config["EmailPassword"] ?? string.Empty;
            string strEmailFrom = config["EmailFrom"] ?? string.Empty;
            string strEmailRecipients = config["EmailRecipients"] ?? string.Empty;
            string licenseCode = config["LicenseCode"] ?? string.Empty;

            const string smtpHost = "smtp.gmail.com";
            const int smtpPort = 587;
            string strTab1 = "     ";
            string strTab2 = "        ";

            //==== Validate the EMLSMS license
            LicenseValidator.ValidateLicense("EMLSMS", licenseCode);


            Console.WriteLine("Software compiled 20250409\n");

            Console.WriteLine("\nTesting email:");
            string strSubjectLine = strProjectTitle + " test email";
            string strEmailBody = "System check email";
            bool emailSuccess = false;

            try
            {
                SmtpMail oMail = new("ES-E1646458156-01989-1A16E5275AF48A22-686E917424789B4F")
                {
                    From = strEmailFrom,
                    To = new AddressCollection(strEmailRecipients),
                    Subject = strSubjectLine,
                    TextBody = strEmailBody
                };

                SmtpServer oServer = new(smtpHost)
                {
                    User = strEmailLogin,
                    Password = strEmailPassword,
                    ConnectType = SmtpConnectType.ConnectTryTLS,
                    Port = smtpPort
                };

                SmtpClient oSmtp = new();
                oSmtp.SendMail(oServer, oMail);

                Console.WriteLine(strTab2 + "email sent...");
                emailSuccess = true;
            }
            catch (Exception ep)
            {
                Console.WriteLine("email failed...");
                Console.WriteLine(ep.Message);
                emailSuccess = false;
            }

            // Allocate recipient numbers
            var smsMobile = new string[9];
            for (int i = 0; i < smsMobile.Length; i++)
            {
                smsMobile[i] = config[$"RecipientPhone{i + 1}"] ?? string.Empty;
            }

            string strFullSMSmessage = emailSuccess
                ? strProjectTitle + ": email successful"
                : strProjectTitle + ": email failed";

            Console.WriteLine("\nTesting sms:");
            Console.WriteLine(strFullSMSmessage);

            bool smsSuccess = gnaT.sendSMSArray(strFullSMSmessage, smsMobile);
            Console.WriteLine(strTab1 + (smsSuccess ? "SMS sent" : "SMS failed"));

            string strRootFolder = "C:\\__SystemLogs\\";
            string strLogNote = string.Empty;

            if (emailSuccess)
                strLogNote += strProjectTitle+": email operational";
            else
                strLogNote += strProjectTitle + ": email failed";

            strLogNote += " | ";

            if (smsSuccess)
                strLogNote += "SMS operational";
            else
                strLogNote += "SMS failed";

            gnaT.updateSystemLogFile(strRootFolder, strLogNote);
            Console.WriteLine(strTab1 + "System log file updated");

            gnaT.freezeScreen(strFreezeScreen);
        }

    }
}