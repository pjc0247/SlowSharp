using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class OpAndAssignTest
    {
        [TestMethod]
        public void Plus()
        {
            Assert.AreEqual(
                3,
                TestRunner.Run(@"
var a = 1;
a += 2;
return a;
"));
        }

        [TestMethod]
        public void Minus()
        {
            Assert.AreEqual(
                -1,
                TestRunner.Run(@"
var a = 1;
a -= 2;
return a;
"));
        }

        [TestMethod]
        public void Mul()
        {
            Assert.AreEqual(
                6,
                TestRunner.Run(@"
var a = 3;
a *= 2;
return a;
"));
        }

        [TestMethod]
        public void Div()
        {
            Assert.AreEqual(
                3,
                TestRunner.Run(@"
var a = 6;
a /= 2;
return a;
"));
        }
    }
}
