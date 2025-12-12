using Assignment_ASP.NET.Models;

namespace Assignment_ASP.NET.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, Order order);
        VnPayResponseModel ProcessCallback(IQueryCollection queryParams);
    }

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContext context, Order order)
        {
            // Redirect to simulator page to show payment interface
            var simulatorUrl = $"{context.Request.Scheme}://{context.Request.Host}/Checkout/VnPaySimulator/{order.OrderID}";
            return simulatorUrl;
        }

        public VnPayResponseModel ProcessCallback(IQueryCollection queryParams)
        {
            var vnpResponseCode = queryParams["vnp_ResponseCode"].ToString();
            var orderId = queryParams["vnp_TxnRef"].ToString();
            var vnpayTranId = queryParams["vnp_TransactionNo"].ToString();
            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();

            // In a real scenario, you would validate the vnpSecureHash here.
            // For mock purposes, we assume the transaction is successful if vnp_ResponseCode is "00".
            bool success = vnpResponseCode == "00";

            return new VnPayResponseModel
            {
                Success = success,
                OrderId = orderId,
                TransactionId = vnpayTranId,
                ResponseCode = vnpResponseCode,
                SecureHash = vnpSecureHash
            };
        }
    }

    public class VnPayResponseModel
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string ResponseCode { get; set; }
        public string SecureHash { get; set; }
    }
}
