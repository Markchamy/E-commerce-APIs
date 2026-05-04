using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Newtonsoft.Json;
using System.Text;

namespace Backend.Interfaces
{
    public class FleetRunnrService
    {
        private readonly HttpClient _httpClient;
        private readonly FleetRunnrOptions _options;
        private readonly string? _connectionString;

        public FleetRunnrService(HttpClient httpClient, IOptions<FleetRunnrOptions> options, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> GetCarriersAsync()
        {
            var url = $"{_options.BaseUrl}/carriers";
            Console.WriteLine($"Request URL: {url}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            return jsonString;
        }


        public async Task<string> GetCustomerAsync(string? phone = null, string? email = null, bool locations = false)
        {
            var queryParameters = new List<string>();

            if (!string.IsNullOrEmpty(phone))
            {
                queryParameters.Add($"phone={Uri.EscapeDataString(phone)}");
            }

            if (!string.IsNullOrEmpty(email))
            {
                queryParameters.Add($"email={Uri.EscapeDataString(email)}");
            }

            if (locations)
            {
                queryParameters.Add("locations=true");
            }

            var queryString = string.Join("&", queryParameters);
            var url = $"{_options.BaseUrl}/customers?{queryString}";
            Console.WriteLine($"Request URL: {url}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response: {errorContent}");
                throw new HttpRequestException($"Error Response: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetLocationsAsync(string? externalId = null, string? uuid = null)
        {
            var queryParameters = new List<string>();

            if (!string.IsNullOrEmpty(externalId))
            {
                queryParameters.Add($"external_id={Uri.EscapeDataString(externalId)}");
            }

            if (!string.IsNullOrEmpty(uuid))
            {
                queryParameters.Add($"uuid={Uri.EscapeDataString(uuid)}");
            }

            var queryString = string.Join("&", queryParameters);
            var url = $"{_options.BaseUrl}/locations?{queryString}";
            Console.WriteLine($"Request URL: {url}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetOrdersAsync(string ordernumber)
        {
            var queryParameters = new List<string>();

            if (!string.IsNullOrEmpty(ordernumber))
            {
                queryParameters.Add($"order_number={Uri.EscapeDataString(ordernumber)}");
            }

            var queryString = string.Join("$", queryParameters);
            var url = $"{_options.BaseUrl}/orders?{queryString}";
            Console.WriteLine($"{queryString} {url}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CreateCustomerAsync(FleetCustomer customer)
        {
            var url = $"{_options.BaseUrl}/customers";
            var json = JsonConvert.SerializeObject(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CreateLocationAsync(FleetLocation location)
        {
            var url = $"{_options.BaseUrl}/locations";
            var json = JsonConvert.SerializeObject(location);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CreateOrderAsync(CreateOrderRequest createOrderRequest)
        {
            var url = $"{_options.BaseUrl}/orders";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(createOrderRequest), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"FleetRunnr error {(int)response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateCustomerAsync(Guid customer_uuid, FleetCustomer customer)
        {
            var url = $"{_options.BaseUrl}/customers/{customer_uuid}";
            var json = JsonConvert.SerializeObject(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Content: {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateOrderAsync(Guid order_uuid, FleetOrderUpdate update)
        {
            var url = $"{_options.BaseUrl}/orders/{order_uuid}";
            var json = JsonConvert.SerializeObject(update);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Content: {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> AssignCarrierAsync(Guid order_uuid, AssignCarrierRequest request)
        {
            var url = $"{_options.BaseUrl}/orders/{order_uuid}/assign";
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CancelOrderAsync(Guid order_uuid)
        {
            var url = $"{_options.BaseUrl}/orders/{order_uuid}/cancel";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GenerateLabelsAsync(GenerateLabelsRequest request)
        {
            var url = $"{_options.BaseUrl}/orders/generate-labels";
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> WebhookSubscribeAsync(WebhookSubscribe subscribe)
        {
            var url = $"{_options.BaseUrl}/webhooks/subscribe";
            var json = JsonConvert.SerializeObject(subscribe);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> WebhookUnsubscribeAsync(WebhookUnsubscribe unsubscribe)
        {
            var url = $"{_options.BaseUrl}/webhooks/unsubscribe";
            var json = JsonConvert.SerializeObject(unsubscribe);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PayInvoiceAsync(Guid invoice_number)
        {
            var url = $"{_options.BaseUrl}/invoices/{invoice_number}/pay";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        
        public async Task<string> VoidInvoiceAsync(Guid invoice_number)
        {
            var url = $"{_options.BaseUrl}/invoices/{invoice_number}/void";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}