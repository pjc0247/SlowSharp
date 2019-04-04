using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class ForEachTest
    {
        [TestMethod]
        public void BasicForEachLoop()
        {
            Assert.AreEqual(
                1 + 2 + 3,
                TestRunner.Run(@"
var a = new int[] {1,2,3};
var sum = 0;

foreach (var b in a) 
    sum += b;
    
return sum;
"));
        }
        [TestMethod]
        public void WithEmptyCollection()
        {
            Assert.AreEqual(
                0,
                TestRunner.Run(@"
var a = new int[] {};
var sum = 0;

foreach (var b in a) 
    sum += b;
    
return sum;
"));
        }

        [TestMethod]
        public void NotEnumerable()
        {
            Assert.ThrowsException<InvalidOperationException>(() => {
                TestRunner.Run(@"
var sum = 0;

foreach (var b in sum) 
    sum += b;
    
return sum;
");
            });
        }
    }
}
