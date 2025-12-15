using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Yunusov41
{
    public partial class ProductPage : Page
    {
        private User currentUser;
        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        private List<Product> selectedProducts = new List<Product>();

        public ProductPage(User user)
        {
            InitializeComponent();
            currentUser = user;

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

            var totalProducts = Yunusov41Entities.GetContext().Product.Count();
            RecordCounter.Text = $"{totalProducts} из {totalProducts} записей";

            foreach (var product in currentProduct)
            {
                product.OrderQuantity = 1;
            }
        }

        private void UpdateOrderButtonVisibility()
        {
            if (selectedOrderProducts.Count > 0)
            {
                OrderBtn.Visibility = Visibility.Visible;
            }
            else
            {
                OrderBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateProducts()
        {
            var currentProducts = Yunusov41Entities.GetContext().Product.ToList();

            if (ComboType.SelectedIndex == 0)
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount <= 100)).ToList();

            if (ComboType.SelectedIndex == 1)
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount < 10)).ToList();

            if (ComboType.SelectedIndex == 2)
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 10 && p.ProductDiscountAmount < 15)).ToList();

            if (ComboType.SelectedIndex == 3)
                currentProducts = currentProducts.Where(p => (p.ProductDiscountAmount >= 15 && p.ProductDiscountAmount <= 100)).ToList();

            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (RButtonDown.IsChecked.Value)
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();

            if (RButtonUp.IsChecked.Value)
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();

            ProductListView.ItemsSource = currentProducts;

            var totalProducts = Yunusov41Entities.GetContext().Product.Count();
            RecordCounter.Text = $"{currentProducts.Count} из {totalProducts} записей";
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
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddToOrder();
        }

        private void ProductListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            AddToOrder();
        }

        private void AddToOrder()
        {
            if (ProductListView.SelectedItem is Product selectedProduct)
            {
                if (selectedProduct.ProductQuantityInStock <= 0)
                {
                    MessageBox.Show("Товар отсутствует на складе", "Внимание",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var existingOrderProduct = selectedOrderProducts
                    .FirstOrDefault(op => op.ProductArticleNumber == selectedProduct.ProductArticleNumber);

                if (existingOrderProduct != null)
                {
                    if (existingOrderProduct.Count < selectedProduct.ProductQuantityInStock)
                    {
                        existingOrderProduct.Count++;
                        selectedProduct.OrderQuantity = existingOrderProduct.Count;
                        MessageBox.Show($"Товар добавлен. Количество: {existingOrderProduct.Count}");
                    }
                    else
                    {
                        MessageBox.Show($"Невозможно добавить больше товара. Доступно: {selectedProduct.ProductQuantityInStock} шт.");
                    }
                }
                else
                {
                    var newOrderProduct = new OrderProduct
                    {
                        ProductArticleNumber = selectedProduct.ProductArticleNumber,
                        Count = 1,
                        Product = selectedProduct
                    };
                    selectedOrderProducts.Add(newOrderProduct);

                    if (!selectedProducts.Contains(selectedProduct))
                    {
                        selectedProducts.Add(selectedProduct);
                    }

                    selectedProduct.OrderQuantity = 1;
                    MessageBox.Show("Товар добавлен к заказу");
                }

                UpdateOrderButtonVisibility();
                ProductListView.SelectedIndex = -1;
            }
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrderProducts.Count == 0)
            {
                MessageBox.Show("В заказе нет товаров");
                return;
            }
            string clientFIO = "Гость";
            if (currentUser.UserID > 0)
            {
                clientFIO = currentUser.UserSurname + " " + currentUser.UserName + " " + currentUser.UserPatronymic;
            }

            OrderWindow orderWindow = new OrderWindow(selectedOrderProducts, selectedProducts, clientFIO, currentUser);
            orderWindow.Owner = Application.Current.MainWindow;
            orderWindow.ShowDialog();

            if (orderWindow.OrderSaved)
            {
                selectedOrderProducts.Clear();
                selectedProducts.Clear();

                foreach (var product in ProductListView.ItemsSource as List<Product> ?? new List<Product>())
                {
                    product.OrderQuantity = 1;
                }

                UpdateOrderButtonVisibility();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }
    }
}