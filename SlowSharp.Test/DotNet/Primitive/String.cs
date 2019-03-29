using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void Instantiate()
        {
            Assert.AreEqual(
                "hello",
                TestRunner.Run(@"
return ""hello"";
"));
        }

        [TestMethod]
        public void Concat()
        {
            Assert.AreEqual(
                "helloworld",
                TestRunner.Run(@"
return ""hello"" + ""world"";
"));
        }

        [TestMethod]
        public void Interpolation()
        {
            Assert.AreEqual(
                "helloworld",
                TestRunner.Run(@"
return $""{""hello""}{""world""}"";
"));
        }
    }
}
