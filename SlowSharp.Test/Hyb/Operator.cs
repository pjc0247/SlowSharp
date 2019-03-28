using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class HybOperatorTest
    {
        [TestMethod]
        public void Plus()
        {
            var n = HybInstance.Int(5);
            n += 1;
            Assert.AreEqual(6, n.As<int>());
        }

        [TestMethod]
        public void PostfixPlus()
        {
            var n = HybInstance.Int(5);
            var after = n ++;
            Assert.AreEqual(5, after.As<int>());
            Assert.AreEqual(6, n.As<int>());
        }
        [TestMethod]
        public void PrefixPlus()
        {
            var n = HybInstance.Int(5);
            var after = ++n;
            Assert.AreEqual(6, after.As<int>());
            Assert.AreEqual(6, n.As<int>());
        }
    }
}
