using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper.Authentication.Validation.Interfaces
{
    public interface IEmailValidation
    {
        bool IsEmailValid(string email);
    }
}
