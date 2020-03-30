using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using InvestCloudTest.Models;
using Newtonsoft.Json;

namespace InvestCloudTest.Controllers
{
    public class ApiController
    {
        static HttpClient client = new HttpClient();
        private readonly string apiPath = "https://recruitment-test.investcloud.com/";
        //GET api/numbers/init/{size}
        //GET api/numbers/{dataset}/{type}/{idx}
        //POST api/numbers/validate

        public ApiController()
        {
            client.BaseAddress = new Uri(apiPath);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task InitializedMatrices(int size)
        {
            HttpResponseMessage response = await client.GetAsync("api/numbers/init/" + size);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
            throw new Exception("Unable to Initialize Matrix");
        }

        public async Task<ResponseData> GetRowOrColumnDataSet(string identifier, string type, int index)
        {
            var content = await client.GetStringAsync("api/numbers/" + identifier + "/" + type + "/" + index);
            return JsonConvert.DeserializeObject<ResponseData>(content);
        }

        public async Task<string> ValidateMatrix(string matrix)
        {
            var json = JsonConvert.SerializeObject(matrix);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var content = await client.PostAsync(apiPath + "api/numbers/validate", data);
            if (content.StatusCode == HttpStatusCode.OK)
            {
                return content.Content.ReadAsStringAsync().Result;
            }
            throw new Exception("Error, incorrect Passcode");
            
        }
    }
}
