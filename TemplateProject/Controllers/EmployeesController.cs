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
using TemplateProject.Infrastructure;
using static Core.Enums;
using Core.Models.Roles;
using Core;
using Services.Roles;

namespace TemplateProject.Controllers
{
    [ServiceFilter(typeof(LoginFilter))]
    [ApiController]
    [Route("[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeesService _employeesService;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IRoleService _roleService;

        public EmployeesController(IMapper mapper, IEmployeesService employeesService, IWorkContext workContext, IRoleService roleService)
        {
            _employeesService = employeesService;
            _mapper = mapper;
            _roleService = roleService;
            _workContext = workContext;
        }

        [HttpGet]
        [Infrastructure.RoleAttribute((int)RoleGroup.Employee, (Int64)EmployeeRoles.GetEmployees)] //LoginFilter'a takılmaz.
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

        [HttpGet("EmployeeWithTerritoriesByContext")]
        public ServiceResponse<EmployeeTerritory> GetEmployeeWithTerritoriesByContext()
        {
            var response = new ServiceResponse<EmployeeTerritory>(HttpContext);
            response.List = _employeesService.GetEmployeeTerritoryByContext("", 0, 10).List.ToList();
            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }

        [Infrastructure.LogAttribute]
        [HttpPost]
        public ServiceResponse<Employees> UpdateEmployee([FromBody] EmployeeTerritory model)
        {
            var response = new ServiceResponse<Employees>(HttpContext);
            var updateEmployee = _employeesService.Update(model);
            return response;
        }

        [HttpGet("GetUserRolesByPage")]
        public ServiceResponse<RoleModel> GetUserRolesByPage()
        {
            var response = new ServiceResponse<RoleModel>(HttpContext);

            int roleGroupID = (int)RoleGroup.Employee;
            //Get Global Variables
            var userID = _workContext.CurrentUserId;

            var modelList = _roleService.GetRoleListByGroupId(userID, roleGroupID).List;
            response.List = modelList;

            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }

        [HttpGet("GetSalesFor1997")]
        public ServiceResponse<CategorySalesFor1997> GetSalesFor1997()
        {
            var response = new ServiceResponse<CategorySalesFor1997>(HttpContext);
            var modelList = _employeesService.GetSalesFor1997().List;
            response.List = modelList;

            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }
    }
}
