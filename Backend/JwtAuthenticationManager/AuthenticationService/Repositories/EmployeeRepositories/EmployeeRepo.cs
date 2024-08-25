using AuthenticationService.Models;

namespace AuthenticationService.Repositories.EmployeeRepositories
{
    public class EmployeeRepo : IEmployeeRepo
    {
        private readonly AuthenticateContext _context;

        public EmployeeRepo(AuthenticateContext context)
        {
            _context = context;
        }
        public async Task CreateEmployeeAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public async Task UpdatePassword(Employee employee)
        {
            _context.Employees.Update(employee);
        }
        public async Task<bool> SaveChangeAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
