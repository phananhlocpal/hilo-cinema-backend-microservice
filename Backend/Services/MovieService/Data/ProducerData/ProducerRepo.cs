using Microsoft.EntityFrameworkCore;
using MovieService.Models;
using System;

namespace MovieService.Data.ProducerData
{
    public class ProducerRepo : IProducerRepo
    {
        private readonly MovieContext _context;

        public ProducerRepo(MovieContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producer>> GetAllAsync()
        {
            return await _context.Producers.ToListAsync();
        }

        public async Task<Producer> GetByIdAsync(int id)
        {
            return await _context.Producers.FindAsync(id);
        }

        public async Task InsertAsync(Producer producer)
        {
            await _context.Producers.AddAsync(producer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Producer producer)
        {
            _context.Producers.Update(producer);
            await _context.SaveChangesAsync();
        }

        public async Task HiddenProducerAsync(int id)
        {
            var producer = await GetByIdAsync(id);
            if (producer != null)
            {
                // Giả sử Producer có thuộc tính Status để xác định xem nó có ẩn hay không
                producer.Status = "Inactive";  // hoặc cách xử lý ẩn tương tự
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CountAsync()
        {
            return await _context.Producers.CountAsync();
        }
    }
}
