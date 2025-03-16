using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace WpfVcardEditor
{
    public class VCard
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public BitmapImage Photo { get; set; }

        public string Company { get; set; }
        public string JobTitle { get; set; }
        public string WorkEmail { get; set; }
        public string WorkPhone { get; set; }

        public string LinkedIn { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string YouTube { get; set; }

        public VCard()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Gender = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Company = string.Empty;
            JobTitle = string.Empty;
            WorkEmail = string.Empty;
            WorkPhone = string.Empty;
            LinkedIn = string.Empty;
            Facebook = string.Empty;
            Instagram = string.Empty;
            YouTube = string.Empty;
        }

        public VCard(
            string firstName, 
            string lastName, 
            DateTime? birthDay = null, 
            string gender = "", 
            string email = "", 
            string phone = "", 
            BitmapImage photo = null)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDay = birthDay;
            Gender = gender;
            Email = email;
            Phone = phone;
            Photo = photo;
            Company = string.Empty;
            JobTitle = string.Empty;
            WorkEmail = string.Empty;
            WorkPhone = string.Empty;
            LinkedIn = string.Empty;
            Facebook = string.Empty;
            Instagram = string.Empty;
            YouTube = string.Empty;
        }

        private string ImageToBase64(BitmapImage imageSource)
        {
            if (imageSource == null)
                return string.Empty;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public string GenerateVcfCode()
        {
            StringBuilder vCard = new StringBuilder();
            vCard.AppendLine("BEGIN:VCARD");
            vCard.AppendLine("VERSION:3.0");

            if (!string.IsNullOrEmpty(LastName) || !string.IsNullOrEmpty(FirstName))
            {
                vCard.AppendLine($"N;CHARSET=UTF-8:{LastName};{FirstName}");
            }

            if (BirthDay.HasValue)
            {
                string birthday = BirthDay.Value.ToString("yyyyMMdd");
                vCard.AppendLine($"BDAY:{birthday}");
            }

            if (!string.IsNullOrEmpty(Gender))
            {
                vCard.AppendLine($"GENDER:{Gender}");
            }

            if (!string.IsNullOrEmpty(Email))
            {
                vCard.AppendLine($"EMAIL;CHARSET=UTF-8;type=WORK,INTERNET:{Email}");
            }

            if (!string.IsNullOrEmpty(Phone))
            {
                vCard.AppendLine($"TEL;TYPE=HOME,VOICE:{Phone}");
            }

            string base64Image = ImageToBase64(Photo);
            if (!string.IsNullOrEmpty(base64Image))
            {
                vCard.AppendLine($"PHOTO;ENCODING=BASE64;TYPE=image/jpeg:{base64Image}");
            }

            if (!string.IsNullOrEmpty(Company))
            {
                vCard.AppendLine($"ORG:{Company}");
            }

            if (!string.IsNullOrEmpty(JobTitle))
            {
                vCard.AppendLine($"TITLE:{JobTitle}");
            }

            if (!string.IsNullOrEmpty(WorkEmail))
            {
                vCard.AppendLine($"EMAIL;CHARSET=UTF-8;type=WORK,INTERNET:{WorkEmail}");
            }

            if (!string.IsNullOrEmpty(WorkPhone))
            {
                vCard.AppendLine($"TEL;TYPE=WORK,VOICE:{WorkPhone}");
            }

            if (!string.IsNullOrEmpty(LinkedIn))
            {
                vCard.AppendLine($"URL;TYPE=LinkedIn:{LinkedIn}");
            }

            if (!string.IsNullOrEmpty(Facebook))
            {
                vCard.AppendLine($"URL;TYPE=Facebook:{Facebook}");
            }

            if (!string.IsNullOrEmpty(Instagram))
            {
                vCard.AppendLine($"URL;TYPE=Instagram:{Instagram}");
            }

            if (!string.IsNullOrEmpty(YouTube))
            {
                vCard.AppendLine($"URL;TYPE=YouTube:{YouTube}");
            }

            vCard.AppendLine("END:VCARD");
            return vCard.ToString();
        }
    }
} 