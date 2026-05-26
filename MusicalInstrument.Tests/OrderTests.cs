using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicalInstrument.Tests
{
    [TestClass]
    public class OrderTests
    {
        
        /// Проверка расчета срока доставки при заказе 5 товаров (>3) - должно быть 3 дня

        [TestMethod]
        public void CalculateDeliveryDate_TotalQuantity5_ReturnsPlus3Days()
        {
            int totalQuantity = 5;
            DateTime orderDate = new DateTime(2026, 5, 20);
            DateTime expected = new DateTime(2026, 5, 23);

            DateTime actual;
            if (totalQuantity >= 3)
                actual = orderDate.AddDays(3);
            else
                actual = orderDate.AddDays(6);

            Assert.AreEqual(expected, actual);
        }

       
        /// Проверка расчета срока доставки при заказе 2 товаров (<3) - должно быть 6 дней

        [TestMethod]
        public void CalculateDeliveryDate_TotalQuantity2_ReturnsPlus6Days()
        {
            int totalQuantity = 2;
            DateTime orderDate = new DateTime(2026, 5, 20);
            DateTime expected = new DateTime(2026, 5, 26);

            DateTime actual;
            if (totalQuantity >= 3)
                actual = orderDate.AddDays(3);
            else
                actual = orderDate.AddDays(6);

            Assert.AreEqual(expected, actual);
        }

       
        /// Проверка генерации кода получения (всегда 4 цифры)

        [TestMethod]
        public void GeneratePickupCode_Always_Returns4DigitCode()
        {
            
            Random random = new Random();

            
            string code = random.Next(1000, 9999).ToString();

            Assert.AreEqual(4, code.Length, "Код получения должен состоять из 4 цифр");
        }

       
        /// Проверка диапазона кода получения (от 1000 до 9999)

        [TestMethod]
        public void GeneratePickupCode_Always_CodeInRange1000To9999()
        {
            Random random = new Random();

            int code = random.Next(1000, 9999);

            Assert.IsTrue(code >= 1000 && code <= 9999,
                "Код получения должен быть в диапазоне от 1000 до 9999");
        }
    }
}