// Copyright (c) Microsoft Corporation.  All rights reserved.

using PInvoke.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace PInvoke.Test
{
    public class PreProcessorEngineTest
    {

        private void VerifyCount(string text, int errorCount, int warningCount)
        {
            PreProcessorEngine p = new PreProcessorEngine(new PreProcessorOptions());
            p.Process(new TextReaderBag("text", new StringReader(text)));
            Assert.Equal(errorCount, p.ErrorProvider.Errors.Count);
            Assert.Equal(warningCount, p.ErrorProvider.Warnings.Count);
        }

        private Dictionary<string, Macro> VerifyImpl(PreProcessorOptions opts, string before, string after)
        {
            PreProcessorEngine p = new PreProcessorEngine(opts);
            string actual = p.Process(new TextReaderBag("before", new StringReader(before)));
            Assert.Equal(after, actual);

            return p.MacroMap;
        }

        private Dictionary<string, Macro> VerifyNormal(string before, string after)
        {
            return VerifyNormal(new List<Macro>(), before, after);
        }

        private Dictionary<string, Macro> VerifyNormal(List<Macro> initialList, string before, string after)
        {
            PreProcessorOptions opts = new PreProcessorOptions();
            opts.InitialMacroList.AddRange(initialList);
            opts.FollowIncludes = false;
            return VerifyImpl(opts, before, after);
        }

        private Dictionary<string, Macro> VerifyNoMetadata(string before, string after)
        {
            PreProcessorOptions opts = new PreProcessorOptions();
            opts.FollowIncludes = true;
            return VerifyImpl(opts, before, after);
        }

        private void VerifyMap(Dictionary<string, Macro> map, params string[] args)
        {
            for (int i = 0; i <= args.Length - 1; i += 2)
            {
                string name = args[i];
                string value = args[i + 1];
                Assert.True(map.ContainsKey(name), "Could not find " + name + " in the macro map");

                Macro macro = map[name];
                Assert.Equal(value, macro.Value);
            }
        }

        private Dictionary<string, Macro> VerifyMacro(string data, params string[] args)
        {
            PreProcessorOptions opts = new PreProcessorOptions();
            opts.FollowIncludes = false;
            PreProcessorEngine p = new PreProcessorEngine(opts);
            p.Process(new TextReaderBag("foo", new StringReader(data)));
            VerifyMap(p.MacroMap, args);
            return p.MacroMap;
        }

        private bool EvalCond(List<Macro> list, string cond)
        {
            string before = "#if " + cond + Environment.NewLine + "true" + Environment.NewLine + "#else" + Environment.NewLine + "false" + Environment.NewLine + "#endif";
            PreProcessorOptions opts = new PreProcessorOptions();
            opts.InitialMacroList.AddRange(list);
            PreProcessorEngine engine = new PreProcessorEngine(opts);
            string val = engine.Process(new TextReaderBag("foo", new StringReader(before)));
            return val.StartsWith("true");
        }

        private void VerifyCondTrue(List<Macro> list, string cond)
        {
            Assert.True(EvalCond(list, cond));
        }

        private void VerifyCondFalse(List<Macro> list, string cond)
        {
            Assert.False(EvalCond(list, cond));
        }

        [Fact()]
        public void PoundInclude1()
        {
            string before = @"#include ""non_existent_file.h""";
            string after = "";
            var opts = new PreProcessorOptions() { FollowIncludes = true };
            var map = VerifyImpl(opts, before, after);

            Assert.True(map.Count == 0); // include not found
        }

        [Fact()]
        public void Conditional1()
        {
            string before = "#define foo bar" + Environment.NewLine + "#if foo" + Environment.NewLine + "hello" + Environment.NewLine + "#endif";
            string after = "hello" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar");
        }

        [Fact()]
        public void Conditional2()
        {
            string before = "#define foo bar" + Environment.NewLine + "#if foo" + Environment.NewLine + "hello" + Environment.NewLine + "world" + Environment.NewLine + "#endif";
            string after = "hello" + Environment.NewLine + "world" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar");
        }

        [Fact()]
        public void Conditional3()
        {
            string before = "#define foo bar" + Environment.NewLine + "#if foo" + Environment.NewLine + "hello" + Environment.NewLine + "#else" + Environment.NewLine + "world" + Environment.NewLine + "#endif";
            string after = "hello" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar");
        }

        /// <summary>
        /// Hit the else clause
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Conditional4()
        {
            string before = "#if foo" + Environment.NewLine + "hello" + Environment.NewLine + "#else" + Environment.NewLine + "world" + Environment.NewLine + "#endif";
            string after = "world" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
        }

        /// <summary>
        /// Hit the #elseif
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Conditional5()
        {
            string before = "#define foo bar" + Environment.NewLine + "#if bar" + Environment.NewLine + "hello" + Environment.NewLine + "#elseif foo" + Environment.NewLine + "world" + Environment.NewLine + "#endif";
            string after = "world" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar");
        }

        /// <summary>
        /// Skip the else when the #elseif is hit
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Conditional6()
        {
            string before = "#define foo bar" + Environment.NewLine + "#if bar " + Environment.NewLine + "hello" + Environment.NewLine + "#elseif foo" + Environment.NewLine + "world" + Environment.NewLine + "#else" + Environment.NewLine + "again" + Environment.NewLine + "#endif";
            string after = "world" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar");
        }

        [Fact()]
        public void Conditional7()
        {
            string before = "#define _PREFAST_" + Environment.NewLine + "#if !(defined(_midl)) && defined(_PREFAST_)" + Environment.NewLine + "hello" + Environment.NewLine + "#endif";
            string after = "hello" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// make sure that we collapse #     define
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Conditional8()
        {
            string before = "#     define foo bar" + Environment.NewLine + "#     if bar" + Environment.NewLine + "hello" + Environment.NewLine + "#    elseif foo" + Environment.NewLine + "world" + Environment.NewLine + "#    endif";
            string after = "world" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar");
        }

        [Fact()]
        public void Conditional9()
        {
            string before = "#     define foo bar" + Environment.NewLine + "#     if defined foo " + Environment.NewLine + "hello" + Environment.NewLine + "#    else " + Environment.NewLine + "world" + Environment.NewLine + "#    endif";
            string after = "hello" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar");
        }

        [Fact()]
        public void Conditional10()
        {
            string before = "#define FOO 1" + Environment.NewLine + "#if FOO & 1" + Environment.NewLine + "hello" + Environment.NewLine + "#endif";
            string after = "hello" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Conditional11()
        {
            string before = "#define FOO 1" + Environment.NewLine + "#if FOO & 2" + Environment.NewLine + "hello" + Environment.NewLine + "#endif";
            string after = string.Empty;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Conditional12()
        {
            string before = "#define FOO 1" + Environment.NewLine + "#if FOO | 2" + Environment.NewLine + "hello" + Environment.NewLine + "#endif";
            string after = "hello" + Environment.NewLine;
            VerifyNormal(before, after);
        }


        /// <summary>
        /// Simple multiline macro.  
        /// 
        /// The hello line will end with a newline because it's the last line in the file.
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Multiline1()
        {
            string before = "#define foo bar \\" + Environment.NewLine + "baz" + Environment.NewLine + "hello";
            string after = "hello" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar baz");
        }


        [Fact()]
        public void Multiline2()
        {
            string before = "#define foo bar \\" + Environment.NewLine + "baz \\" + Environment.NewLine + "again " + Environment.NewLine + "hello";
            string after = "hello" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar baz again");
        }

        [Fact()]
        public void Multiline3()
        {
            string before = "#define foo bar \\" + Environment.NewLine + "baz /*foo*/\\" + Environment.NewLine + " " + Environment.NewLine + "hello";
            string after = "hello" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar baz");
        }

        [Fact()]
        public void Multiline4()
        {
            string before = "#define foo bar \\" + Environment.NewLine + "baz /*foo*/\\" + Environment.NewLine + " // hello " + Environment.NewLine + "hello";
            string after = "hello" + Environment.NewLine;
            Dictionary<string, Macro> map = default(Dictionary<string, Macro>);
            map = VerifyNormal(before, after);
            VerifyMap(map, "foo", "bar baz");
        }

        /// <summary>
        /// Preprocessor should remove comments
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Comment1()
        {
            string before = "/* hello */" + Environment.NewLine + "world" + Environment.NewLine;
            string after = Environment.NewLine + "world" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Comment2()
        {
            string before = "hello" + Environment.NewLine + "/* hello */" + Environment.NewLine + "world" + Environment.NewLine;
            string after = "hello" + Environment.NewLine + Environment.NewLine + "world" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Comment3()
        {
            string before = "// hello */" + Environment.NewLine + "world" + Environment.NewLine;
            string after = Environment.NewLine + "world" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Make sure the preprocessor won't ignore an entire line when it hits
        /// a comment
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Comment4()
        {
            string before = "/* hello */ hello" + Environment.NewLine + "world" + Environment.NewLine;
            string after = " hello" + Environment.NewLine + "world" + Environment.NewLine;
            VerifyNormal(before, after);
        }
        /// <summary>
        /// Parse out simple macros
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Macro1()
        {
            string data = "#define foo bar" + Environment.NewLine + "#define bar foo";
            VerifyMacro(data, "foo", "bar", "bar", "foo");
        }

        /// <summary>
        /// Comment in the value
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Macro2()
        {
            string data = "#define foo /* hello */bar" + Environment.NewLine + "#define bar foo";
            VerifyMacro(data, "foo", "bar", "bar", "foo");
        }

        /// <summary>
        /// Undef the macro
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Macro3()
        {
            string data = "#define foo bar" + Environment.NewLine + "#undef foo";
            Dictionary<string, Macro> map = VerifyMacro(data);
            Assert.Equal(0, map.Count);
        }

        [Fact()]
        public void Macro4()
        {
            string data = "#define foo bar" + Environment.NewLine + "#define /* hollow */ bar foo";
            VerifyMacro(data, "foo", "bar", "bar", "foo");
        }

        /// <summary>
        /// Make sure that if the values are just wrapped in a set of () that we don't 
        /// treat it as a macro method
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Macro5()
        {
            string before = "#define foo   (1)" + Environment.NewLine + "foo" + Environment.NewLine;
            string after = "(1)" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Macro6()
        {
            string before = "#define foo   ((2)1)" + Environment.NewLine + "foo" + Environment.NewLine;
            string after = "((2)1)" + Environment.NewLine;
            VerifyNormal(before, after);
        }


        [Fact()]
        public void Eval1()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("foo", "bar")
            };
            VerifyCondTrue(list, "foo");
        }

        [Fact()]
        public void Eval2()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondFalse(list, "foo");
        }

        /// <summary>
        /// Test the "defined" function
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Eval3()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondTrue(list, "defined(m1)");
        }

        /// <summary>
        /// Add some parens
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Eval4()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondTrue(list, "(m1)");
        }

        /// <summary>
        /// add some ||'s 
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Eval5()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondTrue(list, "foo || m1");
            VerifyCondFalse(list, "foo  || bar");
        }

        /// <summary>
        /// Test some and's
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Eval6()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondTrue(list, "m1 && m2");
            VerifyCondFalse(list, "foo && m2");
            VerifyCondTrue(list, "m1 && 1");
        }

        /// <summary>
        /// Complex ones
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Eval7()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondTrue(list, "defined(m1) || ab");
            VerifyCondFalse(list, "((m1) || m2) && foo");
        }

        [Fact()]
        public void Eval8()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondFalse(list, "defined(m5)");
        }

        [Fact()]
        public void Eval9()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "true"),
                new Macro("m2", "false"),
                new Macro("m3", "1"),
                new Macro("m4", "0")
            };
            VerifyCondTrue(list, "!defined(m5)");
        }

        /// <summary>
        /// Relational evaluation
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Eval10()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "5"),
                new Macro("m2", "6")
            };
            VerifyCondTrue(list, "m2 > m1");
            VerifyCondTrue(list, "m2 >= m1");
        }

        /// <summary>
        /// Relational operators with hex numbers
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Eval11()
        {
            List<Macro> list = new List<Macro>
            {
                new Macro("m1", "0x5"),
                new Macro("m2", "0x6")
            };
            VerifyCondTrue(list, "m2 > m1");
            VerifyCondTrue(list, "m2 >= m1");
        }

        /// <summary>
        /// Make sure we're replacing defined tokens
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Replace1()
        {
            string before = "#define foo bar" + Environment.NewLine + "foo" + Environment.NewLine;
            string after = "bar" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Replace2()
        {
            string before = "#define foo bar" + Environment.NewLine + "baz" + Environment.NewLine;
            string after = "baz" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// C++ trick for introducing a comment.  Used in the definition for _VARIANT_BOOL
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Replace3()
        {
            string before = "#define foo /##/ " + Environment.NewLine + "foo bar" + Environment.NewLine;
            string after = "// bar" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Replace4()
        {
            string before = "#define m1(x) x##1 2" + Environment.NewLine + "m1(5)" + Environment.NewLine;
            string after = "51 2" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Simple macro method
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method1()
        {
            string before = "#define foo(x) x" + Environment.NewLine + "foo(1)";
            string after = "1" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Couple of different replacements
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method2()
        {
            string before = "#define foo(x) x" + Environment.NewLine + "foo(1)" + Environment.NewLine + "foo(\"hello\")" + Environment.NewLine + "foo(0x5)" + Environment.NewLine;
            string after = "1" + Environment.NewLine + "\"hello\"" + Environment.NewLine + "0x5" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Whitespace junk
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method3()
        {
            string before = "#define foo(x)                   x" + Environment.NewLine + "foo(1)" + Environment.NewLine;
            string after = "1" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Several macros
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method4()
        {
            string before = "#define foo(x,y) x y" + Environment.NewLine + "foo(1,2)" + Environment.NewLine + "foo(\"h\", \"y\")" + Environment.NewLine;
            string after = "1 2" + Environment.NewLine + "\"hy\"" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Quote me test
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method5()
        {
            string before = "#define foo(x) #x" + Environment.NewLine + "foo(1)" + Environment.NewLine;
            string after = "\"1\"" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Collapse side by side strings
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method6()
        {
            string before = "#define foo(x) #x" + Environment.NewLine + "\"h\"foo(1)\"y\"" + Environment.NewLine;
            string after = "\"h1y\"" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// ## test
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method7()
        {
            string before = "#define foo(x) x##__" + Environment.NewLine + "foo(y)" + Environment.NewLine;
            string after = "y__" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Method8()
        {
            string before = "#define foo(x,y) x##y" + Environment.NewLine + "foo(y,z)" + Environment.NewLine;
            string after = "yz" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Weird macro arguments
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method9()
        {
            string before = "#define foo(x) x" + Environment.NewLine + "foo(y(0))" + Environment.NewLine;
            string after = "y(0)" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void Method10()
        {
            string before = "#define foo(x,y) x ## y" + Environment.NewLine + "#define foo2(x,y) x## y" + Environment.NewLine + "#define foo3(x,y) x ##y" + Environment.NewLine + "foo(1,2)" + Environment.NewLine + "foo2(3,4)" + Environment.NewLine + "foo3(5,6)" + Environment.NewLine;
            string after = "12" + Environment.NewLine + "34" + Environment.NewLine + "56" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Recursive method calls
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method11()
        {
            string before = "#define inner(x) x" + Environment.NewLine + "#define outer(x) inner(x)" + Environment.NewLine + "outer(5)" + Environment.NewLine;
            string after = "5" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// A more complex recursive method call
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method12()
        {
            string before = "#define inner(x,y) x##y" + Environment.NewLine + "#define outer(x,y) inner(x,y)" + Environment.NewLine + "outer(1,2)" + Environment.NewLine;
            string after = "12" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Make sure that quoted strings used as method macro arguments are properly
        /// replaced in the string and collapsed
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method13()
        {
            string before = "#define x(y) \"foo\" y \"bar\"" + Environment.NewLine + "x(\"hey\")" + Environment.NewLine;
            string after = "\"fooheybar\"" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Spacing between arguments needs to be maintained
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method14()
        {
            string before = "#define x(y) y" + Environment.NewLine + "x(a b)" + Environment.NewLine;
            string after = "a b" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// When several items are passed as a single macro parameter make sure they still
        /// go through replacement
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Method15()
        {
            string before = "#define foo bar" + Environment.NewLine + "#define m1(x) x" + Environment.NewLine + "m1(foo 2)" + Environment.NewLine;
            string after = "bar 2" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Side by strings should collapse
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Misc1()
        {
            string before = "\"foo\"\"bar\"" + Environment.NewLine;
            string after = "\"foobar\"" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        /// <summary>
        /// Side by side strings with spaces should collapse
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void Misc2()
        {
            string before = "\"foo\"     \"bar\"" + Environment.NewLine;
            string after = "\"foobar\"" + Environment.NewLine;
            VerifyNormal(before, after);
        }

        [Fact()]
        public void UnbalancedConditional1()
        {
            string text = "#ifndef WINAPI";
            VerifyCount(text, 0, 0);
        }

        [Fact()]
        public void UnbalancedConditional2()
        {
            string text = "#ifdef WINAPI";
            VerifyCount(text, 1, 0);
        }

        /// <summary>
        /// Make sure that permanent macros are preserved
        /// </summary>
        /// <remarks></remarks>
        [Fact()]
        public void PermanentMacro1()
        {
            List<Macro> list = new List<Macro>();
            list.Add(new Macro("FOO", "BAR", true));
            VerifyNormal(list, "#define FOO BAZ" + Environment.NewLine + "FOO" + Environment.NewLine, "BAR" + Environment.NewLine);
        }
    }
}
