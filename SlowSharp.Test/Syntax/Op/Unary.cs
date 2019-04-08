using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class UnaryTest
    {
        [TestMethod]
        public void PrefixUnary()
        {
            Assert.AreEqual(
                -5,
                TestRunner.Run(@"return -5;"));
            Assert.AreEqual(
                -5,
                TestRunner.Run(@"var a = 5; return -a;"));
        }

        [TestMethod]
        public void PostfixIncDec()
        {
            Assert.AreEqual(
                5,
                TestRunner.Run(@"var a = 5; return a ++;"));
            Assert.AreEqual(
                6,
                TestRunner.Run(@"var a = 5; a ++; return a;"));
        }
    }
}
