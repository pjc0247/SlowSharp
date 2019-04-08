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

        [TestMethod]
        public void BreakInNestedFor()
        {
            Assert.AreEqual(
                33,
                TestRunner.Run(@"
var count = 0;
for (int i=0;i<5;i++) {
    for (int j=0;j<5;j++) {
        count += 10;
        //if (j == 2) break;
    }
    count ++;
    //if (i == 2) break;
}
return count;
"));
        }
    }
}
