using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.ApiResponse;
using Core.Models;
using Core.Extensions;
using DB.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository;
using Services;
using Services.Customers;
using AutoMapper;
using Core.Filters.Customers;
using Services.Employees;

namespace TemplateProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeesService _employeesService;
        private readonly IMapper _mapper;
        public EmployeesController(IMapper mapper, IEmployeesService employeesService)
        {
            _employeesService = employeesService;
            _mapper = mapper;
        }

        [HttpGet]
        public ServiceResponse<EmployeesModel> GetEmployees()
        {
            var response = new ServiceResponse<EmployeesModel>(HttpContext);
            response.List = _employeesService.SearchEmployees("", 0, 10).List.ToList();
            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }

        [HttpGet("EmployeeWithTerritories")]
        public ServiceResponse<EmployeeTerritory> GetEmployeeWithTerritories()
        {
            var response = new ServiceResponse<EmployeeTerritory>(HttpContext);
            response.List = _employeesService.GetEmployeeTerritory("", 0, 10).List.ToList();
            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }
    }
}
