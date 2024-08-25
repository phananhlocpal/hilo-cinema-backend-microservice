using JwtAuthenticationManager.Models;
using JwtAuthenticationManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthenticationService.Repositories;
using AuthenticationService.Dtos;
using AutoMapper;
using AuthenticationService.Models;
using AuthenticationService.Repositories.CustomerRepositories;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAuthenController : ControllerBase
    {
        private readonly JwtTokenHandler _jwtTokenHandler;
        private readonly IMapper _mapper;
        private readonly ICustomerRepo _repository;
        private readonly ILogger<CustomerAuthenController> _logger;
        public CustomerAuthenController(JwtTokenHandler jwtTokenHandler, IMapper mapper, ICustomerRepo repository, ILogger<CustomerAuthenController> logger)
        {
            _jwtTokenHandler = jwtTokenHandler;
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<AuthenticationResponse?> Authenticate([FromBody] AuthenticationRequest authencationRequest)
        {
            var authenticationResponse = _jwtTokenHandler.GenerateJwtToken(authencationRequest);
            if (authenticationResponse == null) return Unauthorized();
            return authenticationResponse;
        }

        //[HttpPost]
        //public async Task<ActionResult<CustomerReadDto>> CreateCustomer(CustomerCreateDto customerCreateDto)
        //{
        //    try
        //    {
        //        // Chuyển đổi DTO thành mô hình dữ liệu
        //        var customerModel = _mapper.Map<Customer>(customerCreateDto);

        //        // Tạo khách hàng mới trong cơ sở dữ liệu
        //        await _repository.CreateCustomerAsync(customerModel);
        //        await _repository.SaveChangesAsync();

        //        // Chuyển đổi mô hình dữ liệu thành DTO để trả về
        //        var customerReadDto = _mapper.Map<CustomerReadDto>(customerModel);

        //        // Trả về mã trạng thái 201 Created với thông tin khách hàng
        //        return CreatedAtAction(nameof(GetCustomerById), new { id = customerReadDto.Id }, customerReadDto);
        //    }
        //    catch (Exception e)
        //    {
        //        // Ghi lại lỗi và trả về mã trạng thái 500 Internal Server Error
        //        _logger.LogError(e, "An error occurred while creating the customer");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

    }
}
