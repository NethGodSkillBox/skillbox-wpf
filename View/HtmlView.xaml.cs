using Microsoft.Win32;
using Newtonsoft.Json;
using SkillboxWPF.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
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

    public partial class HtmlView : Window
    {
        private HtmlTemplate _html;
        private bool _edit;
        private HttpClient _client;

        public HtmlView(HtmlTemplate html, bool edit, HttpClient client)
        {
            InitializeComponent();
            _html = html;
            _edit = edit;
            _client = client;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_html.Type == "blog")
                    this.textBox.Height = 120;
                
                if (_html.Img != null)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    byte[] binaryData = Convert.FromBase64String(_html.Img.Substring(23, _html.Img.Length - 23));
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.EndInit();
                    this.imgBox.Source = bi;
                }

                this.textBox.Text = _html.Text;
                this.infoBox.Text = _html.Info;
                this.Title = _html.Title;

                if (!_edit)
                {
                    saveBtn.Visibility = Visibility.Hidden;
                    fileBtn.Visibility = Visibility.Hidden;
                    this.textBox.IsReadOnly = true;
                    this.infoBox.IsReadOnly = true;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка загрузки информации {exc.Message}");
            }
        }
        private void fileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                this.imgBox.Source = new BitmapImage(new Uri(op.FileName));
            }
        }
        private async void saveBtn_Click(object sender, RoutedEventArgs e) => await SaveHtml();


        private async Task SaveHtml()
        {
            try
            {
                BitmapImage bi = (BitmapImage)imgBox.Source;
                MemoryStream memStream = new MemoryStream();
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bi));
                encoder.Save(memStream);
                var base64String = Convert.ToBase64String(memStream.GetBuffer());

                _html.Text = this.textBox.Text;
                _html.Info = this.infoBox.Text;
                _html.Img = "data:image/jpeg;base64," + base64String;

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
