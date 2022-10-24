using Newtonsoft.Json;
using SkillboxWPF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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

namespace SkillboxWPF
{
    public partial class ServiceView : Window
    {
        private HtmlTemplate _html;
        private bool _edit;
        private HttpClient _client;

        public ServiceView(HtmlTemplate html, bool edit, HttpClient client)
        {
            InitializeComponent();
            _html = html;
            _edit = edit;
            _client = client;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBox.Text = _html.Text;
            this.titleBox.Text = _html.Title;
            this.Title = _html.Title;

            if (!_edit)
            {
                saveBtn.Visibility = Visibility.Hidden;
                this.textBox.IsReadOnly = true;
                this.titleBox.IsReadOnly = true;
            }
        }

        private async void saveBtn_Click(object sender, RoutedEventArgs e) => await SaveHtml();

        private async Task SaveHtml()
        {
            try
            {
                _html.Text = this.textBox.Text;
                _html.Title = this.titleBox.Text;

                var json = JsonConvert.SerializeObject(_html);
                var PostAddFav = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "http://localhost:36255/api/Home/updateHtml");
                PostAddFav.Content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                var PostAddFavSend = await _client.SendAsync(PostAddFav);
                var text = await PostAddFavSend.Content.ReadAsStringAsync();

                this.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка при обновлении {exc.Message}");
                this.Close();
            }
        }

    }
}
