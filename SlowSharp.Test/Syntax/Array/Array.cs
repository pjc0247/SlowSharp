using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class ArrayTest
    {
        [TestMethod]
        public void ArrayType()
        {
            Assert.AreEqual(typeof(int[]), TestRunner.Run(@"
var emptyIntAry = new int[] { };
return emptyIntAry.GetType();
"));

            Assert.AreEqual(typeof(string[]), TestRunner.Run(@"
var emptyStrAry = new string[] { };
return emptyStrAry.GetType();
"));

            Assert.AreEqual(typeof(object[]), TestRunner.Run(@"
var emptyObjAry = new object[] { };
return emptyObjAry.GetType();
"));
        }

        [TestMethod]
        public void CreateWithTrailingElem()
        {
            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, TestRunner.Run(@"
var ary = new int[] { 1,2,3,4,5 };
return ary;
") as ICollection);

            CollectionAssert.AreEqual(new string[] { "A", "B", "C" }, TestRunner.Run(@"
var ary = new string[] { ""A"", ""B"", ""C"" };
return ary;
") as ICollection);

            CollectionAssert.AreEqual(new object[] { 1,2,3, "A", "B", "C" }, TestRunner.Run(@"
var ary = new object[] { 1,2,3, ""A"", ""B"", ""C"" };
return ary;
") as ICollection);
        }
    }
}
