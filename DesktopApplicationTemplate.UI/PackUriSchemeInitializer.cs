using System;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI
{
    /// <summary>
    /// Ensures the WPF pack URI scheme is registered so resource dictionaries
    /// can be loaded in unit tests without a running application.
    /// </summary>
    internal static class PackUriSchemeInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            if (!UriParser.IsKnownScheme("pack"))
            {
                UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "pack", -1);
            }
        }
    }
}
