using MusicalInstrument.DataBase;
using MusicalInstrument.Windows;
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

namespace MusicalInstrument.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        DBEntities db = new DBEntities();
        PrkUsers currentUser;
        List<PrlProducts> allProducts;
        List<PrkSuppliers> suppliersList;

        public ManagerPage(PrkUsers user)
        {
            InitializeComponent();
            currentUser = user;

            db.Configuration.ProxyCreationEnabled = false;

            LoadData();
            UpdateCountInfo();

            FIOBlok.Text = $"{user.Surname} {user.Name} {user.Patronymic}";

            var role = db.PrkRoles.FirstOrDefault(r => r.IdRole == user.IdRole);
            RoleBlok.Text = role?.Role ?? "Менеджер";

            filterBox.ItemsSource = new List<string>()
            {
                "Наименование",
                "Описание",
                "Производитель",
                "Поставщик",
                "Категория",
                "Артикул",
                "Количество"
            };
            filterBox.SelectedIndex = 0;

            // Заполняем поставщиков с пунктом "Все поставщики"
            suppliersList = db.PrkSuppliers.ToList();
            var suppliersWithAll = new List<KeyValuePair<int, string>>();
            suppliersWithAll.Add(new KeyValuePair<int, string>(-1, "Все поставщики"));
            foreach (var s in suppliersList)
            {
                suppliersWithAll.Add(new KeyValuePair<int, string>(s.IdSupplier, s.Supplier));
            }

            supplierFilterBox.ItemsSource = suppliersWithAll;
            supplierFilterBox.DisplayMemberPath = "Value";
            supplierFilterBox.SelectedValuePath = "Key";
            supplierFilterBox.SelectedIndex = 0;

            UpdateCountInfo();
        }

        private void LoadData()
        {
            allProducts = db.PrlProducts
                .Include("PrkSuppliers")
                .Include("PrkManufacturers")
                .Include("PrkCategotyOrders")
                .Include("PrkUunitOfMeasurements")
                .ToList();
            ProductsList.ItemsSource = allProducts;
        }

        private void UpdateCountInfo()
        {
            var currentList = ProductsList.ItemsSource as System.Collections.IEnumerable;
            int displayedCount = currentList?.Cast<object>().Count() ?? 0;
            int totalCount = allProducts.Count;
            CountInfoBlock.Text = $"Показано: {displayedCount} из {totalCount}";
        }

        private void ApplyFilter()
        {
            var filteredList = allProducts.AsEnumerable();

            string searchText = searchBox.Text?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredList = filteredList.Where(p =>
                    (p.NameOrder != null && p.NameOrder.ToLower().Contains(searchText)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchText)) ||
                    (p.Article != null && p.Article.ToLower().Contains(searchText)) ||
                    (p.PrkManufacturers != null && p.PrkManufacturers.Manufacturer != null && p.PrkManufacturers.Manufacturer.ToLower().Contains(searchText)) ||
                    (p.PrkSuppliers != null && p.PrkSuppliers.Supplier != null && p.PrkSuppliers.Supplier.ToLower().Contains(searchText)) ||
                    (p.PrkCategotyOrders != null && p.PrkCategotyOrders.CategotyOrder != null && p.PrkCategotyOrders.CategotyOrder.ToLower().Contains(searchText))
                );
            }

            if (supplierFilterBox.SelectedIndex > 0)
            {
                var selectedSupplier = (KeyValuePair<int, string>)supplierFilterBox.SelectedItem;
                if (selectedSupplier.Key > 0)
                {
                    filteredList = filteredList.Where(p => p.IdSupplier == selectedSupplier.Key);
                }
            }

            int sortField = filterBox.SelectedIndex;
            bool isAscending = sortBox.SelectedIndex == 0;

            if (sortField != -1)
            {
                switch (sortField)
                {
                    case 0:
                        filteredList = isAscending ? filteredList.OrderBy(p => p.NameOrder) : filteredList.OrderByDescending(p => p.NameOrder);
                        break;
                    case 1:
                        filteredList = isAscending ? filteredList.OrderBy(p => p.Description) : filteredList.OrderByDescending(p => p.Description);
                        break;
                    case 2:
                        filteredList = isAscending ? filteredList.OrderBy(p => p.PrkManufacturers?.Manufacturer) : filteredList.OrderByDescending(p => p.PrkManufacturers?.Manufacturer);
                        break;
                    case 3:
                        filteredList = isAscending ? filteredList.OrderBy(p => p.PrkSuppliers?.Supplier) : filteredList.OrderByDescending(p => p.PrkSuppliers?.Supplier);
                        break;
                    case 4:
                        filteredList = isAscending ? filteredList.OrderBy(p => p.PrkCategotyOrders?.CategotyOrder) : filteredList.OrderByDescending(p => p.PrkCategotyOrders?.CategotyOrder);
                        break;
                    case 5:
                        filteredList = isAscending ? filteredList.OrderBy(p => p.Article) : filteredList.OrderByDescending(p => p.Article);
                        break;
                    case 6:
                        filteredList = isAscending ? filteredList.OrderBy(p => int.Parse(p.QuantityInStock)) : filteredList.OrderByDescending(p => int.Parse(p.QuantityInStock));
                        break;
                }
            }

            ProductsList.ItemsSource = filteredList.ToList();
            UpdateCountInfo();
        }

        private void filterBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();
        private void sortBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();
        private void searchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
        private void supplierFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

        private void exitButton_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();

        // Единственная кнопка - "Детали заказов"
        private void ordersButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersWindow = new OrdersListWindow();
            ordersWindow.Owner = Window.GetWindow(this);
            ordersWindow.ShowDialog();
        }
    }
}