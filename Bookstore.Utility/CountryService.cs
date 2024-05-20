using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Bookstore.Utility
{
    public class CountryService
    {
        private readonly HttpClient _httpClient;

        public CountryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetCountries()
        {
            var response = await _httpClient.GetAsync("https://countriesnow.space/api/v0.1/countries");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonString);

            var countries = new List<string>();
            foreach (var country in jsonDocument.RootElement.GetProperty("data").EnumerateArray())
            {
                countries.Add(country.GetProperty("country").GetString());
            }

            return countries;
        }

        public async Task<List<string>> GetStates(string countryName)
        {
            // Tạo body JSON từ tên của quốc gia
            var requestBody = new { country = countryName };
            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Gửi yêu cầu POST đến API
            var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/states", content);
            response.EnsureSuccessStatusCode();

            // Đọc và phân tích cú pháp JSON từ phản hồi
            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonString);

            // Trích xuất danh sách các state từ JSON
            var states = new List<string>();
            var dataNode = jsonDocument.RootElement.GetProperty("data");
            var statesNode = dataNode.GetProperty("states");
            foreach (var state in statesNode.EnumerateArray())
            {
                states.Add(state.GetProperty("name").GetString());
            }

            return states;
        }

        public async Task<List<string>> GetCities(string countryName, string stateName)
        {
            // Tạo body JSON từ tên của quốc gia và tỉnh (state)
            var requestBody = new { country = countryName, state = stateName };
            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Gửi yêu cầu POST đến API
            var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/state/cities", content);
            response.EnsureSuccessStatusCode();

            // Đọc và phân tích cú pháp JSON từ phản hồi
            var jsonString = await response.Content.ReadAsStringAsync();
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                // Trích xuất danh sách các thành phố từ JSON
                var cities = new List<string>();
                var root = document.RootElement;
                var data = root.GetProperty("data");
                foreach (var city in data.EnumerateArray())
                {
                    cities.Add(city.GetString());
                }
                return cities;
            }
        }
    }

}
