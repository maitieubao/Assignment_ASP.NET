using Assignment_ASP.NET.Models;

namespace Assignment_ASP.NET.Services
{
    public interface IZaloPayService
    {
        string CreatePaymentUrl(HttpContext context, Order order);
        ZaloPayResponseModel ProcessCallback(IQueryCollection queryParams);
    }

    public class ZaloPayService : IZaloPayService
    {
        public string CreatePaymentUrl(HttpContext context, Order order)
        {
            // Simulate a successful payment by redirecting to the ZaloPayReturn action with mock parameters
            var returnUrl = $"{context.Request.Scheme}://{context.Request.Host}/Checkout/ZaloPayReturn";
            var mockResponseUrl = $"{returnUrl}?status=1&apptransid={DateTime.Now:yyMMdd}_{order.OrderID}&zptransid=123456789";
            return mockResponseUrl;
        }

        public ZaloPayResponseModel ProcessCallback(IQueryCollection queryParams)
        {
            var status = queryParams["status"].ToString();
            var appTransId = queryParams["apptransid"].ToString();
            var zpTransId = queryParams["zptransid"].ToString();

            // In a real scenario, you would validate the signature here.
            // For mock purposes, we assume the transaction is successful if status is "1".
            bool success = status == "1";

            return new ZaloPayResponseModel
            {
                Success = success,
                OrderId = appTransId.Split('_').Last(),
                TransactionId = zpTransId,
                Status = status
            };
        }
    }

    public class ZaloPayResponseModel
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
    }
}
