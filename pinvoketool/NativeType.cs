using PInvoke;
using pinvoketool.Enums;
using System.IO;
using System.Xml.Serialization;

namespace pinvoketool
{

    public class NativeType
    {
        public NativeType()
        {
            
        }

        public string Name { get; set; }
        public string Namespace { get; set; }
        public NativeTypeKind Kind { get; set; } = NativeTypeKind.BuiltIn;
        public bool IsUnsigned { get; set; }

        public BuiltinType BuiltInType { get; set; }

        public static NativeType[] Load(string fileName)
        {
            var result = default(NativeType[]);

            var serializer = new XmlSerializer(typeof(NativeType[]));
            using (var file = File.OpenText(fileName))
            {
                result = (NativeType[])serializer.Deserialize(file);
            }

            return result;
        }

        public static void Save(string fileName, NativeType[] data)
        {
            var serializer = new XmlSerializer(typeof(NativeType[]));
            using (var file = File.CreateText(fileName))
            {
                serializer.Serialize(file, data);
                file.Flush();
                file.Close();
            }
        }
    }
}
