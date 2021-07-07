using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace travel_app
{
    public class TravelAPI
    {
        private string apiKey;
        private string apiSecret;
        private string bearerToken;
        private HttpClient http;

        public TravelAPI(IConfiguration config, IHttpClientFactory httpFactory)
        {
            apiKey = config.GetValue<string>("AmadeusAPI:APIKey");
            apiSecret = config.GetValue<string>("AmadeusAPI:APISecret");
            http = httpFactory.CreateClient("TravelAPI");
        }

        public async Task ConnectOAuth()
        {
            var message = new HttpRequestMessage(HttpMethod.Post, "/v1/security/oauth2/token");
            message.Content = new StringContent(
                $"grant_type=client_credentials&client_id={apiKey}&client_secret={apiSecret}",
                Encoding.UTF8, "application/x-www-form-urlencoded"
            );

            var results = await http.SendAsync(message);
            await using var stream = await results.Content.ReadAsStreamAsync();
            var oauthResults = await JsonSerializer.DeserializeAsync<OAuthResults>(stream);

            bearerToken = oauthResults.access_token;
        }

        private class OAuthResults
        {
            public string access_token { get; set; }
        }

        public async Task<BusiestPeriodResults> GetBusiestTravelPeriodsOfYear(string cityCode, int year)
        {
            var message = new HttpRequestMessage(HttpMethod.Get,
                $"/v1/travel/analytics/air-traffic/busiest-period?cityCode={cityCode}&period={year}");

            ConfigBearerTokenHeader();
            var response = await http.SendAsync(message);
            using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<BusiestPeriodResults>(stream);
        }

        private void ConfigBearerTokenHeader()
        {
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
        }

        public class BusiestPeriodResults
        {
            public class Item
            {
                public string type { get; set; }
                public string period { get; set; }
                public Analytics analytics { get; set; }
                public int score => analytics.travelers.score;
            }

            public class Analytics
            {
                public Travelers travelers { get; set; }
            }

            public class Travelers
            {
                public int score { get; set; }
            }

            public List<Item> data { get; set; }
            public string graphArray => string.Join(',', data.OrderBy(x => x.period).Select(x => x.score));
        }
    }
}
