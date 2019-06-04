using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class EnumTest
    {
        [TestMethod]
        public void BasicEnum()
        {
            Assert.AreEqual(
                10 * 20,
                TestRunner.Run(
@"
enum Foo { 
  AA = 10, BB = 20
}
",
@"
",
@"
return Foo.AA * Foo.BB;
"));
        }

        [TestMethod]
        public void AutoIncValue()
        {
            Assert.AreEqual(
                12 * 13,
                TestRunner.Run(
@"
enum Foo { 
  AA = 10, BB, CC, DD
}
",
@"
",
@"
return Foo.DD * Foo.CC;
"));
        }

        [TestMethod]
        public void AutoIncValue2()
        {
            Assert.AreEqual(
                101 * 13,
                TestRunner.Run(
@"
enum Foo { 
  AA = 10, BB, CC, DD,
  XX = 99, YY, ZZ
}
",
@"
",
@"
return Foo.DD * Foo.ZZ;
"));
        }
    }
}
