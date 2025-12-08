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

namespace Yunusov41
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage(User user)
        {
            InitializeComponent();
            UserNameTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
            switch (user.UserRole)
            {
                case 1:
                    RoleNameTB.Text = "Клиент"; break;
                case 2:
                    RoleNameTB.Text = "Менеджер"; break;
                case 3:
                    RoleNameTB.Text = "Администратор"; break;
            }

            var currentProduct = Yunusov41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProduct;
            //ComboType.SelectedIndex = 0;

            var totalProducts = Yunusov41Entities.GetContext().Product.Count();
            RecordCounter.Text = $"{totalProducts} из {totalProducts} записей";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }
        private void UpdateProducts()
        {
            var currentProducts = Yunusov41Entities.GetContext().Product.ToList();
            if (ComboType.SelectedIndex == 0)
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount)>= 0 && Convert.ToInt32(p.ProductDiscountAmount) <=100)).ToList();
            
            if (ComboType.SelectedIndex == 1)
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) < 10)).ToList();
           
            if (ComboType.SelectedIndex == 2)
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 10 && Convert.ToInt32(p.ProductDiscountAmount) < 15)).ToList();

            if (ComboType.SelectedIndex == 3)
                currentProducts = currentProducts.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 15 && Convert.ToInt32(p.ProductDiscountAmount) <= 100)).ToList();

            currentProducts =  currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            ProductListView.ItemsSource = currentProducts.ToList();
            if(RButtonDown.IsChecked.Value)
                ProductListView.ItemsSource=currentProducts.OrderByDescending(p => p.ProductCost).ToList();

            if (RButtonUp.IsChecked.Value)
                ProductListView.ItemsSource = currentProducts.OrderBy(p => p.ProductCost).ToList();

            var filteredProducts = currentProducts.ToList();
            var totalProducts = Yunusov41Entities.GetContext().Product.Count();
            RecordCounter.Text = $"{filteredProducts.Count} из {totalProducts} записей";
        }



        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }
    }
}
