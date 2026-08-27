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
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync() =>
            await _context.Products.AsNoTracking().ToListAsync();

        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.FindAsync(id);

        public async Task<Product> AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            _context.Products.Remove(product);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}