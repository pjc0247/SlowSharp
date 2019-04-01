using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Slowsharp.Test
{
    [TestClass]
    public class OptAndOrTest
    {
        [TestMethod]
        public void Or()
        {
            // Right should not be evaluated
            Assert.AreEqual(
                true,
                TestRunner.Run(
                    @"
public static bool Left() {
    return true;
}
public static bool Right() {
    throw new Exception();  
}
",
                    @"
return Left() || Right();
"));
        }

        [TestMethod]
        public void And()
        {
            // Right should not be evaluated
            Assert.AreEqual(
                false,
                TestRunner.Run(
                    @"
public static bool Left() {
    return false;
}
public static bool Right() {
    throw new Exception();  
}
",
                    @"
return Left() && Right();
"));
        }
    }
}
