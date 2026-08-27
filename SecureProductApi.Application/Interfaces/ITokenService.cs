using SecureProductApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureProductApi.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken(string userId);
    }
}