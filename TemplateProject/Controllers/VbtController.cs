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

namespace TemplateProject.Controllers
{
    [ServiceFilter(typeof(LoginFilter))]
    [ApiController]
    [Route("[controller]")]
    public class VbtController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        private readonly IMapper _mapper;

        private readonly ILogger<VbtController> _logger;

        private readonly IWorkContext _workContext;

        public VbtController(ICustomerService customerService, IMapper mapper, ILogger<VbtController> logger, IWorkContext workContext)
        {
            _customerService = customerService;
            _mapper = mapper;
            _logger = logger;
            _workContext = workContext;
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
    }
}
