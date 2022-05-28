using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class YieldTest
    {
        [TestMethod]
        public void BasicYieldStatement()
        {
            Assert.AreEqual(TestRunner.RunRaw(@"
using System.Collections;

class Program
{
    public static int s = 0;

    public static IEnumerator Test() {
        yield return 1;
        yield return 2;
        yield return 3;
        yield return 4;
        yield return 5;
    }

    static void Main() {
       var c = Test();

       while (c.MoveNext()) {
         s += c.Current;
       }

       return s;
    }
}
"), 1 + 2 + 3 + 4 + 5);
        }
    }
}
