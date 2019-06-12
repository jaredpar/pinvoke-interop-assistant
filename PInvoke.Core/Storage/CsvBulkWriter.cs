using System.Diagnostics;
using System.IO;
using System.Text;

namespace PInvoke.Storage
{
    sealed class CsvBulkWriter : IBulkWriter
    {
        private readonly StreamWriter writer;
        private readonly StringBuilder builder = new StringBuilder();
        private int lineNumber;

        internal CsvBulkWriter(Stream stream)
        {
            writer = new StreamWriter(stream);
        }

        internal void WriteDone()
        {
            writer.Flush();
        }

        private void MaybeAddComma()
        {
            if (builder.Length > 0)
            {
                builder.Append(',');
            }
        }

        public void WriteBoolean(bool b)
        {
            MaybeAddComma();
            builder.Append(b ? "true" : "false");
        }

        public void WriteInt32(int i)
        {
            MaybeAddComma();
            builder.Append(i);
        }

        public void WriteItemEnd()
        {
            writer.WriteLine(builder.ToString());
            builder.Length = 0;
            lineNumber++;
        }

        public void WriteItemStart()
        {
            Debug.Assert(builder.Length == 0);
        }

        public void WriteString(string str)
        {
            MaybeAddComma();
            if (str == null)
            {
                builder.Append("0");
            }
            else
            {
                builder.Append("1");
                builder.Append(CsvUtil.EscapeString(str));
            }
        }

        public void WriteEnd()
        {
            writer.Flush();
        }
    }
}
