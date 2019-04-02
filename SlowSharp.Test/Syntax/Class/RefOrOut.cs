using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class RefOrOutTest
    {
        public static void MakeDoubleOut(int input, out int value)
        {
            value = input * 2;
        }
        public static void MakeDoubleRef(ref int value)
        {
            value *= 2;
        }

        [TestMethod]
        public void Ref()
        {
            Assert.AreEqual(
                20,
                TestRunner.Run(
                @"
int v = 10;
RefOrOutTest.MakeDoubleOut(v, out v);
return v;"));
        }

        [TestMethod]
        public void Out()
        {
            Assert.AreEqual(
                20,
                TestRunner.Run(
                @"
int v = 10;
RefOrOutTest.MakeDoubleRef(ref v);
return v;"));
        }
    }
}
