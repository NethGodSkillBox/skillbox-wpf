using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SkillboxWPF
{
    public static class Web
    {
        public static async Task<string> Get(HttpClient hc, string url)
        {
            try
            {
                var get = await hc.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Get, $"{url}"));
                var text = await get.Content.ReadAsStringAsync();
                return text;
            }
            catch (Exception m)
            {
                return null;
            }
        }
        public static async Task<string> Post(HttpClient hc, string url, string data)
        {
            try
            {
                var PostAddFav = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, $"{url}");
                PostAddFav.Content = new System.Net.Http.StringContent($"{data}", Encoding.UTF8, "application/x-www-form-urlencoded");
                var PostAddFavSend = await hc.SendAsync(PostAddFav);
                var text = await PostAddFavSend.Content.ReadAsStringAsync();
                return text;
            }
            catch (Exception m)
            {
                return null;
            }
        }

    }
}
