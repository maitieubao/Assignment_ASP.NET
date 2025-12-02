using System.Security.Claims;

namespace Assignment_ASP.NET.Extensions
{
    /// <summary>
    /// Extension methods cho ClaimsPrincipal (User)
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Lấy UserID từ Claims
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác định người dùng.");
        }

        /// <summary>
        /// Lấy Username từ Claims
        /// </summary>
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value 
                ?? throw new UnauthorizedAccessException("Không thể xác định tên người dùng.");
        }

        /// <summary>
        /// Lấy Role từ Claims
        /// </summary>
        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value 
                ?? throw new UnauthorizedAccessException("Không thể xác định vai trò người dùng.");
        }

        /// <summary>
        /// Kiểm tra user có role cụ thể không
        /// </summary>
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role);
        }
    }
}
