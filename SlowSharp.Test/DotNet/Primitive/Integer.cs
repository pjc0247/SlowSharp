using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class IntegerTest
    {
        [TestMethod]
        public void ParseInt()
        {
            Assert.AreEqual(
                55,
                TestRunner.Run(@"
return int.Parse(""55"");
"));
        }

        [TestMethod]
        public void ToString()
        {
            Assert.AreEqual(
                "55",
                TestRunner.Run(@"
return 55.ToString();
"));
        }
    }
}
