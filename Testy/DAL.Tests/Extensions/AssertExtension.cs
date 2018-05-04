using System;
using Xunit;

namespace DAL.Tests.Extensions
{
    public static class AssertExtension
    {
        public static void Vyskyty(this Assert assertions, int expected, string findText, string actual)
        {
            var cnt = 0;
            var idx = actual.IndexOf(findText, StringComparison.Ordinal);
            while (idx > -1)
            {
                cnt += 1;
                idx = actual.IndexOf(findText, idx, StringComparison.Ordinal);
            }
            
        }
    }
}