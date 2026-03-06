using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace QuanLyDoanKham.API.Helpers
{
    public class StringHelper
    {
        public static string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            result = result.Replace("Đ", "D").Replace("đ", "d");

            return result;
        }

        public static bool IsNumericOnly(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return Regex.IsMatch(text, @"^\d+$");
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return true;

            return Regex.IsMatch(phone, @"^0\d{9,10}$");
        }
    }
}
