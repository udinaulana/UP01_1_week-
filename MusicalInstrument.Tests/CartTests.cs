using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicalInstrument.Tests
{
    [TestClass]
    public class CartTests
    {
        // Вспомогательный класс для тестов (модель товара)
        public class TestProduct
        {
            public int IdProduct { get; set; }
            public string NameOrder { get; set; }
            public int Price { get; set; }
            public int Discount { get; set; }
        }

        // Вспомогательный класс для тестов (элемент корзины)
        public class TestCartItem
        {
            public TestProduct Product { get; set; }
            public int Quantity { get; set; }

            // Расчет стоимости позиции со скидкой
            public double TotalPrice
            {
                get
                {
                    return (Product.Price - (Product.Price * Product.Discount / 100.0)) * Quantity;
                }
            }
        }

        /// Проверка расчета общей суммы корзины с несколькими товарами

        [TestMethod]
        public void CalculateCartTotal_MultipleItems_ReturnsCorrectTotal()
        {
            var products = new List<TestProduct>
            {
                new TestProduct { IdProduct = 1, NameOrder = "Синтезатор", Price = 10000, Discount = 10 },
                new TestProduct { IdProduct = 2, NameOrder = "Гитара", Price = 5000, Discount = 0 },
                new TestProduct { IdProduct = 3, NameOrder = "Пианино", Price = 20000, Discount = 20 }
            };

            var cartItems = new List<TestCartItem>
            {
                new TestCartItem { Product = products[0], Quantity = 2 },  // 2 шт x 9000 = 18000
                new TestCartItem { Product = products[1], Quantity = 1 },  // 1 шт x 5000 = 5000
                new TestCartItem { Product = products[2], Quantity = 3 }   // 3 шт x 16000 = 48000
            };

           
            double expected = 18000 + 5000 + 48000;

            double actual = cartItems.Sum(x => x.TotalPrice);

            Assert.AreEqual(expected, actual, 0.01);
        }
    }
}