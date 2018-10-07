using System;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.Models
{
    [TestFixture]
    public class KeyValueTest
    {
        [Test]
        public void TestEqualityWithFunctionKeys()
        {
            KeyValue valBreak1 = new KeyValue(FunctionKeys.Break);
            KeyValue valBreak2 = new KeyValue(FunctionKeys.Break);
            KeyValue valDelete = new KeyValue(FunctionKeys.Delete);

            Assert.AreEqual(valBreak1, valBreak2);        
            Assert.AreNotEqual(valBreak1, valDelete);
        }

        [Test]
        public void TestEqualityWithStrings()
        {
            KeyValue valHello1 = new KeyValue("Hello");
            KeyValue valHello2 = new KeyValue("Hello");

            KeyValue valGoodbye = new KeyValue("Goodbye");

            Assert.AreEqual(valHello1, valHello2);
            Assert.AreNotEqual(valHello1, valGoodbye);
        }

        [Test]
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
