using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class StringInterpolationTest
    {
        [TestMethod]
        public void BasicInterpolation()
        {
            Assert.AreEqual(
                "1hello",
                TestRunner.Run(@"
var a = 1;
var b = ""hello"";
return $""{a}{b}"";"));
        }

        [TestMethod]
        public void NullInterpolation()
        {
            Assert.AreEqual(
                "null",
                TestRunner.Run(@"
object a = null;
return $""{a}"";"));
        }
    }
}
