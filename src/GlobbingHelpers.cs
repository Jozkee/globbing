using System;
using System.Diagnostics;

namespace Globbing
{
    internal static class GlobbingHelpers
    {
        public static bool EndsWith(this ReadOnlySpan<char> span, char value)
        {
            Debug.Assert(span.Length > 0);
            return span[span.Length - 1] == value;
        }
    }
}
