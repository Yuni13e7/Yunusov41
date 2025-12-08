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
    public partial class AuthPage : Page
    {
        private string _currentCaptcha = "";
        private int _failedAttempts = 0;
        private bool _isBlocked = false;
        private System.Windows.Threading.DispatcherTimer _blockTimer;

        public AuthPage()
        {
            InitializeComponent();
            InitializeBlockTimer();
        }

        private void InitializeBlockTimer()
        {
            _blockTimer = new System.Windows.Threading.DispatcherTimer();
            _blockTimer.Interval = TimeSpan.FromSeconds(10);
            _blockTimer.Tick += BlockTimer_Tick;
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            _isBlocked = false;
            _blockTimer.Stop();
            BtnLogIn.IsEnabled = true;
        }

        private void GenerateCaptcha()
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            _currentCaptcha = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            captchaOneWord.Text = _currentCaptcha[0].ToString();
            captchaTwoWord.Text = _currentCaptcha[1].ToString();
            captchaThreeWord.Text = _currentCaptcha[2].ToString();
            captchaFourWord.Text = _currentCaptcha[3].ToString();

            ApplyNoiseFormatting();
        }

        private void ApplyNoiseFormatting()
        {
            var random = new Random();

            captchaOneWord.RenderTransform = new RotateTransform(random.Next(-15, 15));
            captchaTwoWord.RenderTransform = new RotateTransform(random.Next(-15, 15));
            captchaThreeWord.RenderTransform = new RotateTransform(random.Next(-15, 15));
            captchaFourWord.RenderTransform = new RotateTransform(random.Next(-15, 15));
        }

        private void ShowCaptcha()
        {
            CaptchaPanel.Visibility = Visibility.Visible;
            TBoxCaptcha.Visibility = Visibility.Visible;
            GenerateCaptcha();
        }

        private void HideCaptcha()
        {
            CaptchaPanel.Visibility = Visibility.Collapsed;
            TBoxCaptcha.Visibility = Visibility.Collapsed;
            TBoxCaptcha.Text = "";
        }

        private async void BtnLogIn_Click(object sender, RoutedEventArgs e)
        {
            if (_isBlocked)
            {
                MessageBox.Show("Система заблокирована. Подождите 10 секунд.");
                return;
            }

            string login = TBoxLogin.Text;
            string password = TBoxPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                string captchaInput = TBoxCaptcha.Text;

                if (string.IsNullOrEmpty(captchaInput))
                {
                    MessageBox.Show("Введите капчу");
                    return;
                }

                if (!captchaInput.Equals(_currentCaptcha, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Неверная капча");
                    BlockSystemFor10Seconds();
                    GenerateCaptcha();
                    TBoxCaptcha.Text = "";
                    return;
                }
            }

            User user = Yunusov41Entities.GetContext().User.ToList()
                .Find(p => p.UserLogin == login && p.UserPassword == password);

            if (user != null)
            {
                _failedAttempts = 0;
                HideCaptcha();
                Manager.MainFrame.Navigate(new ProductPage(user));
                TBoxLogin.Text = "";
                TBoxPassword.Text = "";
                TBoxCaptcha.Text = "";
            }
            else
            {
                _failedAttempts++;

                if (_failedAttempts == 1)
                {
                    MessageBox.Show("Введены неверные данные. Требуется ввод капчи.");
                    ShowCaptcha();
                }
                else if (_failedAttempts >= 2)
                {
                    MessageBox.Show("Введены неверные данные или капча");
                    BlockSystemFor10Seconds();
                    GenerateCaptcha();
                    TBoxCaptcha.Text = "";
                }
                else
                {
                    MessageBox.Show("Введены неверные данные");
                }
            }
        }

        private void BlockSystemFor10Seconds()
        {
            _isBlocked = true;
            BtnLogIn.IsEnabled = false;
            _blockTimer.Start();
        }

        private void TBoxCaptcha_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void BtnLogInAsAGuest_Click(object sender, RoutedEventArgs e)
        {
            User guestUser = new User
            {
                UserID = 0,
                UserLogin = "Guest",
                UserPassword = "",
                UserName = "Гость"
            };

            Manager.MainFrame.Navigate(new ProductPage(guestUser));
            TBoxLogin.Text = "";
            TBoxPassword.Text = "";
            TBoxCaptcha.Text = "";
            HideCaptcha();
            _failedAttempts = 0;
        }

        private void TBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TBoxPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}