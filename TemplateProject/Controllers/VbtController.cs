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

namespace TemplateProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VbtController : ControllerBase
    {       
        private readonly ICustomerService _customerService;

        private readonly IMapper _mapper;

        private readonly ILogger<VbtController> _logger;

        public VbtController(ICustomerService customerService, IMapper mapper, ILogger<VbtController> logger)
        {
            _customerService = customerService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public ServiceResponse<CustomerListModel> GetCustomer()
        {
            var response = new ServiceResponse<CustomerListModel>(HttpContext);
            response.List = _customerService.SearchCustomer("", 0, 10).List.ToList();
            response.IsSuccessful = true;
            response.Count = response.List.Count;
            return response;
        }
        [HttpGet("GetCustomerById/{CustomerID}")]
        public ServiceResponse<CustomerModel> GetCustomerById(string CustomerID)
        {
            var response = new ServiceResponse<CustomerModel>(HttpContext);
            response.List = _customerService.GetById(CustomerID, 0).List;
            response.IsSuccessful = true;
            response.Count = response.List.Count;
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
