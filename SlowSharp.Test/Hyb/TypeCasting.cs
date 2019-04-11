using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class HybTypeCastingTest
    {
        [TestMethod]
        public void CastToCompiledType()
        {
            var runner = CScript.CreateRunner(@"
using System.Collections.Generic;
class MyList : List<int> { }
");
            var myList = runner.Instantiate("MyList");
            // virtual casting
            Assert.IsInstanceOfType(myList.Cast<List<int>>().Unwrap(), typeof(List<int>));

            // `(int)MyList` should be failed
            Assert.ThrowsException<InvalidCastException>(() => {
                myList.Cast<int>().Unwrap();
            });
        }
    }
}
