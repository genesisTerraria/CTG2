using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CTG2.Content.Commands.Auth
{
    public static class AuthAPI
    {
        private static readonly HttpClient client = new();
        private const string BaseUrl = "https://ctg2-auth.railway.app"; // your URL here

        public class AuthResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
        }

        public static async Task<AuthResult> Register(string username, string password)
        {
            try
            {
                var body = JsonConvert.SerializeObject(new { username, password });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BaseUrl}/register", content);
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AuthResult>(json) ?? new AuthResult { Success = false, Message = "No response." };
            }
            catch (Exception e)
            {
                return new AuthResult { Success = false, Message = $"Connection failed: {e.Message}" };
            }
        }

        public static async Task<AuthResult> Login(string username, string password)
        {
            try
            {
                var body = JsonConvert.SerializeObject(new { username, password });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BaseUrl}/login", content);
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AuthResult>(json) ?? new AuthResult { Success = false, Message = "No response." };
            }
            catch (Exception e)
            {
                return new AuthResult { Success = false, Message = $"Connection failed: {e.Message}" };
            }
        }
    }
}