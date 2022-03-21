using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class HotReloadTest
    {
        [TestMethod]
        public void BasicOverwriteMethod()
        {
            var r = CScript.CreateRunner(@"
class Foo { public int GiveMeNumber() => 10; }
");
            var foo = r.Instantiate("Foo");
            Assert.AreEqual(10, foo.Invoke("GiveMeNumber").Unwrap());

            r.UpdateMethodsOnly(@"
class Foo { public int GiveMeNumber() => 20; }
");
            Assert.AreEqual(20, foo.Invoke("GiveMeNumber").Unwrap());
        }

        [TestMethod]
        public void MembersShouldBeUnchanged()
        {
            var r = CScript.CreateRunner(@"
class Foo {
    public int Number = 10;
    public int GiveMeNumber() => Number;
}
");
            var foo = r.Instantiate("Foo");
            Assert.AreEqual(10, foo.Invoke("GiveMeNumber").Unwrap());

            foo.SetPropertyOrField("Number", HybInstance.Int(20));
            Assert.AreEqual(20, foo.Invoke("GiveMeNumber").Unwrap());

            r.UpdateMethodsOnly(@"
class Foo { public int GiveMeNumber() => Number * 2; }
");
            Assert.AreEqual(40, foo.Invoke("GiveMeNumber").Unwrap());
        }
    }
}
