using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace xbridge.Util
{
    public static class Util
    {

        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }
        public static Android.Net.Uri ResourceUriFromPath(string path)
        {
            while (path.StartsWith("/", StringComparison.Ordinal))
                path = path.Substring(1);
            var uri = "file:///android_asset/" + path;
            return Android.Net.Uri.Parse(uri);
        }

        public static Android.Net.Uri ValidateResourceUri(string url)
        {
            if (url.StartsWith("resource:", StringComparison.Ordinal))
                url = "file:///android_asset/" + url.Substring(9);
            else if (!url.StartsWith("file:///android_asset/", StringComparison.Ordinal))
                throw new Exception("not a resource url");
            return Android.Net.Uri.Parse(url);
        }


    }

}
