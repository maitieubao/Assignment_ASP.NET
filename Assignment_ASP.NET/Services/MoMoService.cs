using Assignment_ASP.NET.Models;

namespace Assignment_ASP.NET.Services
{
    public interface IMoMoService
    {
        string CreatePaymentUrl(HttpContext context, Order order);
        MoMoResponseModel ProcessCallback(IQueryCollection queryParams);
    }

    public class MoMoService : IMoMoService
    {
        private readonly IConfiguration _config;

        public MoMoService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContext context, Order order)
        {
            // Simulate a successful payment by redirecting to the MoMoReturn action with mock parameters
            var returnUrl = $"{context.Request.Scheme}://{context.Request.Host}/Checkout/MoMoReturn";
            var mockResponseUrl = $"{returnUrl}?resultCode=0&orderId={order.OrderID}&transId=123456789&message=Success";
            return mockResponseUrl;
        }

        public MoMoResponseModel ProcessCallback(IQueryCollection queryParams)
        {
            var resultCode = queryParams["resultCode"].ToString();
            var orderId = queryParams["orderId"].ToString();
            var transId = queryParams["transId"].ToString();

            // In a real scenario, you would validate the signature here.
            // For mock purposes, we assume the transaction is successful if resultCode is "0".
            bool success = resultCode == "0";

            return new MoMoResponseModel
            {
                Success = success,
                OrderId = orderId,
                TransactionId = transId,
                ResultCode = resultCode
            };
        }
    }

    public class MoMoResponseModel
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string ResultCode { get; set; }
    }
}