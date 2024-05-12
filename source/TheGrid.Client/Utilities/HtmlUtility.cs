using System.Text;

namespace TheGrid.Client.Utilities
{
    public static class HtmlUtility
    {
        public static string GetSafeId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            input = input.ToLowerInvariant();

            var sb = new StringBuilder(input.Length);
            var startIndex = 0;

            if (!char.IsLetter(input[0]))
            {
                if (char.IsLetter(input[1]))
                {
                    startIndex = 1;
                }
                else
                {
                    startIndex = 2;
                }
            }

            for (var i = startIndex; i < input.Length; i++)
            {
                var c = input[i];

                if (char.IsLetter(c) || char.IsNumber(c) || c == '-' || c == '_')
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    sb.Append('-');
                }
            }

            return sb.ToString();
        }
    }
}
