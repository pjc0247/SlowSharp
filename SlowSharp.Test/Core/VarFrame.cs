using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class VarFrameTest
    {
        [TestMethod]
        public void FrameBetweenMethods()
        {
            Assert.AreEqual(
                10,
                TestRunner.Run(@"
public static void Bo() {
    var aa = 1234;
}
",@"
var aa = 10;
Bo();
return aa;
"));
        }
    }
}
