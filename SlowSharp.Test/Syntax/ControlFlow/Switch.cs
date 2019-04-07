using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class SwitchTest
    {
        [TestMethod]
        public void WithInt()
        {
            Assert.AreEqual(
                true,
                TestRunner.Run(@"
var a = 15;
switch (a) {
case 1: return false;
case 2: return false;
case 15: return true;
case 16: return false;
}
"));
        }

        [TestMethod]
        public void Fall()
        {
            Assert.AreEqual(
                true,
                TestRunner.Run(@"
var a = 15;
switch (a) {
case 1: return false;
case 2: return false;
case 15:
case 16: return true;
case 17: return false;
}
"));
        }

        [TestMethod]
        public void Default()
        {
            Assert.AreEqual(
                true,
                TestRunner.Run(@"
var a = 15;
switch (a) {
case 16: return false;
case 17: return false;
default: return true;
}
"));
        }
    }
}

