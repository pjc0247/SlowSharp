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

        [TestMethod]
        public void Break()
        {
            Assert.AreEqual(
                3,
                TestRunner.Run(@"
var count = 0;
for (int i=0;i<5;i++) {
  count ++;
  if (i == 2) break;
}
return count;
"));
        }
    }
}
