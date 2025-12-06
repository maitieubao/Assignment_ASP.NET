namespace Assignment_ASP.NET.Models // <-- Đổi namespace thành Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Thuộc tính tính toán tổng tiền
        public decimal Total
        {
            get { return Price * Quantity; }
        }

        public CartItem() { }

        public CartItem(Product product) // Dùng Product từ namespace Models
        {
            ProductID = product.ProductID;
            ProductName = product.ProductName;
            ImageUrl = product.ImageUrl;
            Price = product.Price;
            Quantity = 1; // Mặc định là 1
        }
    }
}