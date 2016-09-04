using PInvoke.Primitive.Bulk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvoke
{
    public static partial class StorageUtil
    {
        private sealed class BinaryBulkReader : IBulkReader
        {
            private readonly BinaryReader _reader;

            internal BinaryBulkReader(Stream stream)
            {
                _reader = new BinaryReader(stream);
            }

            public bool IsDone() => _reader.PeekChar() != -1;

            public bool ReadBoolean() => _reader.ReadBoolean();

            public int ReadInt32() => _reader.ReadInt32();

            public void ReadItemEnd() { }

            public void ReadItemStart() { }

            public string ReadString() => _reader.ReadBoolean() ? _reader.ReadString() : null;
        }

        private sealed class BinaryBulkWriter : IBulkWriter
        {
            private readonly BinaryWriter _writer;

            internal BinaryBulkWriter(Stream stream)
            {
                _writer = new BinaryWriter(stream);
            }

            public void WriteBoolean(bool b) => _writer.Write(b);

            public void WriteInt32(int i) => _writer.Write(i);

            public void WriteItemEnd() { }

            public void WriteItemStart() { }

            public void WriteString(string str)
            {
                if (str == null)
                {
                    _writer.Write(false);
                }
                else
                {
                    _writer.Write(true);
                    _writer.Write(str);
                }
            }
        }
    }
}
