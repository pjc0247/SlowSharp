using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class CtorTest
    {
        [TestMethod]
        public void BasicCtor()
        {
            Assert.AreEqual(
                5,
                TestRunner.Run(
@"
class Boo {
    public int flag = 0;

    public Boo() {
        flag = 5;
    }
}
",
@"
",
@"
return (new Boo()).flag;
"));
        }

        [TestMethod]
        public void Overload()
        {
            Assert.AreEqual(
                15,
                TestRunner.Run(
@"
class Boo {
    public int flag = 0;

    public Boo() {
        flag = 5;
    }
    public Boo(int b) {
        flag = 15;
    }
}
",
@"
",
@"
return (new Boo(2)).flag;
"));
        }


        // Field initializer should be executed before ctor
        [TestMethod]
        public void InitSequence_Field()
        {
            Assert.AreEqual(
                99,
                TestRunner.Run(
@"
class Boo {
    public int flag = 99;

    public Boo() {
        if (flag != 99)
            throw new Exception();
    }
}
",
@"
",
@"
return (new Boo()).flag;
"));
        }

        // Property initializer should be executed before ctor
        [TestMethod]
        public void InitSequence_Property()
        {
            Assert.AreEqual(
                99,
                TestRunner.Run(
@"
class Boo {
    public int flag { get; } = 99;

    public Boo() {
        if (flag != 99)
            throw new Exception();
    }
}
",
@"
",
@"
return (new Boo()).flag;
"));
        }
    }
}
