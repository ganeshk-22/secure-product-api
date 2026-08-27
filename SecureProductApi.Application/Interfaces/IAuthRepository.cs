using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureProductApi.Domain.Entities;

namespace SecureProductApi.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task AddRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task SaveChangesAsync();
    }
}
