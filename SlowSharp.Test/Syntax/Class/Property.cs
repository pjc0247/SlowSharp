using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class ClassPropertyTest
    {
        [TestMethod]
        public void ArrowExpression()
        {
            Assert.AreEqual(
                1,
                TestRunner.Run(
                "public static int foo => 1;",
                @"return foo;"));
        }
    }
}
