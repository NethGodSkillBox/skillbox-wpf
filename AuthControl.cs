using Newtonsoft.Json;
using SkillboxWPF.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SkillboxWPF
{
    public static class AuthControl
    {
        public static async Task<bool> Auth(HttpClient client, string login, string pass)
        {
            try
            {
                var PostAddFav = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "http://localhost:36255/token");
                PostAddFav.Content = new System.Net.Http.StringContent($"data=username={login},password={pass}", Encoding.UTF8, "application/x-www-form-urlencoded");
                var PostAddFavSend = await client.SendAsync(PostAddFav);
                var text = await PostAddFavSend.Content.ReadAsStringAsync();
                if (!text.Contains("Invalid username or password"))
                {
                    var getAuthInfo = JsonConvert.DeserializeObject<AuthInfo>(text);
                    var token = getAuthInfo.access_token;

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    return true;
                }

                return false;
            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка при авторизации {exc.Message}");
                return false;
            }
        }

        public static async Task<bool> Check(HttpClient client)
        {
            try
            {
                var get = await client.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "http://localhost:36255/api/Home/getlogin"));
                var text = await get.Content.ReadAsStringAsync();

                if (text.Contains("Ваш логин"))
                {
                    return true;
                }
                return false;

            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка при проверке авторизации {exc.Message}");
                return false;
            }
        }
        public static string GetLogin(HttpClient client)
        {
            try
            {
                var get = client.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "http://localhost:36255/api/Home/getlogin")).Result;
                var text = get.Content.ReadAsStringAsync().Result;

                if (text.Contains("Ваш логин"))
                {
                    return text.Split(':')[1];
                }
                return "";

            }
            catch (Exception exc)
            {
                MessageBox.Show($"Ошибка при получении логина {exc.Message}");
                return "";
            }
        }

    }
}
