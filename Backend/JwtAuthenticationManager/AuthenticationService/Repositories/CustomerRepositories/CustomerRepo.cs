using AuthenticationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Repositories.CustomerRepositories
{
    public class CustomerRepo : ICustomerRepo
    {
        private readonly AuthenticateContext _context;

        public CustomerRepo(AuthenticateContext context)
        {
            _context = context;
        }
        public async Task CreateCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers
                .AsNoTracking() 
                .FirstOrDefaultAsync(c => c.Id == customer.Id);

            if (existingCustomer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            // Update the existing customer with the new values
            _context.Entry(existingCustomer).CurrentValues.SetValues(customer);

            // Optionally, update any specific properties if needed
            // existingCustomer.Name = customer.Name;
            // existingCustomer.Email = customer.Email;
            // etc.
        }

        public async Task UpdatePassword(Customer customer)
        {
            _context.Customers.Update(customer);
        }
        public async Task<bool> SaveChangeAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
