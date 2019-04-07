using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class HybTypeComparision
    {
        [TestMethod]
        public void Is()
        {
            var a = new HybInstance(new HybType(typeof(int)), 1);
            var b = new HybInstance(new HybType(typeof(string)), "string");

            Assert.AreEqual(true, a.Is<int>());
            Assert.AreEqual(false, a.Is<float>());
            Assert.AreEqual(true, b.Is<string>());
        }

        [TestMethod]
        public void Is2()
        {
            var runner = CScript.CreateRunner(@"
using System.Collections.Generic;
class MyList : List<int> { }
class MyStringList : List<string> { }
class MyFakeList { }
");
            var myList = runner.Instantiate("MyList");
            Assert.AreEqual(true, myList.Is<List<int>>());

            var myStringList = runner.Instantiate("MyStringList");
            Assert.AreEqual(false, myStringList.Is<List<int>>());

            var myFakeList = runner.Instantiate("MyFakeList");
            Assert.AreEqual(false, myStringList.Is<List<int>>());
        }
    }
}
