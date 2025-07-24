namespace DesktopApplicationTemplate.UI.Helpers
{
    public static class InputValidators
    {
        public static bool IsValidPartialIp(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            if (value.EndsWith("."))
                return true;
            return System.Net.IPAddress.TryParse(value, out _);
        }
    }
}
