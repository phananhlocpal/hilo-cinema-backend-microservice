using MovieService.Models;

namespace MovieService.Data.MovieData
{
    public interface IMovieRepo
    {
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<Movie> GetByIdAsync(int id);
        Task<Movie> GetByUrlAsync(string url);
        Task InsertAsync(Movie movie);
        Task UpdateAsync(Movie movie);
        Task DisableMovieAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
