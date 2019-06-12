using System.Diagnostics;
using System.Text;

namespace PInvoke.Storage
{
    static class CsvUtil
    {
        internal static string EscapeString(string str)
        {
            var builder = new StringBuilder(str.Length);
            foreach (var c in str)
            {
                switch (c)
                {
                    case ',':
                        builder.Append('#');
                        break;
                    case '#':
                        builder.Append(@"\#");
                        break;
                    case '\r':
                        builder.Append(@"\r");
                        break;
                    case '\n':
                        builder.Append(@"\n");
                        break;
                    case '\\':
                        builder.Append(@"\\");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }

        internal static string UnescapeString(string str)
        {
            var builder = new StringBuilder(str.Length);
            var i = 0;
            while (i < str.Length)
            {
                var c = str[i];
                if (c == '\\' && i + 1 < str.Length)
                {
                    var n = str[i + 1];
                    switch (n)
                    {
                        case '#':
                            builder.Append('#');
                            break;
                        case '\\':
                            builder.Append('\\');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                    i += 2;
                }
                else if (c == '#')
                {
                    builder.Append(',');
                    i++;
                }
                else
                {
                    builder.Append(c);
                    i++;
                }
            }

            return builder.ToString();
        }
    }
}
