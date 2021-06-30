using System;

namespace MuGet
{
    public static class AppCenterEvents
    {
        public static class Error
        {
            public static readonly string PackageLoadFailed = "package_load_failed";
        }

        public static class Property
        {
            public static readonly string Key = "key";
        }
    }
}