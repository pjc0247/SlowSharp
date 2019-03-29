using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class CScriptTest
    {
        [TestMethod]
        public void RunAndReturn()
        {
            Assert.AreEqual(
                10,
                CScript.Run(@"
class Foo {
public static int Main() {
    return 10;
}
}
"));
        }
    }
}
