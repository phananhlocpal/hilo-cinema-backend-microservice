using MovieService.Models;

namespace MovieService.Data.ProducerData
{
    public interface IProducerRepo
    {
        Task<IEnumerable<Producer>> GetAllAsync();
        Task<Producer> GetByIdAsync(int id);
        Task InsertAsync(Producer producer);
        Task UpdateAsync(Producer producer);
        Task HiddenProducerAsync(int id);
        Task<int> CountAsync();
    }
}
