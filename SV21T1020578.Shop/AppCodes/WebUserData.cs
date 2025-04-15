using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SV21T1020578.Shop
{
    public class WebUserData
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Photo { get; set; } = "";
        public List<string>? Roles { get; set; }
        public ClaimsPrincipal CreatePrincipal()
        {
            //Tạo danh sách các claim, mỗi claim lưu giữ thông tin danh tính của người dùng
            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameof(UserId), UserId),
                new Claim(nameof(UserName), UserName),
                new Claim(nameof(DisplayName), DisplayName),
                new Claim(nameof(Photo), Photo)
            };
            if (Roles != null)

                foreach (var role in Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

            //Tạo Identity
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //Tạo principal (giấy chứng nhận)
            var principal = new ClaimsPrincipal(identity);
            return principal;
        }
    }
}

