// EmployeePublisher.cs
using EmployeeService.Dtos;
using EmployeeService.Models;
using MessageBrokerService;
using System.Text.Json;

namespace EmployeeService.Services
{
    public class EmployeePublisher : BaseMessageBroker
    {
        private readonly ILogger _logger;

        public EmployeePublisher(ILogger<EmployeePublisher> logger) : base(logger)
        {
            _logger = logger;
            DeclareQueue("employee_authen");
        }

        public void PublishEmployeeCreation(Employee employee)
        {
            var employeeDto = new EmployeeCreateDto
            {
                Name = employee.Name,
                Email = employee.Email,
                Address = employee.Address,
                Phone = employee.Phone,
                Gender = employee.Gender,
                Birthdate = employee.Birthdate,
                Password = employee.Password,
                Position = employee.Position,
                SysRole = employee.SysRole,
                CreatedDate = employee.CreatedDate,
                Status = employee.Status
            };

            var queueName = "employee_authen";
            var message = JsonSerializer.Serialize(employeeDto);
            PublishMessage(queueName, message);
            _logger.LogInformation("Message published successfully.");
        }

    }
}
