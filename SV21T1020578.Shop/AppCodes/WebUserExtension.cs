using System.Security.Claims;

namespace SV21T1020578.Shop
{
    public static class WebUserExtension
    {
        ///<sumary>
        /// Đọc thông tin của người dùng được "ghi" trong giấy chứng nhận (Principal)
        ///</sumary>
        ///<param name="principal"></param>
        ///<returns></returns>
        public static WebUserData? GetUserData(this ClaimsPrincipal principal)
        {
            try
            {
                if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                    return null;

                var userData = new WebUserData();

                userData.UserId = principal.FindFirstValue(nameof(userData.UserId)) ?? "";
                userData.UserName = principal.FindFirstValue(nameof(userData.UserName)) ?? "";
                userData.DisplayName = principal.FindFirstValue(nameof(userData.DisplayName)) ?? "";
                userData.Photo = principal.FindFirstValue(nameof(userData.Photo)) ?? "";

                userData.Roles = new List<string>();
                foreach (var role in principal.FindAll(ClaimTypes.Role))
                    userData.Roles.Add(role.Value);

                return userData;
            }
            catch
            {
                return null;
            }
        }
    }
}
