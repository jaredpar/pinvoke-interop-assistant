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
        private sealed class CsvBulkReader : IBulkReader
        {
            private readonly StreamReader _reader;
            private string[] _parts;
            private int _index;

            internal CsvBulkReader(Stream stream)
            {
                _reader = new StreamReader(stream);
                ReadLine();
            }

            private void ReadLine()
            {
                var line = _reader.ReadLine();
                _index = 0;
                _parts = line == null
                    ? null
                    : line.Split(new[] { ',' }, StringSplitOptions.None);
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
                return cur.Substring(1);
            }
        }

        private sealed class CsvBulkWriter : IBulkWriter
        {
            private readonly StreamWriter _writer;
            private readonly StringBuilder _builder = new StringBuilder();

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
                    _builder.Append(str);
                }
            }

            public void WriteEnd()
            {
                _writer.Flush();
            }
        }
    }
}
