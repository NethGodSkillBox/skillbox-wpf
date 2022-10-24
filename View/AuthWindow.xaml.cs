using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SkillboxWPF
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private HttpClient _client;
        private string login;
        private string pass;

        public AuthWindow(HttpClient client)
        {
            InitializeComponent();
            _client = client;
        }

        private async void authBtn_Click(object sender, RoutedEventArgs e)
        {
            login = this.loginBox.Text;
            pass = this.passBox.Text;

            await Auth();
            this.Close();
            //Thread work = new Thread(async () => { await Auth(); });
            //work.IsBackground = true;
            //work.Start();
        }

        private async Task Auth()
        {
            var auth = await AuthControl.Auth(_client, login, pass);
            if (!auth)
                MessageBox.Show("Не удалось авторизоваться");
            else
            {
                MessageBox.Show("Успешно");
                this.Close();
            }

        }
    }
}
