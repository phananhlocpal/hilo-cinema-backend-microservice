using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaleService.Dtos;
using SaleService.Models;
using SaleService.OtherModels;
using SaleService.Repositories.InvoiceFoodRepository;
using SaleService.Repositories.InvoiceRepository;
using SaleService.Service.RabbitMQServices;
using SaleService.Services.HttpServices;
using SaleService.ViewModels;
using System.Text.Json;


namespace SaleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepo _repository;
        private readonly IInvoiceFoodRepo _invoiceFoodRepo;
        private readonly IMapper _mapper;
        private readonly ScheduleHttpService _scheduleHttpService;
        private readonly CustomerHttpService _customerHttpService;
        private readonly EmployeeHttpService _employeeHttpService;
        private readonly SalePublisherService _salePublisherService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
             IInvoiceRepo repository,
              IInvoiceFoodRepo invoiceFoodRepo,
        IMapper mapper,
             ScheduleHttpService scheduleHttpService,
             SalePublisherService salePublisherService,
             ILogger<InvoiceController> logger)
        {
            _repository = repository;
            _invoiceFoodRepo = invoiceFoodRepo;
            _mapper = mapper;
            _scheduleHttpService = scheduleHttpService;
            _salePublisherService = salePublisherService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<InvoiceReadDto>>> GetAllInvoices()
        {
            var invoices = await _repository.GetAllInvoiceAsync();

            var invoiceReadDtos = new List<InvoiceReadDto>();

            foreach (var invoice in invoices)
            {
                // Start all tasks concurrently
                var scheduleTask = _scheduleHttpService.GetScheduleByInvoiceId(invoice.Id);
                var customerTask = _customerHttpService.GetCustomerById(invoice.CustomerId);
                var employeeTask = _employeeHttpService.GetEmployeeById(invoice.EmployeeId);

                // Wait for all tasks to complete
                await Task.WhenAll(scheduleTask, customerTask, employeeTask);

                var schedules = await scheduleTask;
                var customer = await customerTask;
                var employee = await employeeTask;

                // Check if customer and employee are not null before proceeding
                if (customer == null || employee == null)
                {
                    _logger.LogWarning($"Customer or Employee not found for InvoiceId: {invoice.Id}");
                    continue;  // Skip this invoice if related entities are not found
                }

                var invoiceReadDto = new InvoiceReadDto
                {
                    Id = invoice.Id,
                    EmployeeId = invoice.EmployeeId,
                    CustomerId = invoice.CustomerId,
                    PromotionId = invoice.PromotionId,
                    CreatedDate = invoice.CreatedDate,
                    PaymentMethod = invoice.PaymentMethod,
                    Total = invoice.Total,
                    Status = invoice.Status,
                    Schedules = schedules,
                    Customer = customer,
                    Employee = employee,
                };

                invoiceReadDtos.Add(invoiceReadDto);
            }

            return Ok(invoiceReadDtos);
        }

        [HttpGet("GetInvoiceById/{invoiceId}")]
        [AllowAnonymous]
        public async Task<ActionResult<InvoiceReadDto>> GetInvoiceById(int invoiceId)
        {
            var invoice = await _repository.GetInvoiceByIdAsync(invoiceId);

            if (invoice == null)
            {
                _logger.LogWarning($"Invoice with ID {invoiceId} not found.");
                return NotFound($"Invoice with ID {invoiceId} not found.");
            }

            // Start all tasks concurrently
            var scheduleTask = _scheduleHttpService.GetScheduleByInvoiceId(invoice.Id);
            var customerTask = _customerHttpService.GetCustomerById(invoice.CustomerId);
            var employeeTask = _employeeHttpService.GetEmployeeById(invoice.EmployeeId);

            // Wait for all tasks to complete
            await Task.WhenAll(scheduleTask, customerTask, employeeTask);

            var schedules = await scheduleTask;
            var customer = await customerTask;
            var employee = await employeeTask;

            // Check if customer and employee are not null before proceeding
            if (customer == null)
            {
                _logger.LogWarning($"Customer with ID {invoice.CustomerId} not found for InvoiceId: {invoice.Id}");
            }

            if (employee == null)
            {
                _logger.LogWarning($"Employee with ID {invoice.EmployeeId} not found for InvoiceId: {invoice.Id}");
            }

            var invoiceReadDto = new InvoiceReadDto
            {
                Id = invoice.Id,
                EmployeeId = invoice.EmployeeId,
                CustomerId = invoice.CustomerId,
                PromotionId = invoice.PromotionId,
                CreatedDate = invoice.CreatedDate,
                PaymentMethod = invoice.PaymentMethod,
                Total = invoice.Total,
                Status = invoice.Status,
                Schedules = schedules,
                Customer = customer,
                Employee = employee,
            };

            return Ok(invoiceReadDto);
        }

        [HttpGet("GetInvoicesByCustomerId/{customerId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<InvoiceReadDto>>> GetInvoicesByCustomerId(int customerId)
        {
            // Fetch invoices for the given customerId
            var invoices = await _repository.GetInvoicesByCustomerId(customerId);

            if (invoices == null || !invoices.Any())
            {
                _logger.LogWarning($"No invoices found for CustomerId: {customerId}");
                return NotFound($"No invoices found for CustomerId: {customerId}");
            }

            var invoiceReadDtos = new List<InvoiceReadDto>();

            foreach (var invoice in invoices)
            {
                // Fetch schedules, customer, and employee concurrently
                var scheduleTask = _scheduleHttpService.GetScheduleByInvoiceId(invoice.Id);
                var customerTask = _customerHttpService.GetCustomerById(invoice.CustomerId);
                var employeeTask = _employeeHttpService.GetEmployeeById(invoice.EmployeeId);

                // Await all tasks to complete
                await Task.WhenAll(scheduleTask, customerTask, employeeTask);

                var schedules = await scheduleTask;
                var customer = await customerTask;
                var employee = await employeeTask;

                // Logging if customer or employee information is not found
                if (customer == null)
                {
                    _logger.LogWarning($"Customer with ID {invoice.CustomerId} not found for InvoiceId: {invoice.Id}");
                }

                if (employee == null)
                {
                    _logger.LogWarning($"Employee with ID {invoice.EmployeeId} not found for InvoiceId: {invoice.Id}");
                }

                // Create InvoiceReadDto with the fetched information
                var invoiceReadDto = new InvoiceReadDto
                {
                    Id = invoice.Id,
                    EmployeeId = invoice.EmployeeId,
                    CustomerId = invoice.CustomerId,
                    PromotionId = invoice.PromotionId,
                    CreatedDate = invoice.CreatedDate,
                    PaymentMethod = invoice.PaymentMethod,
                    Total = invoice.Total,
                    Status = invoice.Status,
                    Schedules = schedules,
                    Customer = customer,
                    Employee = employee,
                };

                invoiceReadDtos.Add(invoiceReadDto);
            }

            return Ok(invoiceReadDtos);
        }

        [HttpPost]
        [Authorize]
        public async Task<InvoiceRequestModel> CreateInvoice([FromBody] VnPaymentRequestModel paymentRequest)
        {
            if (paymentRequest == null)
            {
                return null;
            }

            var invoice = new InvoiceRequestModel
            {
                CreatedDate = paymentRequest.Invoice.CreatedDate,
                CustomerId = paymentRequest.Invoice.CustomerId,
                EmployeeId = paymentRequest.Invoice.EmployeeId,
                PromotionId = paymentRequest.Invoice.PromotionId,
                PaymentMethod = paymentRequest.Invoice.PaymentMethod,
                Total = paymentRequest.Invoice.Total,
                SeatIds = paymentRequest.Invoice.SeatIds,
                FoodRequests = paymentRequest.Invoice.FoodRequests ?? new List<FoodRequestModel>(),
                Schedule = paymentRequest.Invoice.Schedule
            };

            var invoiceCreate = new Invoice
            {
                CreatedDate = invoice.CreatedDate,
                CustomerId = invoice.CustomerId,
                EmployeeId = invoice.EmployeeId,
                PromotionId = invoice.PromotionId,
                PaymentMethod = invoice.PaymentMethod,
                Total = invoice.Total,
                Status = "Completed",
            };

            var createdInvoice = await _repository.CreateInvoiceAsync(invoiceCreate);
            _logger.LogInformation("Invoice created successfully. Invoice ID: {InvoiceId}", createdInvoice.Id);

            if (invoice.FoodRequests != null && invoice.FoodRequests.Any())
            {
                foreach (var foodRequest in invoice.FoodRequests)
                {
                    var invoiceFood = new InvoiceFood
                    {
                        InvoiceId = createdInvoice.Id,
                        FoodId = foodRequest.FoodId,
                        Quantity = foodRequest.Quantity
                    };
                    await _invoiceFoodRepo.CreateInvoiceFoodAsync(invoiceFood);
                    _logger.LogInformation("Invoice food created successfully. Food ID: {FoodId}", invoiceFood.FoodId);
                }
            }

            var schedules = invoice.SeatIds?.Select(seatId => new OriginalSchedule
            {
                MovieId = invoice.Schedule.MovieId,
                Date = invoice.Schedule.Date,
                Time = invoice.Schedule.Time,
                SeatId = seatId,
                InvoiceId = createdInvoice.Id,
            }).ToList();
            if (schedules != null)
            {
                _salePublisherService.UpdateInvoiceIdInSchedule(schedules);
                _logger.LogInformation("Update invoice successfully!");
            }

            return invoice;
        }

        [HttpDelete]
        [Authorize(Policy = "AdminOnly")]
        public async Task<bool> DeleteInvoice(int movieId, DateOnly date, TimeOnly time, int seatId, int invoiceId)
        {
            if (invoiceId == null)
            {
                return false;
            }

            try
            {
                // Delete InvoiceFood
                await _invoiceFoodRepo.DeleteInvoiceFoodByInvoiceIdAsync(invoiceId);
                //Delete Invoice
                await _repository.DeleteInvoiceAsync(invoiceId);
                // Update schedules with invoiceId = null
                var invoice = await _repository.GetInvoiceByIdAsync(invoiceId);
                var schedule = new OriginalSchedule
                {
                    MovieId = movieId,
                    Date = date,
                    Time = time,
                    SeatId = seatId,
                    InvoiceId = null,
                };

                if (schedule != null)
                {
                    _salePublisherService.UpdateEachInvoiceIdInSchedule(schedule);
                    _logger.LogInformation("Update invoice successfully!");
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Error: {Error}", e.Message);
                return false;
            }
        }
    }
}
