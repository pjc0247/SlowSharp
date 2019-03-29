using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class AccessorTest
    {
        [TestMethod]
        public void Public()
        {
            Assert.AreEqual(
                10,
                TestRunner.RunRaw(@"
public class Boo {
    public int a = 10;
}
public class Foo {
    public static int Main() {
        return (new Boo()).a;
    }
}
                "));
        }

        [TestMethod]
        public void NonSpecified()
        {
            Assert.ThrowsException<SemanticViolationException>(() => {
                TestRunner.RunRaw(@"
public class Boo {
    int a = 10;
}
public class Foo {
    public static int Main() {
        return (new Boo()).a;
    }
}
                ");
            });
        }

    }
}
