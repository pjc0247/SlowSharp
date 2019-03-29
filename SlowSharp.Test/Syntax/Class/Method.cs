using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class MethodTest
    {
        [TestMethod]
        public void BasicMethod()
        {
            Assert.AreEqual(
                10,
                TestRunner.Run(@"
static int Fu() {
    return 10;
}
",
@"
return Fu();
"));
        }

        [TestMethod]
        public void WithArrowExpressionBody()
        {
            Assert.AreEqual(
                10,
                TestRunner.Run(@"
static int Fu() => 10;
", 
@"
return Fu();
"));
        }
    }
}
