using Demka.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Demka.window_xaml
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
            LoginTB.Text = "94d5ous@gmail.com";
            PasswordTB.Text = "uzWC67";
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Успешный вход!","Уведомление");
            Login();
        }
        private void LoginAsGuestBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginAsGuest();
        }

        public void Login()
        {
            if (CheckInputs())
            {
                ShoeContext context = new ShoeContext();
                UserImport? user = context.UserImports.FirstOrDefault(u => u.LoginUserImport == LoginTB.Text
                && u.PasswordUserImport == PasswordTB.Text);
                if (user == null)
                {
                    MessageBox.Show("Неверные данные!", "Уведомление",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Environment.SetEnvironmentVariable("USERID", user.IdUserImport.ToString(), EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("USERROLE", user.RoleUserImport, EnvironmentVariableTarget.Process);
                new UserWindow().Show();
                this.Close();
            }


        }

        public void LoginAsGuest()
        {
            Environment.SetEnvironmentVariable("USERID", "none", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("USERROLE", "none", EnvironmentVariableTarget.Process);
            new UserWindow().Show();
            this.Close();
        }

        public bool CheckInputs()
        {
            if (string.IsNullOrWhiteSpace(LoginTB.Text) || string.IsNullOrWhiteSpace(PasswordTB.Text))
            {
                MessageBox.Show("Заполните все поля!", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Regex.IsMatch(LoginTB.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("Введите корректную почту", "Уведомление",
                       MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (PasswordTB.Text.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов!", "Уведомление",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
