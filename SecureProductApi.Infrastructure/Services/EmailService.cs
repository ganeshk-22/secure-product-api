using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SecureProductApi.Application.Interfaces;

namespace SecureProductApi.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendVerificationEmailAsync(string toEmail, string verificationLink)
        {
            _logger.LogInformation("VERIFICATION EMAIL for {Email}: {Link}", toEmail, verificationLink);
            return Task.CompletedTask;
        }
    }
}
