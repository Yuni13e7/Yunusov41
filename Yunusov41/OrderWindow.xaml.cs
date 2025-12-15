using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yunusov41
{
    public partial class OrderWindow : Window
    {
        private List<OrderProduct> selectedOrderProducts;
        private List<Product> selectedProducts;
        private User currentUser;
        public bool OrderSaved { get; private set; } = false;
        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO, User user = null)
        {
            InitializeComponent();
            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;
            this.currentUser = user;
            InitializeWindow(FIO);
        }
       private void InitializeWindow(string FIO)
{
    try
    {
        var currentPickups = Yunusov41Entities.GetContext().PickupPoint.ToList();
        PickupCombo.ItemsSource = currentPickups;
        
        PickupCombo.DisplayMemberPath = "FullAddress";
        PickupCombo.SelectedValuePath = "PickupPointID";

        if (currentPickups.Count > 0)
            PickupCombo.SelectedIndex = 0;

        ClientTB.Text = FIO;
        
        int orderId = 1;
        var lastOrder = Yunusov41Entities.GetContext().Order
            .OrderByDescending(o => o.OrderID)
            .FirstOrDefault();
        if (lastOrder != null)
            orderId = lastOrder.OrderID + 1;

        TBOrderID.Text = orderId.ToString();

        ProductListView2.ItemsSource = selectedProducts;
        
        foreach (Product p in selectedProducts)
        {
            p.OrderQuantity = 1;
            
            var orderProduct = selectedOrderProducts
                .FirstOrDefault(q => p.ProductArticleNumber == q.ProductArticleNumber);
            
            if (orderProduct != null)
            {
                p.OrderQuantity = orderProduct.Count;
            }
        }
        
        OrderDateTB.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        
        SetDeliveryDate();
        CalculateSums();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка инициализации: {ex.Message}\n" +
                      $"StackTrace: {ex.StackTrace}",
                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
        private void SetDeliveryDate()
        {
            try
            {
                int deliveryDays = 6; 
                foreach (var orderProduct in selectedOrderProducts)
                {
                    var product = selectedProducts
                        .FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);

                    if (product != null)
                    {
                        if (product.ProductQuantityInStock < 4)
                        {
                            deliveryDays = 6;
                            break;
                        }
                        else
                        {
                         
                            deliveryDays = 3;
                        }
                    }
                }

                DeliveryDateTB.Text = DateTime.Now.AddDays(deliveryDays).ToString("dd.MM.yyyy");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета даты доставки: {ex.Message}");
                DeliveryDateTB.Text = DateTime.Now.AddDays(6).ToString("dd.MM.yyyy");
            }
        }
        private void CalculateSums()
        {
            try
            {
                decimal totalSum = 0;
                decimal discountedSum = 0;

                foreach (var orderProduct in selectedOrderProducts)
                {
                    var product = selectedProducts
                        .FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);

                    if (product != null)
                    {
                        decimal productCost = 0;
                        if (product.ProductCost != null)
                        {
                            try
                            {
                                productCost = Convert.ToDecimal(product.ProductCost);
                            }
                            catch
                            {
                                productCost = 0;
                            }
                        }

                        decimal itemTotal = productCost * orderProduct.Count;

           
                        decimal discountPercent = 0;
                        if (product.ProductDiscountAmount != null)
                        {
                            try
                            {
                                discountPercent = Convert.ToDecimal(product.ProductDiscountAmount);
                            }
                            catch
                            {
                                discountPercent = 0;
                            }
                        }

                        decimal discountAmount = itemTotal * (discountPercent / 100m);
                        decimal itemDiscountedTotal = itemTotal - discountAmount;

                        totalSum += itemTotal;
                        discountedSum += itemDiscountedTotal;
                    }
                }

                TotalSumTB.Text = totalSum.ToString("0.00") + " руб";
                DiscountedSumTB.Text = discountedSum.ToString("0.00") + " руб";

                ProductListView2.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета сумм: {ex.Message}");
            }
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var prod = button?.DataContext as Product;

                if (prod != null)
                {
                    
                    if (prod.OrderQuantity < prod.ProductQuantityInStock)
                    {
                        prod.OrderQuantity++;

                    
                        var orderProduct = selectedOrderProducts
                            .FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

                        if (orderProduct != null)
                        {
                            orderProduct.Count = prod.OrderQuantity;
                        }
                    
                        ProductListView2.Items.Refresh();
                    
                        CalculateSums();
                        SetDeliveryDate();
                    }
                    else
                    {
                        MessageBox.Show($"На складе доступно только {prod.ProductQuantityInStock} шт.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка увеличения количества: {ex.Message}");
            }
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var prod = button?.DataContext as Product;

                if (prod != null)
                {
                    if (prod.OrderQuantity > 1)
                    {
                        prod.OrderQuantity--;

                    
                        var orderProduct = selectedOrderProducts
                            .FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

                        if (orderProduct != null)
                        {
                            orderProduct.Count = prod.OrderQuantity;
                        }

                        ProductListView2.Items.Refresh();
                        CalculateSums();
                        SetDeliveryDate();
                    }
                    else if (prod.OrderQuantity == 1)
                    {
                    
                        prod.OrderQuantity = 0;

                    
                        var orderProduct = selectedOrderProducts
                            .FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

                        if (orderProduct != null)
                        {
                            selectedOrderProducts.Remove(orderProduct);
                        }

                    
                        selectedProducts.Remove(prod);
                        ProductListView2.ItemsSource = null;
                        ProductListView2.ItemsSource = selectedProducts;

                        CalculateSums();
                        SetDeliveryDate();

                        if (selectedOrderProducts.Count == 0)
                        {
                            MessageBox.Show("В заказе не осталось товаров");
                            Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка уменьшения количества: {ex.Message}");
            }
        }

        private void PickupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PickupCombo.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пункт выдачи", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selectedOrderProducts.Count == 0)
                {
                    MessageBox.Show("Добавьте товары в заказ", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                foreach (var orderProduct in selectedOrderProducts)
                {
                    var product = selectedProducts
                        .FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);

                    if (product != null && orderProduct.Count > product.ProductQuantityInStock)
                    {
                        MessageBox.Show($"Товар '{product.ProductName}' доступен только в количестве {product.ProductQuantityInStock} шт.",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                var selectedPickup = (PickupPoint)PickupCombo.SelectedItem;

                string orderClientValue = null;
                if (currentUser != null && currentUser.UserID > 0)
                {
                    orderClientValue = currentUser.UserID.ToString();
                }

                var newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    OrderDeliveryDate = DateTime.Parse(DeliveryDateTB.Text),
                    OrderPickupPoint = selectedPickup.PickupPointID,
                    OrderClient = orderClientValue,
                    OrderCode = GenerateOrderCode(),
                    OrderStatus = "1",
                    PickupPoint = selectedPickup
                };

                var context = Yunusov41Entities.GetContext();
                context.Order.Add(newOrder);
                context.SaveChanges();

                foreach (var orderProduct in selectedOrderProducts)
                {
                    var dbOrderProduct = new OrderProduct
                    {
                        OrderID = newOrder.OrderID,
                        ProductArticleNumber = orderProduct.ProductArticleNumber,
                        Count = orderProduct.Count
                    };
                    context.OrderProduct.Add(dbOrderProduct);

                    var product = context.Product
                        .FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);

                    if (product != null)
                    {
                        product.ProductQuantityInStock -= orderProduct.Count;
                    }
                }

                context.SaveChanges();

                OrderSaved = true;

                MessageBox.Show($"Заказ №{newOrder.OrderID} успешно сохранен!\n" +
                              $"Код для получения: {newOrder.OrderCode}\n" +
                              $"Пункт выдачи: {selectedPickup.FullAddress}",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}\n" +
                              $"Детали: {ex.InnerException?.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private int GenerateOrderCode()
        {
            try
            {
                Random random = new Random();
                return random.Next(100000, 999999);
            }
            catch
            {
                return int.Parse(DateTime.Now.ToString("HHmmss"));
            }
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}