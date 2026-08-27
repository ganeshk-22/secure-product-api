using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureProductApi.Application.Interfaces;
using SecureProductApi.Domain.Entities;
using SecureProductApi.Infrastructure.Persistence;

namespace SecureProductApi.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRefreshTokenAsync(RefreshToken token) =>
            await _context.RefreshTokens.AddAsync(token);

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token) =>
            await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
