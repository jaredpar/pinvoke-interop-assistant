using System;
using System.Diagnostics;
using System.IO;

namespace PInvoke.Storage
{
    sealed class CsvBulkReader : IBulkReader
    {
        private readonly StreamReader reader;
        private string[] parts;
        private int index;
        private int lineNumber;

        internal CsvBulkReader(Stream stream)
        {
            reader = new StreamReader(stream);
            lineNumber = -1;
            ReadLine();
        }

        private void ReadLine()
        {
            var line = reader.ReadLine();
            index = 0;
            parts = line?.Split(new[] { ',' }, StringSplitOptions.None);
            lineNumber++;
        }

        public bool IsDone() => parts == null;

        public bool ReadBoolean()
        {
            Debug.Assert(index < parts.Length);
            return bool.Parse(parts[index++]);
        }

        public int ReadInt32()
        {
            Debug.Assert(index < parts.Length);
            return int.Parse(parts[index++]);
        }

        public void ReadItemEnd()
        {
            Debug.Assert(index == parts.Length);
            ReadLine();
        }

        public void ReadItemStart()
        {
            Debug.Assert(index == 0);
        }

        public string ReadString()
        {
            var cur = parts[index++];
            if (cur == "0")
            {
                return null;
            }

            return CsvUtil.UnescapeString(cur.Substring(1));
        }
    }
}
