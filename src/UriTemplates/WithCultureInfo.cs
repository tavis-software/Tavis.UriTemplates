using System;
using System.Globalization;
#if NETSTANDARD1_0
#else
using System.Threading;
#endif

namespace Tavis.UriTemplates
{
    public sealed class WithCultureInfo : IDisposable
    {
        private readonly CultureInfo _oldCultureInfo;

        public WithCultureInfo(CultureInfo cultureInfo)
        {
            _oldCultureInfo = CultureInfo.CurrentCulture;

#if NETSTANDARD1_0
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
#else
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
#endif
        }

        public void Dispose()
        {
#if NETSTANDARD1_0
            CultureInfo.DefaultThreadCurrentCulture = _oldCultureInfo;
#else
            Thread.CurrentThread.CurrentUICulture = _oldCultureInfo;
#endif
        }
    }
}
