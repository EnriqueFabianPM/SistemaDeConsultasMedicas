using System.Text;
using System.Text.Json;
using CSM_BlazorHybridApp.ViewModels;
using CSM_BlazorHybridApp.Models;

namespace CSM_BlazorHybridApp.Services
{
    public class Services
    {
        private readonly Consultories_System_DevContext db = new();
        public Services() { }

        //Método para conseguir las urls de las Apis que se vayan a consumir
        public Api? FetchAPI(ApiConfig config)
        {
            Api? api = new();
            if (config.IdApi != null && config.IdApi > 0)
            {
                api = db.APIs
                    .Where(a => a.Id_API == config.IdApi)
                    .Select(a => new Api
                    {
                        Id_API = a.Id_API,
                        Name = a.Name,
                        URL = a.URL,
                        IsGet = a.IsGet,
                        IsPost = a.IsPost,
                        Param = a.IsGet ? (config.Param ?? "") : "",
                        BodyParams = a.IsPost ? config.BodyParams : null,
                    })
                    .FirstOrDefault();
            }

            return api;
        }

        //Método genérico para consumir APIs Get y Post
        public async Task<T?> ConsumeApi<T>(ApiConfig config)
        {
            Api? api = FetchAPI(config);
            if (api == null) return default;

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(api.IsGet ? HttpMethod.Get : HttpMethod.Post, api.Param != "" ? (api.URL + api.Param) : api.URL);

            if (api.IsPost && api.BodyParams != null)
            {
                var json = JsonSerializer.Serialize(api.BodyParams);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(responseString, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });
        }
    }
}