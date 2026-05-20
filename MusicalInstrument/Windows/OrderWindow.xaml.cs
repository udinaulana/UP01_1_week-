using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MusicalInstrument.DataBase;
using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;


namespace MusicalInstrument.Windows
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        DBEntities db = new DBEntities();
        PrkUsers user;
        PrkOrders order;

        public OrderWindow(PrkUsers user, PrkOrders order)
        {
            // Регистрируем шрифт Times New Roman
            GlobalFontSettings.FontResolver = new FontResolver();

            InitializeComponent();
            this.user = user;
            this.order = order;

            FIOBlok.Text = $"{user.Surname} {user.Name} {user.Patronymic}";
            LoadOrderTotals();
        }

        private void LoadOrderTotals()
        {
            double totalOriginal = 0;
            double totalDiscounted = 0;

            var items = db.PrkOrderDetails.Where(x => x.IdOrder == order.IdOrder).ToList();

            foreach (var item in items)
            {
                var product = db.PrlProducts.Find(item.IdProduct);
                if (product != null)
                {
                    double price = product.Price;
                    double discount = product.Discount;
                    double priceWithDiscount = price - (price * discount / 100);

                    totalOriginal += price * item.Quantity;
                    totalDiscounted += priceWithDiscount * item.Quantity;
                }
            }

            double discountSum = totalOriginal - totalDiscounted;

            TotalPriceBlock.Text = $"Сумма заказа: {totalOriginal:F2} руб.";
            DiscountBlock.Text = $"Скидка: {discountSum:F2} руб.";
            ResultPriceBlock.Text = $"Итого к оплате: {totalDiscounted:F2} руб.";
        }

        private void FormCoupon_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF файл|*.pdf";
            saveFileDialog.FileName = $"Талон_заказ_{order.CodeOrder}.pdf";

            if (saveFileDialog.ShowDialog() == true)
            {
                CreatePdf(saveFileDialog.FileName);
            }
        }

        private void CreatePdf(string path)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Используем Times New Roman
            var titleFont = new XFont("Times New Roman", 18, XFontStyleEx.Bold);
            var headerFont = new XFont("Times New Roman", 12, XFontStyleEx.Bold);
            var normalFont = new XFont("Times New Roman", 10);
            var codeFont = new XFont("Times New Roman", 28, XFontStyleEx.Bold);

            double y = 40;
            double leftMargin = 40;

            // Заголовок
            gfx.DrawString("ТАЛОН ЗАКАЗА", titleFont, XBrushes.Black, leftMargin, y);
            y += 35;

            // Дата заказа
            gfx.DrawString($"Дата заказа: {order.DateOrder:dd.MM.yyyy}", normalFont, XBrushes.Black, leftMargin, y);
            y += 20;

            // Номер заказа
            gfx.DrawString($"Номер заказа: {order.CodeOrder}", normalFont, XBrushes.Black, leftMargin, y);
            y += 20;

            // ФИО клиента
            gfx.DrawString($"Клиент: {user.Surname} {user.Name} {user.Patronymic}", normalFont, XBrushes.Black, leftMargin, y);
            y += 30;

            // Состав заказа
            gfx.DrawString("Состав заказа:", headerFont, XBrushes.Black, leftMargin, y);
            y += 20;

            double totalOriginal = 0;
            double totalDiscounted = 0;

            var items = db.PrkOrderDetails.Where(x => x.IdOrder == order.IdOrder).ToList();

            foreach (var item in items)
            {
                var product = db.PrlProducts.Find(item.IdProduct);
                if (product != null)
                {
                    double price = product.Price;
                    double discount = product.Discount;
                    double priceWithDiscount = price - (price * discount / 100);
                    double itemTotal = priceWithDiscount * item.Quantity;

                    totalOriginal += price * item.Quantity;
                    totalDiscounted += itemTotal;

                    string line = $"{product.NameOrder} x {item.Quantity} шт. = {itemTotal:F2} руб.";
                    gfx.DrawString(line, normalFont, XBrushes.Black, leftMargin + 10, y);
                    y += 18;
                }
            }

            y += 10;
            double discountSum = totalOriginal - totalDiscounted;

            gfx.DrawString($"Сумма заказа: {totalOriginal:F2} руб.", normalFont, XBrushes.Black, leftMargin, y);
            y += 20;

            gfx.DrawString($"Сумма скидки: {discountSum:F2} руб.", normalFont, XBrushes.Black, leftMargin, y);
            y += 20;

            gfx.DrawString($"Итого к оплате: {totalDiscounted:F2} руб.", headerFont, XBrushes.Black, leftMargin, y);
            y += 30;

            // Пункт выдачи
            var point = db.PrkPickUpPoints.Find(order.IdPickUpPoint);
            if (point != null)
            {
                gfx.DrawString($"Пункт выдачи: {point.City}, {point.Street}, {point.Home}", normalFont, XBrushes.Black, leftMargin, y);
                y += 25;
            }

            // Срок доставки
            gfx.DrawString($"Срок доставки: {order.DateDelivery:dd.MM.yyyy}", normalFont, XBrushes.Black, leftMargin, y);
            y += 30;

            // Код получения
            gfx.DrawString("КОД ПОЛУЧЕНИЯ:", headerFont, XBrushes.Black, leftMargin, y);
            y += 25;
            gfx.DrawString(order.CodeOrder, codeFont, XBrushes.Red, leftMargin, y);

            document.Save(path);
            MessageBox.Show($"Талон сохранен!\nКод получения: {order.CodeOrder}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Регистратор шрифта для Times New Roman
        public class FontResolver : IFontResolver
        {
            public byte[] GetFont(string faceName)
            {
                string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

                string[] fontNames = { "times.ttf", "timesnewroman.ttf", "Times New Roman.ttf" };

                foreach (string fontName in fontNames)
                {
                    string fontPath = System.IO.Path.Combine(fontsFolder, fontName);
                    if (System.IO.File.Exists(fontPath))
                    {
                        return System.IO.File.ReadAllBytes(fontPath);
                    }
                }

                return null;
            }

            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                return new FontResolverInfo("Times New Roman");
            }
        }
    }
}