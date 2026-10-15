using System.Globalization;
using System.Text;

namespace CinemaBooking.Application.Common
{
    public static class VietQr
    {
        public static string Payload(string bankBin, string accountNumber, string accountName, decimal amount, string description)
        {
            var merchantAccount = Tlv("00", "A000000727")
                + Tlv("01", Tlv("00", Digits(bankBin)) + Tlv("01", Digits(accountNumber)))
                + Tlv("02", "QRIBFTTA");

            var body = Tlv("00", "01")
                + Tlv("01", "12")
                + Tlv("38", merchantAccount)
                + Tlv("53", "704")
                + Tlv("54", decimal.Truncate(amount).ToString("0", CultureInfo.InvariantCulture))
                + Tlv("58", "VN")
                + Tlv("59", Ascii(accountName, 25))
                + Tlv("60", "HANOI")
                + Tlv("62", Tlv("08", Ascii(description, 25)))
                + "6304";

            return body + Crc16(body);
        }

        public static string ImageUrl(string payload, int size = 280)
        {
            return "https://api.qrserver.com/v1/create-qr-code/?size="
                + size + "x" + size
                + "&data=" + Uri.EscapeDataString(payload);
        }

        private static string Tlv(string id, string value)
        {
            return id + value.Length.ToString("00", CultureInfo.InvariantCulture) + value;
        }

        private static string Digits(string value)
        {
            var chars = (value ?? "").Where(char.IsDigit).ToArray();
            return chars.Length == 0 ? "0" : new string(chars);
        }

        private static string Ascii(string value, int max)
        {
            var text = (value ?? "").ToUpperInvariant();
            var buffer = new StringBuilder();
            foreach (var ch in text)
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == ' ')
                    buffer.Append(ch);
            }
            var clean = buffer.ToString().Trim();
            if (clean.Length == 0) clean = "CINEMA";
            return clean.Length <= max ? clean : clean.Substring(0, max);
        }

        private static string Crc16(string value)
        {
            ushort crc = 0xFFFF;
            foreach (var ch in value)
            {
                crc ^= (ushort)(ch << 8);
                for (var i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc.ToString("X4", CultureInfo.InvariantCulture);
        }
    }
}
