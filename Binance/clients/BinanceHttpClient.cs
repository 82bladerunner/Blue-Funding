using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using BlueFunding.Binance.models;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace BlueFunding.Binance.clients
{
    public class BinanceHttpClient
    {
        
        public HttpClient httpClient = new HttpClient();
        
        HashSet<OtherPosition> positions = new HashSet<OtherPosition>();

        public BinanceHttpClient()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
            (sender, certificate, chain, errors) =>
            {
                return errors == SslPolicyErrors.None;
            };

            static bool MyCertHandler(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors error)
            {
                // Ignore errors
                return true;
            }

            ServicePointManager.ServerCertificateValidationCallback +=
            (mender, certificate, chain, sslPolicyErrors) => true;

        }

        public async Task<OtherPosition> GetPositionsAsync(string userId)
        {
            var requestURL = "https://www.binance.com/bapi/futures/v1/public/future/leaderboard/getOtherPosition";

            var data = new
            {
                encryptedUid = userId,
                tradeType = "PERPETUAL"
            };

            var dataJsonStr = JsonConvert.SerializeObject(data);
            var content = new StringContent(dataJsonStr, Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestURL);
            
            HttpResponseMessage response = await httpClient.PostAsync(requestURL, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var returnObj = JsonConvert.DeserializeObject<OtherPosition>(responseBody);

            return returnObj;
        }

    }
}
