using System;

namespace Xunit
{   
    public partial class Assert
    {
        public static void ContainsCount(int expected, string findText, string actual)
        {
            var cnt = 0;
            var idx = actual.IndexOf(findText, StringComparison.Ordinal);
            while (idx > -1)
            {
                cnt += 1;
                idx = actual.IndexOf(findText, idx + 1, StringComparison.Ordinal);
            }

            Equal(expected, cnt);
        }
    }
}