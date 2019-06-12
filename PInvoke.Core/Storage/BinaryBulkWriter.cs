using System.IO;

namespace PInvoke.Storage
{
    sealed class BinaryBulkWriter : IBulkWriter
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
