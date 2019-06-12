using System.IO;

namespace PInvoke.Storage
{
    sealed class BinaryBulkReader : IBulkReader
    {
        private readonly BinaryReader reader;

        internal BinaryBulkReader(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public bool IsDone() => reader.PeekChar() == -1;

        public bool ReadBoolean() => reader.ReadBoolean();

        public int ReadInt32() => reader.ReadInt32();

        public void ReadItemEnd() { }

        public void ReadItemStart() { }

        public string ReadString() => reader.ReadBoolean() ? reader.ReadString() : null;
    }

}
