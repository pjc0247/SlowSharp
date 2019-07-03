using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class MultifileTest
    {
        [TestMethod]
        public void Multifile()
        {
            var r = CScript.CreateRunner(new string[]{
                @"
class Bar { public int GiveMeNumber() => 10; }
",
            @"
class Foo { public int GiveMeNumber() => (new Bar()).GiveMeNumber() * 2; }
"});
            var foo = r.Instantiate("Foo");
            Assert.AreEqual(20, foo.Invoke("GiveMeNumber").Unwrap());
        }
    }
}
