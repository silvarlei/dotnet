
using Newtonsoft.Json;
using OpenQA.Selenium.DevTools;
using RoboAutomacaoConsole.Constants;
using RoboAutomacaoConsole.Model;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Serilog;
using Log = Serilog.Log;

namespace RoboAutomacaoConsole
{
    public class API
    {
       
        public async Task<string> Authorize()
        {

             HttpClient _httpClient = new HttpClient();

            using var req = new HttpRequestMessage(HttpMethod.Get, $"https://api-labs-hom.localiza.com/uniid-uniid-netcore/auth/authorize?response_type=code&client_id=localiza_appreservas_code&state=bUpjWmRlT1FlSmxSTndBMlB1UUZzZ0x3VVJ0WWFRd3hNZFhiUmtOYnBQYw&scope=profile&redirect_uri=http%3A%2F%2Flocalhost%3A4200%2Flogin&code_challenge=tdQEXD9Gb6kf4sxqvnkjKhpXzfEE96JucW4KHieJ33g&code_challenge_method=S256")
            {
                // Content = new FormUrlEncodedContent(request)
            };
            _httpClient.DefaultRequestHeaders.Add("device_id", $"Device123");
            _httpClient.DefaultRequestHeaders.Add("device_name", $"DeviceTeste");
            _httpClient.DefaultRequestHeaders.Add("device_ip", $"123.0.0.4");
            _httpClient.DefaultRequestHeaders.Add("Cookie", $"_abck=6B2346CB677237C9AD1E297398FD6B56~-1~YAAQxNPPF9A5hgSIAQAAQdOiCgmb+Sif4hyUQeZxJEU4avzGZwmYgKiWd0PqAiekQh81RDclre2Zka8G/JsI7abcpWjuyfbaQEOAZHj9N9hbh4sZkTKZdjOZZV+l6dAnUui8P5UUvO65Hc3WuThYsXqwNEH2i0c/7HiuViXpQ0psakGXQMzJIOG9ZxvkWV7OFoWtk44MhyNgu5x07WTQYBUhjup11YMfNM3VLLWDQHBvFrBj6qztvSDIgfpR3sw6vTwlRyb5cYlW1biFQoXXuDuIV5IqKXuB+7ghZbdeAy4ukyGcoJircZhXP9OXyVeIDeodu3/Pzl5zRFWN76URNFvIXbJXyn4E8OH84ZMXmWEzKHBeyqjxhcrO8cQCv3xEDeGRQHtzmN5d~-1~-1~-1");
            var response = _httpClient.Send(req);

            var ret = await response.Content.ReadAsStringAsync();

            return response.RequestMessage.RequestUri.ToString();
            //if (!response.IsSuccessStatusCode)
            //    var tt = "passou";
            //else
            //{
            //    var obj = JsonConvert.DeserializeObject<TokenResponse>(ret);
            //    return obj;
            //}


            // return null;
        }

        public Dictionary<string, string> ParseQuery(string query)
        {
            var dic = new Dictionary<string, string>();
            var reg = new Regex("(?:[?&]|^)([^&]+)=([^&]*)");
            var matches = reg.Matches(query);
            foreach (Match match in matches)
            {
                dic[match.Groups[1].Value] = Uri.UnescapeDataString(match.Groups[2].Value);
            }
            return dic;
        }


        public async Task<TokenResponse> GetTokens(string refreshToken)
        {
            try
            {
                var clientId = Configuration.ClientId;
                var clientSecret = Configuration.ClientSecret;

                var Request = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                };

                var tokens = await SendToken(Request);

                return tokens;
            }
            catch (Exception ex)
            {
                throw new Exception("FailedRetrieveToken", ex);
            }
        }

        public async Task<TokenResponse> GetTokens(string refreshToken, string code)
        {
            try
            {
                var clientId = Configuration.ClientId;
                var clientSecret = Configuration.ClientSecret;

                var Request = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    //{ "refresh_token", refreshToken },
                    { "client_id", clientId },
                    { "code", code },
                    { "code_verifier", "ABC" },
                    { "redirect_uri", "http://localhost:4200/login" },
                    { "client_secret", clientSecret }
                };

                var tokens = await SendToken(Request);

                return tokens;
            }
            catch (Exception ex)
            {
                throw new Exception("FailedRetrieveToken", ex);
            }
        }

        public async Task<TokenResponse> SendToken(Dictionary<string, string> request)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, $"{Configuration.UrlApi}{Configuration.EndpointToken}")
                {
                    Content = new FormUrlEncodedContent(request)
                };
                _httpClient.DefaultRequestHeaders.Add("device_id", $"Device123");
                _httpClient.DefaultRequestHeaders.Add("device_name", $"DeviceTeste");
                _httpClient.DefaultRequestHeaders.Add("device_ip", $"123.0.0.4");
                var response = _httpClient.Send(req);

                var ret = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    Log.Warning($"[SendToken] - {ret} - {Configuration.UrlApi}{Configuration.EndpointToken}");
                else
                {
                    var obj = JsonConvert.DeserializeObject<TokenResponse>(ret);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SendToken] {ex.Message}");
            }

            return null;
        }

        public async Task<string> SendUserInfo(string accessToken)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Get, $"{Configuration.UrlApi}{Configuration.EndpointUserInfo}");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                _httpClient.DefaultRequestHeaders.Add("device_id", $"Device123");
                _httpClient.DefaultRequestHeaders.Add("device_name", $"DName{DateTime.Now}");
                _httpClient.DefaultRequestHeaders.Add("device_ip", $"{DateTime.Now.Year}.{DateTime.Now.Hour}.{DateTime.Now.Minute}{DateTime.Now.Second}");

                var responseUserInfo = _httpClient.Send(req);

                var ret = await responseUserInfo.Content.ReadAsStringAsync();
                Log.Information($"{Configuration.UrlApi} - {ret}");

                if (!responseUserInfo.IsSuccessStatusCode)
                    Log.Warning($"[SendUserInfo] - {ret} - {Configuration.UrlApi}{Configuration.EndpointUserInfo}");
                else
                {
                    //var obj = JsonConvert.DeserializeObject<TokenResponse>(ret);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SendUserInfo] {ex.Message}");
            }

            return string.Empty;
        }

    }
}
