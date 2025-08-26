using System;
using System.Globalization;
using DesktopApplicationTemplate.UI.Helpers;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ServiceTypeToIconConverterTests
    {
        [Theory]
        [InlineData("FTP", "📤")]
        [InlineData("Unknown", "❓")]
        public void Convert_ReturnsExpectedIcon(string serviceType, string expected)
        {
            var converter = new ServiceTypeToIconConverter();
            var result = converter.Convert(serviceType, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplemented()
        {
            var converter = new ServiceTypeToIconConverter();
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack("icon", typeof(string), null, CultureInfo.InvariantCulture));
        }
    }
}
