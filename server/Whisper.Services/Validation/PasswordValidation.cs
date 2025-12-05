using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Whisper.Services.Validation.Interfaces;

namespace Whisper.Services.Validation
{
    public class PasswordValidation : IPasswordValidation
    {
        public bool IsStrong(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,64}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}
