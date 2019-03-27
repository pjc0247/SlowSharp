using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class ForTest
    {
        [TestMethod]
        public void BasicForLoop()
        {
            Assert.AreEqual(TestRunner.Run(@"
var count = 0;
for (int i=0;i<5;i++) count ++;
return count;
"), 5);
        }
    }
}
