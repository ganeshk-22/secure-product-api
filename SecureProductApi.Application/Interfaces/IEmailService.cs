using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureProductApi.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationLink);
    }
}