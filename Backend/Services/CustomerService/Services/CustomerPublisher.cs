using CustomerService.Models;
using MessageBrokerService;

namespace CustomerService.Services
{
    public class CustomerPublisher : BaseMessageBroker
    {
        private readonly ILogger _logger;
        public CustomerPublisher(ILogger<CustomerPublisher> logger) : base(logger)
        {
            _logger = logger;
            DeclareQueue("customer_authen");
        }

        public void CreateCustomerPubSub(Customer customer)
        {
            var queueName = "customer_authen";
            var message = customer;
            PublishMessage(queueName, message);
            _logger.LogInformation("Message published successfully.");
        }
    }
}
