﻿using AutoMapper;
using Jobportal.Data.Services;
using Jobportal.Models;
using Jobportal.Services;
using JobPortal.Data.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace JobPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        

        public EmployeeController(IEmployeeRepository employeeRepository, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]EmployeeDto employeeParam)
        {
            var employee = _employeeRepository.Authenticate(employeeParam.Username, employeeParam.Password);

            if (employee == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, employee.EmployeeId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(employee);
        }



        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]EmployeeDto employeeDto)
        {
            // map dto to entity
            var employee = _mapper.Map<Employee>(employeeDto);

            try
            {
                // save 
                _employeeRepository.CreateEmployee(employee, employeeDto.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetEmployees()
        {
            var employees = _employeeRepository.GetEmployees();
            var employeeDtos = _mapper.Map<IList<EmployeeDto>>(employees);
            return Ok(employeeDtos);
        }



        [HttpGet("{employeeId}")]
        public IActionResult GetEmployee(int employeeId)
        {
            var employee = _employeeRepository.GetEmployee(employeeId);
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Ok(employeeDto);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]EmployeeDto employeeDto)
        {
            // map dto to entity and set id
            var employee = _mapper.Map<Employee>(employeeDto);
            employee.EmployeeId = id;

            try
            {
                // save 
                _employeeRepository.UpdateEmployee(employee, employeeDto.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}
