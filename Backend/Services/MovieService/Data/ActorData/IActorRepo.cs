using MovieService.Models;

namespace MovieService.Data.ActorData
{
    public interface IActorRepo
    {
        Task<IEnumerable<Actor>> GetAllAsync();
        Task<Actor> GetByIdAsync(int id);
        Task InsertAsync(Actor actor);
        Task UpdateAsync(Actor actor);
        Task HideActor(int id);
        Task<int> CountAsync();
    }
}
