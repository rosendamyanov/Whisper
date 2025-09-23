using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper.Authentication.DTOs.Response
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public Guid RefreshTokenId { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
    }
}
