using System.Net;
using System.Text.RegularExpressions;

namespace CinemaBooking.Application.Common
{
    public static class InputGuard
    {
        private static readonly Regex Tags = new("<[^>]*>", RegexOptions.Compiled);

        public static string Plain(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var text = Tags.Replace(value, "").Trim();
            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            return text;
        }

        public static string? Optional(string? value, int maxLength)
        {
            var text = Plain(value, maxLength);
            return text.Length == 0 ? null : text;
        }

        public static string? MediaUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var text = value.Trim();
            if (text.Length > 500) return null;

            if (text.StartsWith("/uploads/", StringComparison.Ordinal)
                && !text.Contains("..", StringComparison.Ordinal)
                && text.IndexOfAny(new[] { '\\', ' ', '?' }) < 0)
                return text;

            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) return null;
            if (IPAddress.TryParse(uri.Host, out _)) return null;

            var host = uri.IdnHost.ToLowerInvariant();
            if (host is "localhost" or "127.0.0.1" or "0.0.0.0" || host.EndsWith(".local"))
                return null;

            return text;
        }

        public static string? HexColor(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var text = value.Trim();
            return Regex.IsMatch(text, "^#[0-9A-Fa-f]{6}$") ? text : null;
        }
    }
}
