using PInvoke;
using PInvoke.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertLegacyFormat
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            var dataDirectory = @"..\..\..\StorageGenerator\Data";
            var oldFilePath = Path.Combine(dataDirectory, "windows.xml");
            var newFilePath = Path.Combine(dataDirectory, "windows.csv");

            Console.WriteLine("Reading old data");
            var nativeStorage = new NativeStorage();
            nativeStorage.ReadXml(oldFilePath);

            Console.WriteLine($"Converting to a {nameof(BasicSymbolStorage)}");
            var storage = new BasicSymbolStorage();
            foreach (var name in nativeStorage.NativeNames)
            {
                if (name.Kind == NativeNameKind.EnumValue)
                {
                    continue;
                }

                NativeGlobalSymbol symbol;
                if (!nativeStorage.TryGetGlobalSymbol(name, out symbol))
                {
                    Console.WriteLine($"Error: Unable to load {name.Name} {name.Kind}");
                    continue;
                }

                storage.Add(symbol);
            }

            Console.WriteLine("Saving new data");
            using (var stream = File.Open(newFilePath, FileMode.Create))
            {
                StorageUtil.WriteCsv(stream, storage);
            }

            Console.WriteLine("Loading data for sanity check");
            using (var stream = File.Open(newFilePath, FileMode.Open))
            {
                var testStorage = StorageUtil.ReadCsv(stream);
                if (testStorage.Count != storage.Count)
                {
                    Console.WriteLine("Error: Different count on load");
                }
            }
        }
    }
}
