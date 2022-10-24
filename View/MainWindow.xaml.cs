using Newtonsoft.Json;
using SkillboxWPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkillboxWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BindingList<Req> _reqs = new BindingList<Req>();
        private BindingList<HtmlTemplate> _html = new BindingList<HtmlTemplate>();
        private HttpClient _client = new HttpClient();

        public bool check
        {
            get { return _check; }
            set
            {
                try
                {
                    _check = value;

                    if (check)
                    {
                        GetReqs();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            nickLabel.Content = nickLabel.Content = $"Вы вошли как: {AuthControl.GetLogin(_client)}"; ;
                            logoutBtn.Visibility = Visibility.Visible;
                            addContactBtn.Visibility = Visibility.Visible;
                            saveContactBtn.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            reqsGrid.ItemsSource = null;
                            nickLabel.Content = "Вы вошли как: Гость";
                            logoutBtn.Visibility = Visibility.Hidden;
                            addContactBtn.Visibility = Visibility.Hidden;
                            saveContactBtn.Visibility = Visibility.Hidden;

                        });
                        _client.DefaultRequestHeaders.Clear();
                    }

                    Application.Current.Dispatcher.Invoke(() => { LoadHtmls(); });
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"Ошибка {exc.StackTrace}");
                }
            }
        }
        private bool _check;


        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e) => await LoadHtmls();


        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            Thread work = new Thread(async () => { await Auth(); });
            work.IsBackground = true;
            work.Start();
        }
        private void logoutBtn_Click(object sender, RoutedEventArgs e) => check = false;
        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();
        private void closeBtn_Click(object sender, RoutedEventArgs e) => this.Close();
        private async void minimizeBtn_Click(object sender, RoutedEventArgs e) { await Task.Delay(100); this.WindowState = WindowState.Minimized; }
        private async void sendReqBtn_Click(object sender, RoutedEventArgs e) => await SendReq();
        private async void saveContactBtn_Click(object sender, RoutedEventArgs e) => await SaveContact();
        private void addContactBtn_Click(object sender, RoutedEventArgs e) => AddContacts();
        private async void refreshButton_Click(object sender, RoutedEventArgs e) => await LoadHtmls();


        private void projectGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() != "Title"
                && e.Column.Header.ToString() != "Text"
                && e.Column.Header.ToString() != "Text")
                e.Column.Visibility = Visibility.Hidden;
            else if (e.Column.Header.ToString() == "Title")
                e.Column.Width = 250;
            else if (e.Column.Header.ToString() == "Text")
                e.Column.Width = 550;

            if (e.Column.Header.ToString() == "Link")
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new EventSetter(Hyperlink.ClickEvent, (RoutedEventHandler)EventSetter_OnHandler));

                DataGridHyperlinkColumn col = new DataGridHyperlinkColumn() { ElementStyle = style };
                col.Header = e.Column.Header;
                Binding binding = new Binding(e.PropertyName);
                col.Binding = binding;
                e.Column = col;
            }
            else if (e.Column.Header.ToString() == "Edit")
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new EventSetter(Hyperlink.ClickEvent, (RoutedEventHandler)EventSetter_OnHandler));

                DataGridHyperlinkColumn col = new DataGridHyperlinkColumn() { ElementStyle = style };
                col.Header = e.Column.Header;
                Binding binding = new Binding(e.PropertyName);
                col.Binding = binding;
                e.Column = col;
            }
        }
        private void serviceGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() != "Link"
                && e.Column.Header.ToString() != "Title"
                && e.Column.Header.ToString() != "Text")
                e.Column.Visibility = Visibility.Hidden;

            if (e.Column.Header.ToString() == "Text")
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new EventSetter(Hyperlink.ClickEvent, (RoutedEventHandler)EventSetter_OnHandler));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

                DataGridTextColumn col = new DataGridTextColumn() { ElementStyle = style };

                col.Header = e.Column.Header;
                Binding binding = new Binding(e.PropertyName);
                col.Binding = binding;
                col.Width = 650;
                e.Column = col;
            }
            else if (e.Column.Header.ToString() == "Link")
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new EventSetter(Hyperlink.ClickEvent, (RoutedEventHandler)EventSetter_OnHandler));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

                DataGridHyperlinkColumn col = new DataGridHyperlinkColumn() { ElementStyle = style };

                col.Header = e.Column.Header;
                Binding binding = new Binding(e.PropertyName);
                col.Binding = binding;
                e.Column = col;
            }

        }
        private void reqsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            try
            {
                if (e.Column.Header.ToString() == "Status")
                {
                    DataGridComboBoxColumn col = new DataGridComboBoxColumn();
                    Binding binding = new Binding(e.PropertyName);
                    string[] list = new string[] { "Получена", "В работе", "Выполнена", "Отклонена", "Отменена", };
                    col.ItemsSource = list;
                    col.SelectedValueBinding = binding;
                    col.Header = e.Column.Header;
                    col.EditingElementStyle = new Style(typeof(ComboBox))
                    {
                        Setters =
                        {
                            new EventSetter(Selector.SelectionChangedEvent, new SelectionChangedEventHandler(OnComboBoxSelectionChanged))
                        }
                    };
                    e.Column = col;
                }
                else
                    e.Column.IsReadOnly = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.StackTrace);
            }
        }


        private async Task LoadHtmls()
        {
            try
            {
                if(check)
                    GetReqs();

                var get2 = await _client.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "http://localhost:36255/api/Home/getHtml"));
                var text = await get2.Content.ReadAsStringAsync();
                _html = JsonConvert.DeserializeObject<BindingList<HtmlTemplate>>(text);

                projectGrid.ItemsSource = _html.Where(x => x.Type == "project");
                blogGrid.ItemsSource = _html.Where(x => x.Type == "blog");
                serviceGrid.ItemsSource = _html.Where(x => x.Type == "service");

                contactPanel.Children.Clear();

                foreach (var item in _html.Where(x => x.Type == "contact"))
                {
                    TextBox block = new TextBox();
                    if (!check)
                        block.IsReadOnly = true;

                    block.Name = $"name{item.Id}";
                    block.Text = item.Text;
                    block.FontSize = 16;
                    block.Margin = new Thickness(0, 20, 0, 0);
                    block.HorizontalAlignment = HorizontalAlignment.Center;
                    contactPanel.Children.Add(block);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.StackTrace}");
            }
        }
        private async Task Auth()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var aw = new AuthWindow(_client);
                    aw.ShowDialog();
                });

                check = await AuthControl.Check(_client);
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.Message}");
            }
        }
        private async Task SendReq()
        {
            try
            {
                var req = new Req();
                req.Email = emailBox.Text;
                req.Name = nameBox.Text;
                req.Text = reqTextBox.Text;
                req.Time = DateTime.Now;
                req.Status = "Получена";

                string json = JsonConvert.SerializeObject(req);

                var PostAddFav = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "http://localhost:36255/api/Home/addReq");
                PostAddFav.Content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                var PostAddFavSend = await _client.SendAsync(PostAddFav);
                var text = await PostAddFavSend.Content.ReadAsStringAsync();

                if (text.Contains("Заявка добавлена"))
                    MessageBox.Show("Заявка успешно отправлена");
                else
                    MessageBox.Show("Не удалось отправить заявку.\nПопробуйте позже");

                Application.Current.Dispatcher.Invoke(() =>
                {
                });

            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.Message}");
            }
        }
        private async Task SaveContact()
        {
            try
            {
                foreach (var item in contactPanel.Children)
                {
                    var box = (TextBox)item;
                    var html = _html.FirstOrDefault(x => x.Id.ToString() == box.Name.Replace("name", ""));
                    if (html != null)
                    {
                        if (html.Text == box.Text && box.Text != "") continue;
                        else if (box.Text == "")
                        {
                            await UpdateContact(html, "remove");
                            continue;
                        }
                        html.Text = box.Text;
                    }
                    else if (box.Text != "")
                        html = new HtmlTemplate() { Text = box.Text, Time = DateTime.Now, Type = "contact" };
                    else
                        continue;

                    await UpdateContact(html, "edit");
                }

                MessageBox.Show("Обновлено");
                await LoadHtmls();
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.StackTrace}");
            }
        }
        private async Task UpdateContact(HtmlTemplate html, string type)
        {
            string url = "";

            if (type == "edit")
                url = "http://localhost:36255/api/Home/updateHtml";
            else
                url = "http://localhost:36255/api/Home/removeHtml";
            try
            {
                var json = JsonConvert.SerializeObject(html);
                var PostAddFav = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                PostAddFav.Content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                var PostAddFavSend = await _client.SendAsync(PostAddFav);
                var text = await PostAddFavSend.Content.ReadAsStringAsync();
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.StackTrace}");
            }
        }

        private void AddContacts()
        {
            TextBox block = new TextBox();
            block.Width = 200;
            block.FontSize = 16;
            block.Margin = new Thickness(0, 20, 0, 0);
            block.HorizontalAlignment = HorizontalAlignment.Center;
            contactPanel.Children.Add(block);
        }

        private void GetReqs()
        {
            try
            {
                var get = _client.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "http://localhost:36255/api/Home/getReqs")).Result;
                var text = get.Content.ReadAsStringAsync().Result;
                _reqs = JsonConvert.DeserializeObject<BindingList<Req>>(text);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    reqsGrid.ItemsSource = _reqs;
                });
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.Message}");
            }
        }
        private async void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = ((Hyperlink)e.OriginalSource).DataContext as HtmlTemplate;
                check = await AuthControl.Check(_client);

                if (data.Type == "service")
                {
                    var vw = new ServiceView(data, check, _client);
                    vw.ShowDialog();
                }
                else
                {
                    var vw = new HtmlView(data, check, _client);
                    vw.ShowDialog();
                }

                await LoadHtmls();

            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.Message}");
            }
        }
        private async void OnComboBoxSelectionChanged(object sender, RoutedEventArgs e) => await ChangeStatus(sender);

        private async Task ChangeStatus(object sender)
        {
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                DataGridRow row = (DataGridRow)reqsGrid.ContainerFromElement(comboBox);
                int rowIndex = row.GetIndex();
                Req req = (Req)reqsGrid.Items[rowIndex];
                if(req.Status != comboBox.SelectedItem.ToString())
                {
                    req.Status = comboBox.SelectedItem.ToString();
                    string json = JsonConvert.SerializeObject(req);

                    var PostAddFav = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "http://localhost:36255/api/Home/updateReq");
                    PostAddFav.Content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                    var PostAddFavSend = await _client.SendAsync(PostAddFav);
                    var text = await PostAddFavSend.Content.ReadAsStringAsync();

                    if (text.Contains("Заявка обновлена"))
                        MessageBox.Show($"Статус обновлен");
                    else
                    {
                        MessageBox.Show($"Не удаось обновить статус.\n{text}");
                        check = await AuthControl.Check(_client);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка {exc.Message}");
            }
        }
    }
}
