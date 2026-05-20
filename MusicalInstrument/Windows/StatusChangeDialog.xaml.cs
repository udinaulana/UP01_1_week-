using MusicalInstrument.DataBase;
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
using System.Windows.Shapes;

namespace MusicalInstrument.Windows
{
    /// <summary>
    /// Логика взаимодействия для StatusChangeDialog.xaml
    /// </summary>
    public partial class StatusChangeDialog : Window
    {
        PrkOrders order;
        DBEntities db = new DBEntities();

        public StatusChangeDialog(PrkOrders order, System.Collections.Generic.List<PrkOrderStatuses> statuses)
        {
            InitializeComponent();
            this.order = order;
            StatusCombo.ItemsSource = statuses;
            StatusCombo.SelectedValue = order.IdOrderStatus;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (StatusCombo.SelectedValue != null)
            {
                order.IdOrderStatus = (int)StatusCombo.SelectedValue;
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}