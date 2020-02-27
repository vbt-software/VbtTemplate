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
using TemplateProject.Infrastructure;
using ServiceStack.DataAnnotations;
using Core;
using static Core.Enums;
using Core.Models.Roles;
using Services.Roles;
using Services.Users;
using Core.Models.Users;

namespace TemplateProject.Controllers
{
    [ServiceFilter(typeof(LoginFilter))]
    [ApiController]
    [Route("[controller]")]
    public class VbtController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        private readonly IRoleService _roleService;

        private readonly IMapper _mapper;

        private readonly ILogger<VbtController> _logger;

        private readonly IWorkContext _workContext;

        private readonly IUserService _userService;

        public VbtController(ICustomerService customerService, IMapper mapper, ILogger<VbtController> logger, IWorkContext workContext, IRoleService roleService, IUserService userService)
        {
            _customerService = customerService;
            _mapper = mapper;
            _logger = logger;
            _workContext = workContext;
            _roleService = roleService;
            _userService = userService;
        }

        [Infrastructure.IgnoreAttribute] //LoginFilter'a takılmaz.
        [HttpGet]
        public ServiceResponse<CustomerListModel> GetCustomer()
        {
            var response = new ServiceResponse<CustomerListModel>(HttpContext);
            response.List = _customerService.SearchCustomer("", 0, 10).List.ToList();
            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }
        [Infrastructure.RoleAttribute((int)RoleGroup.Customer, (Int64)CustomerRoles.GetCustomerById)] //LoginFilter'a takılmaz.
        [HttpGet("GetCustomerById/{CustomerID}")]
        public ServiceResponse<CustomerModel> GetCustomerById(string CustomerID)
        {
            var response = new ServiceResponse<CustomerModel>(HttpContext);
            response.List = _customerService.GetById(CustomerID, 0).List;
            response.IsSuccessful = true;
            response.Count = response.List.Count;
            //Get Global Variables
            var userID = _workContext.CurrentUserId;
            var IsMobile = _workContext.IsMobile;
            //
            return response;
        }
        [HttpGet("GetCustomerList/{CustomerID?}/{CompanyName?}/{ContactName?}")]
        public ServiceResponse<CustomerModel> GetCustomerList(string CustomerID, string CompanyName, string ContactName)
        {
            var response = new ServiceResponse<CustomerModel>(HttpContext);
            CustomerFilter filter = new CustomerFilter() { CustomerID = CustomerID, CompanyName = CompanyName, ContactName = ContactName };
            var query = _customerService.List(filter);
            var list = query.Select(s => new CustomerModel()
            {
                CustomerID = s.CustomerId,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                Address = s.Address,
                City = s.City,
                Country = s.Country,
                MobilePhone = s.Phone,
                Orders = _mapper.Map<List<OrderModel>>(s.Orders)
            }).ToList();
            response.List = list;

            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }
        [HttpGet("GetUserRolesByPage")]
        public ServiceResponse<RoleModel> GetUserRolesByPage()
        {
            var response = new ServiceResponse<RoleModel>(HttpContext);

            int roleGroupID = (int)RoleGroup.Customer;
            //Get Global Variables
            var userID = _workContext.CurrentUserId;

            var modelList = _roleService.GetRoleListByGroupId(userID, roleGroupID).List;
            response.List = modelList;

            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }

        [Infrastructure.AdminAttribute()] //AdminFilter
        [HttpGet("SetUserAdminRole/{userID}/{isAdmin}")]
        public ServiceResponse<RoleModel> SetUserAdminRole(int userID, bool isAdmin)
        {
            var response = new ServiceResponse<RoleModel>(HttpContext);
            _userService.UpdateAdmin(userID, isAdmin);
            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }

        [Infrastructure.RoleAttribute((int)RoleGroup.Customer, (Int64)CustomerRoles.InsertUser)]
        [Route("InsertUser")]
        [HttpPost]
        public ServiceResponse<UserModel> InsertUser([FromBody] UserModel model)
        {
            var response = new ServiceResponse<UserModel>(HttpContext);
            response.Entity = _userService.Insert(model, _workContext.CurrentUserId).Entity;
            response.IsSuccessful = true;
            return response;
        }

    }
}
