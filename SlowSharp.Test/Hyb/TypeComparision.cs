using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class HybTypeComparision
    {
        [TestMethod]
        public void Is()
        {
            var a = new HybInstance(new HybType(typeof(int)), 1);
            var b = new HybInstance(new HybType(typeof(string)), "string");

            Assert.AreEqual(true, a.Is<int>());
            Assert.AreEqual(false, a.Is<float>());
            Assert.AreEqual(true, b.Is<string>());
        }
    }
}
