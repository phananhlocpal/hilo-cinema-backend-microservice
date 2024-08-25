
using MovieService.Models;

namespace MovieService.Dtos.CategoryDtos
{
    public class CategoryCreateDto
    {
        public string? Name { get; set; }
        public string? Status { get; set; }

        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
