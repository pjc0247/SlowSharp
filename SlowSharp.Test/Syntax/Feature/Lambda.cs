using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class LambdaTest
    {
        [TestMethod]
        public void BasicAction()
        {
            Assert.AreEqual(
                10,
                TestRunner.Run(@"
var b = 0;
var a = new Action(() => {
    b = 10;
});
a();
return b;
"));
        }
    }
}
