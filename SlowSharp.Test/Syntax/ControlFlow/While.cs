using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class WhileTest
    {
        [TestMethod]
        public void BasicWhileLoop()
        {
            Assert.AreEqual(TestRunner.Run(@"
var count = 0;
while(count < 5) count ++;
return count;
"), 5);
        }

        [TestMethod]
        public void WithFalse()
        {
            Assert.AreEqual(TestRunner.Run(@"
while(false) return false;
return true;
"), true);
        }
    }
}
