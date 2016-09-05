using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke.Storage
{
    public static partial class StorageUtil
    {
        private static class CsvUtil
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

        private sealed class CsvBulkReader : IBulkReader
        {
            private readonly StreamReader _reader;
            private string[] _parts;
            private int _index;
            private int _lineNumber;

            internal CsvBulkReader(Stream stream)
            {
                _reader = new StreamReader(stream);
                _lineNumber = -1;
                ReadLine();
            }

            private void ReadLine()
            {
                var line = _reader.ReadLine();
                _index = 0;
                _parts = line == null
                    ? null
                    : line.Split(new[] { ',' }, StringSplitOptions.None);
                _lineNumber++;
            }

            public bool IsDone() => _parts == null;

            public bool ReadBoolean()
            {
                Debug.Assert(_index < _parts.Length);
                return bool.Parse(_parts[_index++]);
            }

            public int ReadInt32()
            {
                Debug.Assert(_index < _parts.Length);
                return int.Parse(_parts[_index++]);
            }

            public void ReadItemEnd()
            {
                Debug.Assert(_index == _parts.Length);
                ReadLine();
            }

            public void ReadItemStart()
            {
                Debug.Assert(_index == 0);
            }

            public string ReadString()
            {
                var cur = _parts[_index++];
                if (cur == "0")
                {
                    return null;
                }

                return CsvUtil.UnescapeString(cur.Substring(1));
            }
        }

        private sealed class CsvBulkWriter : IBulkWriter
        {
            private readonly StreamWriter _writer;
            private readonly StringBuilder _builder = new StringBuilder();
            private int _lineNumber;

            internal CsvBulkWriter(Stream stream)
            {
                _writer = new StreamWriter(stream);
            }

            internal void WriteDone()
            {
                _writer.Flush();
            }

            private void MaybeAddComma()
            {
                if (_builder.Length > 0)
                {
                    _builder.Append(',');
                }
            }

            public void WriteBoolean(bool b)
            {
                MaybeAddComma();
                _builder.Append(b ? "true" : "false");
            }

            public void WriteInt32(int i)
            {
                MaybeAddComma();
                _builder.Append(i);
            }

            public void WriteItemEnd()
            {
                _writer.WriteLine(_builder.ToString());
                _builder.Length = 0;
                _lineNumber++;
            }

            public void WriteItemStart()
            {
                Debug.Assert(_builder.Length == 0);
            }

            public void WriteString(string str)
            {
                MaybeAddComma();
                if (str == null)
                {
                    _builder.Append("0");
                }
                else
                {
                    _builder.Append("1");
                    _builder.Append(CsvUtil.EscapeString(str));
                }
            }

            public void WriteEnd()
            {
                _writer.Flush();
            }
        }
    }
}
