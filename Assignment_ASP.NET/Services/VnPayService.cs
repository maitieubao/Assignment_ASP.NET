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
            // Simulate a successful payment by redirecting to the VnPayReturn action with mock parameters
            var returnUrl = $"{context.Request.Scheme}://{context.Request.Host}/Checkout/VnPayReturn";
            var mockResponseUrl = $"{returnUrl}?vnp_ResponseCode=00&vnp_TxnRef={order.OrderID}&vnp_TransactionNo=123456789&vnp_SecureHash=mock_hash";
            return mockResponseUrl;
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
