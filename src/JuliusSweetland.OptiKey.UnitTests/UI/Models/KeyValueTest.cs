using System;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JuliusSweetland.OptiKey.UnitTests.UI.Models
{
    [TestClass]
    public class KeyValueTest
    {
        [TestMethod]
        public void TestEqualityWithFunctionKeys()
        {
            KeyValue valBreak1 = new KeyValue(FunctionKeys.Break);
            KeyValue valBreak2 = new KeyValue(FunctionKeys.Break);
            KeyValue valDelete = new KeyValue(FunctionKeys.Delete);

            Assert.AreEqual(valBreak1, valBreak2);        
            Assert.AreNotEqual(valBreak1, valDelete);
        }

        [TestMethod]
        public void TestEqualityWithStrings()
        {
            KeyValue valHello1 = new KeyValue("Hello");
            KeyValue valHello2 = new KeyValue("Hello");

            KeyValue valGoodbye = new KeyValue("Goodbye");

            Assert.AreEqual(valHello1, valHello2);
            Assert.AreNotEqual(valHello1, valGoodbye);
        }

        [TestMethod]
        public void TestEqualityWithStringsAndFunctionKeys()
        {
            KeyValue valBreakHello1 = new KeyValue(FunctionKeys.Break, "Hello");
            KeyValue valBreakHello2 = new KeyValue(FunctionKeys.Break, "Hello");

            KeyValue valBreakGoodbye = new KeyValue(FunctionKeys.Break, "Goodbye");
            KeyValue valDeleteGoodbye = new KeyValue(FunctionKeys.Delete, "Goodbye");

            Assert.AreEqual(valBreakHello1, valBreakHello2);
            Assert.AreNotEqual(valBreakHello1, valBreakGoodbye);
            Assert.AreNotEqual(valBreakGoodbye, valDeleteGoodbye);
            Assert.AreNotEqual(valBreakHello1, valDeleteGoodbye);
        }
    }
}
