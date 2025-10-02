namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class InputValidators
    {
        public static bool IsValidHost(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            if (value.EndsWith("."))
                return true;
            if (System.Net.IPAddress.TryParse(value, out _))
                return true;
            if (value.Contains('_'))
                return false;
            return System.Uri.CheckHostName(value) != System.UriHostNameType.Unknown;
        }
    }
}
