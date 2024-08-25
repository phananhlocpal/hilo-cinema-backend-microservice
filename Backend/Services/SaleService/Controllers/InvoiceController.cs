using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SaleService.Dtos;
using SaleService.Models;
using SaleService.OtherModels;
using SaleService.Repositories.InvoiceRepository;
using SaleService.Service.HttpServices;
using SaleService.Service.RabbitMQServices;


namespace SaleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepo _repository;
        private readonly IMapper _mapper;
        private readonly ScheduleHttpService _scheduleHttpService;
        private readonly SalePublisherService _salePublisherService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
             IInvoiceRepo repository,
             IMapper mapper,
             ScheduleHttpService scheduleHttpService,
             SalePublisherService salePublisherService,
             ILogger<InvoiceController> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _scheduleHttpService = scheduleHttpService;
            _salePublisherService = salePublisherService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceReadDto>>> GetAllInvoices()
        {
            var invoices = await _repository.GetAllInvoiceAsync();

            var invoiceReadDtos = new List<InvoiceReadDto>();

            foreach (var invoice in invoices)
            {
                IEnumerable<Schedule> schedule = await _scheduleHttpService.GetScheduleByInvoiceId(invoice.Id);

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
                    Schedules = schedule,
                    Customer = null,
                    Employee = null,
                };
                
                invoiceReadDtos.Add(invoiceReadDto);
            };
            return Ok(invoiceReadDtos);

        }

        [HttpGet("GetInvoiceById/{invoiceId}")]
        public async Task<ActionResult<InvoiceReadDto>> GetInvoiceById(int invoiceId)
        {
            var invoice = await _repository.GetInvoiceByIdAsync(invoiceId);

            IEnumerable<Schedule> schedule = await _scheduleHttpService.GetScheduleByInvoiceId(invoice.Id);

            var invoiceReadDto = new InvoiceReadDto
            {
                Id = invoice.Id,
                EmployeeId = invoice.EmployeeId,
                CustomerId = invoice.CustomerId,
                PromotionId = invoice.PromotionId,
                PaymentMethod = invoice.PaymentMethod,
                Total = invoice.Total,
                Status = invoice.Status,
                Schedules = schedule,
                Customer = null,
                Employee = null,
            };
            return Ok(invoiceReadDto);
        }

        [HttpGet("GetInvoicesByCustomerId/{customerId}")]
        public async Task<ActionResult<IEnumerable<InvoiceReadDto>>> GetInvoicesByCustomerId(int customerId)
        {
            var invoices = await _repository.GetInvoicesByCustomerId(customerId);

            var invoiceReadDtos = new List<InvoiceReadDto>();

            foreach (var invoice in invoices)
            {
                IEnumerable<Schedule> schedule = await _scheduleHttpService.GetScheduleByInvoiceId(invoice.Id);

                var invoiceReadDto = new InvoiceReadDto
                {
                    Id = invoice.Id,
                    EmployeeId = invoice.EmployeeId,
                    CustomerId = invoice.CustomerId,
                    PromotionId = invoice.PromotionId,
                    PaymentMethod = invoice.PaymentMethod,
                    Total = invoice.Total,
                    Status = invoice.Status,
                    Schedules = schedule,
                    Customer = null,
                    Employee = null,
                };

                invoiceReadDtos.Add(invoiceReadDto);
            };
            return Ok(invoiceReadDtos);
        }
       /* [HttpPost]
        public async Task<ActionResult<InvoiceCreateDto>> CreateInvoice(InvoiceCreateDto invoiceCreateDto)
        {
            try
            {
                var invoiceModel = _mapper.Map<Invoice>(invoiceCreateDto);
                var createdInvoice = await _repository.CreateInvoiceAsync(invoiceModel);

                if (invoiceCreateDto.InvoiceFoods != null && invoiceCreateDto.InvoiceFoods.Any())
                {
                    var invoiceFoods = invoiceCreateDto.InvoiceFoods.Select(f => new InvoiceFood
                    {
                        InvoiceId = createdInvoice.Id,
                        FoodId = f.FoodId,
                        Quantity = f.Quantity
                    }).ToList();

                    // Thêm món ăn vào hóa đơn
                    foreach (var invoiceFood in invoiceFoods)
                    {
                        createdInvoice.InvoiceFoods.Add(invoiceFood);
                    }

                    // Cập nhật hóa đơn trong cơ sở dữ liệu
                    await _repository.UpdateInvoiceAsync(createdInvoice);
                }
                
                // Update schedule with new invoice ID
                _salePublisherService.UpdateInvoiceIdInSchedule(invoiceCreateDto.Schedules);

                // Map created invoice back to InvoiceReadDto
                var invoiceReadDto = _mapper.Map<InvoiceReadDto>(createdInvoice);

                return CreatedAtAction(nameof(GetInvoiceById), new { invoiceId = invoiceReadDto.Id }, invoiceReadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating invoice");
            }*/
        }

    }
