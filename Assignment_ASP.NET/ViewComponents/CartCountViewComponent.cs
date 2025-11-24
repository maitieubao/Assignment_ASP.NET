using Assignment_ASP.NET.Data;
using Assignment_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Assignment_ASP.NET.Helpers;

namespace Assignment_ASP.NET.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("MyCart");
            int count = cart?.Sum(item => item.Quantity) ?? 0;
            return Content(count.ToString());
        }
    }
}
