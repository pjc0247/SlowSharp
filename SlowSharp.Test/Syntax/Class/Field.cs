using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class ClassFieldTest
    {
        [TestMethod]
        public void InitializerWithInteger()
        {
            Assert.AreEqual(TestRunner.Run(
                "public static int foo = 1;",
                @"return foo;"), 1);
        }

        [TestMethod]
        public void InitializerWithString()
        {
            Assert.AreEqual(TestRunner.Run(
                "public static int foo = \"foo\";",
                @"return foo;"), "foo");
        }
    }
}
