// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.UnitTests.Properties;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.UI.Models
{
    [TestFixture]
    public class KeyValueTest
    {
        [SetUp]
        public void BaseSetUp()
        {
            Settings.Initialise();
        }

        [Test]
        public void TestEqualityWithFunctionKeys()
        {
            KeyValue valBreak1 = new KeyValue(FunctionKeys.Break);
            KeyValue valBreak2 = new KeyValue(FunctionKeys.Break);
            KeyValue valDelete = new KeyValue(FunctionKeys.Delete);

            Assert.Multiple(() =>
            {
                Assert.That(valBreak1, Is.EqualTo(valBreak2));
                Assert.That(valBreak1, Is.Not.EqualTo(valDelete));
            });            
        }

        [Test]
        public void TestEqualityWithStrings()
        {
            KeyValue valHello1 = new KeyValue("Hello");
            KeyValue valHello2 = new KeyValue("Hello");

            KeyValue valGoodbye = new KeyValue("Goodbye");

            Assert.Multiple(() =>
            {
                Assert.That(valHello1, Is.EqualTo(valHello2));
                Assert.That(valHello1, Is.Not.EqualTo(valGoodbye));
            });            
        }

        [Test]
        public void TestEqualityWithStringsAndFunctionKeys()
        {
            KeyValue valBreakHello1 = new KeyValue(FunctionKeys.Break, "Hello");
            KeyValue valBreakHello2 = new KeyValue(FunctionKeys.Break, "Hello");

            KeyValue valBreakGoodbye = new KeyValue(FunctionKeys.Break, "Goodbye");
            KeyValue valDeleteGoodbye = new KeyValue(FunctionKeys.Delete, "Goodbye");

            Assert.Multiple(() =>
            {
                Assert.That(valBreakHello1, Is.EqualTo(valBreakHello2));
                Assert.That(valBreakHello1, Is.Not.EqualTo(valBreakGoodbye));
                Assert.That(valBreakGoodbye, Is.Not.EqualTo(valDeleteGoodbye));
                Assert.That(valBreakHello1, Is.Not.EqualTo(valDeleteGoodbye));
            });            
        }
    }
}
