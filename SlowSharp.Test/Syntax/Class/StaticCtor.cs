using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class StaticCtorTest
    {
        [TestMethod]
        public void BasicCtor()
        {
            Assert.AreEqual(
                1234,
                TestRunner.Run(
@"
class Boo {
    public static int flag = 0;
    
    static Boo() {
        flag = 1234;
    }
}
",
@"
",
@"
return Boo.flag;
"));
        }

        [TestMethod]
        public void Dependency()
        {
            // `Bar` should be initialized during `Foo`'s static ctor
            Assert.AreEqual(
                40,
                TestRunner.Run(
@"
public class Foo {
	public static int A;
	public static int B;
	
	static Foo() {
		Bar.A = 20;
		
		B *= 2;
	}
}
public class Bar {
	public static int A;
	
	static Bar() {
		Foo.B = 20;
	}
}
",
@"
",
@"
return Foo.B;
"));
        }
    }
}
