using Assignment_ASP.NET.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Assignment_ASP.NET.Helpers;

namespace Assignment_ASP.NET.Services
{
    public interface IMoMoService
    {
        Task<string> CreatePaymentUrl(HttpContext context, Order order);
        MoMoResponseModel ProcessCallback(IQueryCollection queryParams);
    }

    public class MoMoService : IMoMoService
    {
        private readonly IConfiguration _config;

        public MoMoService(IConfiguration config)
        {
            _config = config;
        }

       public async Task<string> CreatePaymentUrl(HttpContext context, Order order)
        {
            var partnerCode = _config["MoMo:PartnerCode"];
            var accessKey = _config["MoMo:AccessKey"];
            var secretKey = _config["MoMo:SecretKey"];
            var returnUrl = _config["MoMo:ReturnUrl"];
            var notifyUrl = _config["MoMo:NotifyUrl"];
            var requestType = "captureWallet";
            var orderInfo = $"Thanh toan don hang {order.OrderID}";
            var amount = order.TotalAmount.ToString();
            var orderId = order.OrderID.ToString();
            var requestId = Guid.NewGuid().ToString();
            var extraData = "";

            var rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData={extraData}";

            var signature = MoMoHelper.SignHmacSHA256(rawHash, secretKey);

            var payload = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount,
                orderId,
                orderInfo,
                returnUrl,
                notifyUrl,
                extraData,
                requestType,
                signature
            };

            var requestBody = JsonConvert.SerializeObject(payload);

            return await CreateMoMoPayment(requestBody);
        }

        private async Task<string> CreateMoMoPayment(string requestBody)
        {
            var endpoint = _config["MoMo:Endpoint"];
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var jmessage = JsonConvert.DeserializeObject<MoMoCreatePaymentResponse>(responseContent);

            return jmessage.PayUrl;
        }

        public MoMoResponseModel ProcessCallback(IQueryCollection queryParams)
        {
            var accessKey = _config["MoMo:AccessKey"];
            var secretKey = _config["MoMo:SecretKey"];

            var partnerCode = queryParams["partnerCode"].ToString();
            var orderId = queryParams["orderId"].ToString();
            var requestId = queryParams["requestId"].ToString();
            var amount = queryParams["amount"].ToString();
            var orderInfo = queryParams["orderInfo"].ToString();
            var orderType = queryParams["orderType"].ToString();
            var transId = queryParams["transId"].ToString();
            var resultCode = queryParams["resultCode"].ToString();
            var message = queryParams["message"].ToString();
            var payType = queryParams["payType"].ToString();
            var responseTime = queryParams["responseTime"].ToString();
            var extraData = queryParams["extraData"].ToString();
            var signature = queryParams["signature"].ToString();
            
            var rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&orderType={orderType}&transId={transId}&resultCode={resultCode}&message={message}&payType={payType}&responseTime={responseTime}&extraData={extraData}";
            
            var expectedSignature = MoMoHelper.SignHmacSHA256(rawHash, secretKey);

            return new MoMoResponseModel
            {
                Success = expectedSignature == signature && resultCode == "0",
                OrderId = orderId,
                TransactionId = transId,
                ResultCode = resultCode
            };
        }
    }

    public class MoMoResponseModel
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
    }
     public class MoMoCreatePaymentResponse
    {
        public string PartnerCode { get; set; }
        public string RequestId { get; set; }
        public string OrderId { get; set; }
        public long Amount { get; set; }
        public long ResponseTime { get; set; }
        public string Message { get; set; }
        public int ResultCode { get; set; }
        public string PayUrl { get; set; }
    }
}
