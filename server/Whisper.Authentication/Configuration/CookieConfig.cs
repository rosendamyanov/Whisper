using Microsoft.AspNetCore.Http;

namespace Whisper.Authentication.Configuration
{
    public static class CookieConfig
    {
        public static CookieOptions Build(int minutes)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(minutes),
                Path = "/",
            };
        }
    }
}
