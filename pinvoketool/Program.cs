using Mono.Options;
using PInvoke;
using PInvoke.NativeTypes;
using PInvoke.Transform;
using PInvoke.Transform.Enums;
using pinvoketool.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace pinvoketool
{

    class Program
    {
        static void Main(string[] args)
        {
            var libname = default(string);
            var filename = default(string);
            var outputFilename = default(string);
            var typesFilename = default(string);
            var namespaces = new List<string>();
            var targetNamespace = "GeneratedCode";

            var p = new OptionSet()
            {
                { "t|types=", "extra c/c++ types", x=> typesFilename = x  },
                { "f|file=", "the c/c++ header to parse", x=> filename = x  },
                { "o|output=", "file to create for output", x => outputFilename = x },
                { "l|library=",  "name of the exported library", x => libname = x },
                { "n|namespace=",  "name of the exported library", x =>
                    {
                        if(!string.IsNullOrEmpty(x))
                        {
                            targetNamespace = x;
                        }
                    }
                },
                { "i|imports=",  "imported namespaces to add in scope", x => 
                     {
                        if(!string.IsNullOrEmpty(x) && !namespaces.Contains(x))
                        {
                            namespaces.Add(x);
                        }
                    }
                },
            };

            var extra = default(List<string>);
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("An error happened:");
                Console.WriteLine(e.Message);
                return;
            }

            var cstring = string.Empty;
            if (File.Exists(filename))
            {
                using (var f = File.OpenText(filename))
                {
                    cstring = f.ReadToEnd();
                }
            }

            if (!string.IsNullOrEmpty(cstring))
            {
                try
                {
                    var storage = new BasicSymbolStorage();

                    if (!string.IsNullOrEmpty(typesFilename) && File.Exists(typesFilename))
                    {
                        try
                        {
                            var items = NativeType.Load(typesFilename);
                            foreach (var item in items)
                            {
                                if(!string.IsNullOrEmpty(item.Namespace) && !namespaces.Contains(item.Namespace))
                                {
                                    namespaces.Add(item.Namespace);
                                }

                                switch (item.Kind)
                                {
                                    case NativeTypeKind.Pointer:
                                        {
                                            storage.AddTypeDef(new NativeTypeDef(item.Name, new NativePointer(item.BuiltInType)));
                                            break;
                                        };

                                    case NativeTypeKind.BuiltIn:
                                        {
                                            storage.AddTypeDef(new NativeTypeDef(item.Name, new NativeBuiltinType(item.BuiltInType, item.IsUnsigned)));
                                            break;
                                        }

                                    case NativeTypeKind.Struct:
                                        {
                                            storage.AddDefinedType(new NativeStruct(item.Name));
                                            break;
                                        }
                                    case NativeTypeKind.Enum:
                                        {
                                            storage.AddDefinedType(new NativeEnum(item.Name));
                                            break;
                                        }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Write($"An error happened loading NativeType file \"{typesFilename}\":");
                            Console.WriteLine(e.Message);
                            return;
                        }
                    }

                    var basicConverter = new BasicConverter(LanguageType.CSharp, storage)
                    {
                        TransformKindFlags = TransformKindFlags.WrapperMethods,
                        Namespace = targetNamespace,
                        Imports = namespaces
                    };

                    var code = basicConverter.ConvertNativeCodeToPInvokeCode(cstring, libname);

                    if (string.IsNullOrEmpty(outputFilename))
                    {
                        Console.WriteLine(code);
                    }
                    else
                    {
                        if (File.Exists(outputFilename))
                        {
                            File.Delete(outputFilename);
                        }

                        using (var f = File.CreateText(outputFilename))
                        {
                            f.WriteLine(code);
                            f.Close();
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            else
            {
                Console.Write($"An error happened: file \"{filename ?? "<none>"}\" does not exist.");
            }
        }
    }
}
