﻿using Mono.Options;
using PInvoke;
using PInvoke.Transform;
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

            var p = new OptionSet()
            {
                { "t|types=", "extra c/c++ types", x=> typesFilename = x  },
                { "f|file=", "the c/c++ header to parse", x=> filename = x  },
                { "o|output=", "file to create for output", x => outputFilename = x },
                { "l|library=",  "name of the exported library", x => libname = x },
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
                            foreach(var item in items)
                            {
                                if(item.IsPointer)
                                {
                                    storage.AddTypeDef(new NativeTypeDef(item.Name, new NativePointer(item.BuiltInType)));
                                }
                                else
                                {
                                    storage.AddTypeDef(new NativeTypeDef(item.Name, new NativeBuiltinType(item.BuiltInType, item.IsUnsigned)));
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
                        TransformKindFlags = TransformKindFlags.All,
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