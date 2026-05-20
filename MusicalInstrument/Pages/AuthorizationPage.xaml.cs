using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicalInstrument.DataBase;

namespace MusicalInstrument.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        DBEntities db = new DBEntities();
        Random rand = new Random();
        bool firstAttempt = true;
        bool cantLogin = false;
        string currentCaptcha = "";

        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void GenerateCaptcha()
        {
            CaptchaCanvas.Children.Clear();

            // Генерируем 4 символа
            string chars = "QWERTYUIOPASDFGHJKLZXCVBNM0123456789";
            currentCaptcha = "";
            for (int i = 0; i < 4; i++)
            {
                currentCaptcha += chars[rand.Next(chars.Length)];
            }

            // Рисуем символы со смещением и поворотом
            double startX = 30;
            double startY = 40;

            for (int i = 0; i < currentCaptcha.Length; i++)
            {
                double offsetX = rand.Next(-5, 5);
                double offsetY = rand.Next(-8, 8);
                double rotation = rand.Next(-20, 20);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = currentCaptcha[i].ToString();
                textBlock.FontSize = 24;
                textBlock.FontWeight = FontWeights.Bold;
                textBlock.Foreground = GetRandomBrush();

                Canvas.SetLeft(textBlock, startX + (i * 45) + offsetX);
                Canvas.SetTop(textBlock, startY + offsetY);
                textBlock.RenderTransform = new RotateTransform(rotation);

                CaptchaCanvas.Children.Add(textBlock);
            }

            // Добавляем линии шума
            for (int i = 0; i < 15; i++)
            {
                Line line = new Line();
                line.X1 = rand.Next(0, 250);
                line.Y1 = rand.Next(0, 60);
                line.X2 = rand.Next(0, 250);
                line.Y2 = rand.Next(0, 60);
                line.Stroke = GetRandomBrush();
                line.StrokeThickness = 1;
                CaptchaCanvas.Children.Add(line);
            }

            // Добавляем точки шума
            for (int i = 0; i < 80; i++)
            {
                Ellipse point = new Ellipse();
                point.Width = 2;
                point.Height = 2;
                point.Fill = GetRandomBrush();
                Canvas.SetLeft(point, rand.Next(0, 250));
                Canvas.SetTop(point, rand.Next(0, 60));
                CaptchaCanvas.Children.Add(point);
            }

            // Перечеркивающие линии
            for (int i = 0; i < 2; i++)
            {
                Line strikeLine = new Line();
                strikeLine.X1 = rand.Next(20, 60);
                strikeLine.Y1 = rand.Next(25, 45);
                strikeLine.X2 = rand.Next(190, 230);
                strikeLine.Y2 = rand.Next(25, 45);
                strikeLine.Stroke = Brushes.Red;
                strikeLine.StrokeThickness = 1.5;
                CaptchaCanvas.Children.Add(strikeLine);
            }
        }

        private Brush GetRandomBrush()
        {
            Brush[] brushes = { Brushes.White, Brushes.LightGreen, Brushes.Yellow,
                               Brushes.LightBlue, Brushes.Orange, Brushes.Pink };
            return brushes[rand.Next(brushes.Length)];
        }

        private async void enterButton_Click(object sender, RoutedEventArgs e)
        {
            if (cantLogin) return;

            var userObj = db.PrkUsers.FirstOrDefault(x => x.Login == loginBox.Text && x.Password == PasswordBox.Password);

            // Если это не первая попытка, проверяем капчу
            if (!firstAttempt)
            {
                if (CaptchaInputBox.Text.ToUpper() != currentCaptcha)
                {
                    errorBlock.Text = "Неверный код подтверждения!";
                    errorBlock.Visibility = Visibility.Visible;
                    GenerateCaptcha(); // Обновляем капчу
                    CaptchaInputBox.Text = "";

                    // Блокируем вход на 10 секунд
                    cantLogin = true;
                    enterButton.IsEnabled = false;
                    await CaptchaBlock();
                    enterButton.IsEnabled = true;
                    return;
                }
            }

            if (userObj != null)
            {
                // Перенаправляем в зависимости от роли
                var role = db.PrkRoles.FirstOrDefault(r => r.IdRole == userObj.IdRole);

                if (role?.Role == "Администратор")
                {
                    NavigationService.Navigate(new AdministratorPage(userObj));
                }
                else if (role?.Role == "Менеджер")
                {
                    NavigationService.Navigate(new ManagerPage(userObj));
                }
                else
                {
                    NavigationService.Navigate(new ClientPage(userObj));
                }
            }
            else
            {
                firstAttempt = false;
                errorBlock.Text = "Неверный логин или пароль!";
                errorBlock.Visibility = Visibility.Visible;

                // Показываем капчу
                CaptchaBorder.Visibility = Visibility.Visible;
                CaptchaInputPanel.Visibility = Visibility.Visible;
                GenerateCaptcha();

                await HideError();
            }
        }

        public async Task CaptchaBlock()
        {
            int n = 10;
            while (n >= 0)
            {
                errorBlock.Text = $"Повторите попытку через {n} сек.";
                await Task.Delay(1000);
                n--;
                if (n < 0)
                {
                    cantLogin = false;
                    errorBlock.Visibility = Visibility.Collapsed;
                    break;
                }
            }
        }

        public async Task HideError()
        {
            await Task.Delay(3000);
            errorBlock.Visibility = Visibility.Collapsed;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            NavigationService.Navigate(new GuestPage());
        }
    }
}
