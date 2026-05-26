using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicalInstrument.Tests
{
    [TestClass]
    public class PriceCalculatorTests
    {

        /// Проверка расчета цены со скидкой 10%
        [TestMethod]
        public void CalculateFinalPrice_Price1000Discount10_Returns900()
        {
            int price = 1000;
            int discount = 10;
            double expected = 900.00;

            double actual = price - (price * discount / 100.0);

            Assert.AreEqual(expected, actual, 0.01);
        }

        /// Проверка расчета цены без скидки (скидка 0%)
        [TestMethod]
        public void CalculateFinalPrice_Price5000Discount0_Returns5000()
        {
            int price = 5000;
            int discount = 0;
            double expected = 5000.00;

            double actual = price - (price * discount / 100.0);

            Assert.AreEqual(expected, actual, 0.01);
        }

        
        /// Проверка расчета цены со 100% скидкой (товар бесплатно)
        [TestMethod]
        public void CalculateFinalPrice_Price5000Discount100_Returns0()
        {
            int price = 5000;
            int discount = 100;
            double expected = 0.00;

            
            double actual = price - (price * discount / 100.0);

            Assert.AreEqual(expected, actual, 0.01);
        }

        /// Проверка расчета цены с большой скидкой (99%)
        [TestMethod]
        public void CalculateFinalPrice_Price10000Discount99_Returns100()
        {
            
            int price = 10000;
            int discount = 99;
            double expected = 100.00;

            double actual = price - (price * discount / 100.0);

            Assert.AreEqual(expected, actual, 0.01);
        }

        
        /// Проверка расчета цены с минимальной ценой (1 рубль, скидка 50%)

        [TestMethod]
        public void CalculateFinalPrice_Price1Discount50_Returns0_5()
        {
            
            int price = 1;
            int discount = 50;
            double expected = 0.50;

            double actual = price - (price * discount / 100.0);

           
            Assert.AreEqual(expected, actual, 0.01);
        }
    }
}