using System.Text.RegularExpressions;

namespace SRTGenerator.RegularExpressions
{
    internal partial class Expressions
    {
        [GeneratedRegex("[^\\u0000-\\u007F]+")]
        public static partial Regex SanitizeName();

        [GeneratedRegex("^[0-9\\->:, ]+$")]
        public static partial Regex SRTTimming();
    }
}
