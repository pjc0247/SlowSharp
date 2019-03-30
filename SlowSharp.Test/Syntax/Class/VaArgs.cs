using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class VaArgsTest
    {
        [TestMethod]
        public void BasicCall()
        {
            Assert.AreEqual(
                6,
                TestRunner.Run(@"
static int Fu(params int[] args) {
    var sum = 0;
    foreach (var arg in args)
        sum += arg;
    return sum;
}
",
@"
return Fu(1,2,3);
"));
        }

        [TestMethod]
        public void WithZeroArg()
        {
            Assert.AreEqual(
                0,
                TestRunner.Run(@"
static int Fu(params int[] args) {
    var sum = 0;
    foreach (var arg in args)
        sum += arg;
    return sum;
}
",
@"
return Fu();
"));
        }
    }
}
