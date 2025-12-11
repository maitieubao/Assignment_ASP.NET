namespace Assignment_ASP.NET.Constants
{
    /// <summary>
    /// Chứa các hằng số về trạng thái đơn hàng
    /// </summary>
    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Shipped = "Shipped";
        public const string Completed = "Completed";
        public const string Canceled = "Canceled";
    }

    /// <summary>
    /// Chứa các hằng số về phương thức thanh toán
    /// </summary>
    public static class PaymentMethod
    {
        public const string COD = "COD";
        public const string VnPay = "VnPay";
        public const string ZaloPay = "ZaloPay";
        public const string MoMo = "MoMo";
    }

    /// <summary>
    /// Chứa các hằng số về trạng thái thanh toán
    /// </summary>
    public static class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }

    /// <summary>
    /// Chứa các hằng số về Session Keys
    /// </summary>
    public static class SessionKeys
    {
        public const string Cart = "MyCart";
        public const string Coupon = "MyCoupon";
    }

    /// <summary>
    /// Chứa các hằng số về Roles
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string Customer = "Customer";
    }

    /// <summary>
    /// Chứa các hằng số về ngân hàng
    /// </summary>
    public static class BankCodes
    {
        public const string Vietcombank = "Vietcombank";
        public const string VietinBank = "VietinBank";
        public const string BIDV = "BIDV";
        public const string Agribank = "Agribank";
        public const string Techcombank = "Techcombank";
        public const string MBBank = "MBBank";

        public static readonly string[] AllBanks = new[]
        {
            Vietcombank,
            VietinBank,
            BIDV,
            Agribank,
            Techcombank,
            MBBank
        };
    }
}
