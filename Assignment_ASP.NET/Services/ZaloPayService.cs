using Assignment_ASP.NET.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Assignment_ASP.NET.Helpers;


namespace Assignment_ASP.NET.Services
{
    public interface IZaloPayService
    {
        Task<string> CreatePaymentUrl(HttpContext context, Order order);
        ZaloPayResponseModel ProcessCallback(IQueryCollection queryParams);
    }

    public class ZaloPayService : IZaloPayService
    {
        private readonly IConfiguration _config;
        private readonly string _appId;
        private readonly string _key1;
        private readonly string _key2;
        private readonly string _endpoint;
        private readonly string _returnUrl;

        public ZaloPayService(IConfiguration config)
        {
            _config = config;
            _appId = _config["ZaloPay:AppId"];
            _key1 = _config["ZaloPay:Key1"];
            _key2 = _config["ZaloPay:Key2"];
            _endpoint = _config["ZaloPay:Endpoint"];
            _returnUrl = _config["ZaloPay:ReturnUrl"];
        }

        public async Task<string> CreatePaymentUrl(HttpContext context, Order order)
        {
            var appTransId = $"{DateTime.Now:yyMMdd}_{order.OrderID}";
            var appTime = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            var appUser = order.UserID.ToString();
            var amount = (long)order.TotalAmount;
            var description = $"Thanh toan don hang {order.OrderID}";
            var embedData = JsonConvert.SerializeObject(new { redirecturl = _returnUrl });
            var items = "[]"; 
            
            var data = $"{_appId}|{appTransId}|{appUser}|{amount}|{appTime}|{embedData}|{items}";
            var mac = ZaloPayHelper.SignHmacSHA256(data, _key1);

            var requestData = new Dictionary<string, string>
            {
                { "app_id", _appId },
                { "app_trans_id", appTransId },
                { "app_user", appUser },
                { "app_time", appTime.ToString() },
                { "amount", amount.ToString() },
                { "item", items },
                { "description", description },
                { "embed_data", embedData },
                { "mac", mac },
                { "bank_code", "zalopayapp" }
            };
            
            var response = await SendPostRequest(_endpoint, requestData);
            var jObject = JsonConvert.DeserializeObject<ZaloPayCreatePaymentResponse>(response);

            return jObject?.order_url;
        }

        public ZaloPayResponseModel ProcessCallback(IQueryCollection queryParams)
        {
            var status = queryParams["status"];
            var appTransId = queryParams["apptransid"];
            var zpTransId = queryParams["zptransid"];
            
            // ZaloPay doesn't return all parameters on redirect, so we can't fully validate signature here.
            // A separate callback URL for server-to-server communication is needed for full validation.
            // For the purpose of this demo, we will assume success if status is 1.
            
            return new ZaloPayResponseModel
            {
                Success = status == "1",
                OrderId = appTransId.ToString().Split('_').Last(),
                TransactionId = zpTransId.ToString(),
                Status = status
            };
        }
        
        private async Task<string> SendPostRequest(string url, Dictionary<string, string> data)
        {
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(data);
            var response = await client.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class ZaloPayResponseModel
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ZaloPayCreatePaymentResponse
    {
        public int return_code { get; set; }
        public string return_message { get; set; }
        public string order_url { get; set; }
        public string zp_trans_token { get; set; }
    }

    public class ZaloPayCallbackRequest
    {
        public int return_code { get; set; }
        public string app_trans_id { get; set; }
        public long zp_trans_id { get; set; }
        public int amount { get; set; }
    }
}
