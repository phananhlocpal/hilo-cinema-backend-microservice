using AuthenticationService.Models;

namespace AuthenticationService.Repositories.EmployeeRepositories
{
    public interface IEmployeeRepo
    {
<<<<<<< HEAD
        Task CreateEmployeeAsync(Employee employee);
        Task UpdatePassword(Employee employee);
        Task<bool> SaveChangeAsync();
=======
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee> GetByIdAsync(int id);
        Task CreateAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task<bool> SaveChangesAsync();
>>>>>>> 960a83c (commit)
    }
}
