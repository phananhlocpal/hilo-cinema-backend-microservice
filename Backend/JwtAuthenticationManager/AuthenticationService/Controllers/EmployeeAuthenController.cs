<<<<<<< HEAD
﻿using AuthenticationService.Repositories;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
=======
﻿using AuthenticationService.Dtos;
using AuthenticationService.Models;
using AuthenticationService.Repositories;
using AuthenticationService.Repositories.EmployeeRepositories;
using AutoMapper;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Authorization;
>>>>>>> 960a83c (commit)
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeAuthenController : ControllerBase
    {
<<<<<<< HEAD
        private readonly JwtTokenHandler _jwtTokenHandler;

        public EmployeeAuthenController(JwtTokenHandler jwtTokenHandler)
        {
            _jwtTokenHandler = jwtTokenHandler;

        }

        [HttpPost]
        public ActionResult<AuthenticationResponse?> Authenticate([FromBody] AuthenticationRequest authencationRequest)
=======
        private readonly JwtTokenHandlerEmp _jwtTokenHandler;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IMapper _mapper;
        public EmployeeAuthenController(JwtTokenHandlerEmp jwtTokenHandler, IEmployeeRepo employeeRepo, IMapper mapper)
        {
            _jwtTokenHandler = jwtTokenHandler;
            _employeeRepo = employeeRepo;
            _mapper = mapper;

        }

        [HttpPost("authenticate")]
        public ActionResult<AuthenticationEmpResponse?> Authenticate([FromBody] AuthenticationEmpRequest authencationRequest)
>>>>>>> 960a83c (commit)
        {
            var authenticationResponse = _jwtTokenHandler.GenerateJwtToken(authencationRequest);
            if (authenticationResponse == null) return Unauthorized();
            return authenticationResponse;
<<<<<<< HEAD

        } 
=======
        }


        //Employee
        [Authorize(Policy = "AdminEmployeeOnly")]
        public async Task<ActionResult<IEnumerable<EmployeeReadDto>>> GetAllEmployees()
        {
            try
            {
                var employees = await _employeeRepo.();
                var employeeDtos = _mapper.Map<IEnumerable<EmployeeReadDto>>(employees);
                return Ok(employeeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}", Name = "GetEmployeeById")]
        public async Task<ActionResult<EmployeeReadDto>> GetEmployeeById(int id)
        {
            try
            {
                var employee = await _employeeRepo.GetByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                var employeeDto = _mapper.Map<EmployeeReadDto>(employee);
                return Ok(employeeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee with Id {id}.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<EmployeeReadDto>> CreateEmployee(EmployeeCreateDto employeeCreateDto)
        {
            try
            {
                // Set default password 
                employeeCreateDto.Password = "hilocinema@2024";

                var employee = _mapper.Map<Employee>(employeeCreateDto);
                await _employeeRepo.CreateAsync(employee);
                var isSaved = await _employeeRepo.SaveChangesAsync();

                // Send welcome email to employee
                _emailService.WelcomeEmail(employee);
                if (!isSaved)
                {
                    return StatusCode(500, "Error saving employee to database.");
                }

                var employeeReadDto = _mapper.Map<EmployeeReadDto>(employee);
                return CreatedAtRoute(nameof(GetEmployeeById), new { id = employeeReadDto.Id }, employeeReadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeReadDto employeeUpdateDto)
        {
            try
            {
                var existingEmployee = await _employeeRepo.GetByIdAsync(id);
                if (existingEmployee == null)
                {
                    return NotFound();
                }

                _mapper.Map(employeeUpdateDto, existingEmployee);
                await _employeeRepo.UpdateAsync(existingEmployee);
                var isSaved = await _employeeRepo.SaveChangesAsync();

                if (!isSaved)
                {
                    return StatusCode(500, "Error updating employee in database.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee with Id {id}.");
                return StatusCode(500, "Internal server error.");
            }
        }
>>>>>>> 960a83c (commit)
    }
}
