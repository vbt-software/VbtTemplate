using Core.ApiResponse;
using Core.Filters.Customers;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AutoMapper;
using DB.Entities;

namespace Services.Customers
{
    public class CustomerService : ICustomerService
    {
        //private readonly IRepository<DB.Entities.Customers> _customerRepository;
        private readonly IRepository<DB.Entities.Customers> _customerRepository;
        private readonly IRepository<DB.Entities.Top5OrderModel> _customerOrderRepository;
        private readonly IRepository<DB.Entities.VwCustomerProducts> _customerProductRepository;
        private readonly IMapper _mapper;
        public CustomerService(IRepository<DB.Entities.Customers> customerRepository, IRepository<DB.Entities.VwCustomerProducts> customerProductRepository, IMapper mapper, IRepository<DB.Entities.Top5OrderModel> customerOrderRepository)
        {
            _customerRepository = customerRepository;
            _customerProductRepository = customerProductRepository;
            _mapper = mapper;
            _customerOrderRepository = customerOrderRepository;
        }

        public IServiceResponse<bool> Delete(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<CustomerModel> GetAll(int page, int pageSize, int userId)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<CustomerModel> GetById(string name, int userId)
        {
            name = string.IsNullOrWhiteSpace(name) ? string.Empty : name.ToLower(CultureInfo.CurrentCulture);
            var query = _customerRepository.Table
                .Include(s => s.Orders)
                .Where(k => EF.Functions.Like(k.CustomerId ?? string.Empty, $"%{name}%"))
                 //.Where(k => (EF.Functions.Like(k.CustomerId ?? string.Empty, $"%{name}%")) && (k.IsDeleted == true))
                 .OrderBy(c => c.CustomerId).ToList();
            var models = _mapper.Map<List<CustomerModel>>(query);
            /*var models = query.Select(s => new CustomerModel()
            {
                CustomerID = s.CustomerId,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                Address = s.Address,
                City = s.City,
                Country = s.Country,
                Phone = s.Phone,
                Orders = _mapper.Map<List<OrderModel>>(s.Orders)
                //Orders = s.Orders.Select(o => new OrderModel()
                //{
                //    CustomerId = o.CustomerId,
                //    EmployeeId = o.EmployeeId,
                //    OrderDate = o.OrderDate,
                //    RequiredDate = o.RequiredDate,
                //    ShippedDate = o.ShippedDate,
                //    ShipVia = o.ShipVia,
                //    //Freight = o.Freight,
                //    ShipName = o.ShipName,
                //    ShipAddress = o.ShipAddress,
                //    ShipCity = o.ShipCity,
                //    ShipRegion = o.ShipRegion,
                //    ShipPostalCode = o.ShipPostalCode,
                //    ShipCountry = o.ShipCountry
                //}).ToList()
            }).ToList();
            */
            var response = new ServiceResponse<CustomerModel>(null);
            response.List = models;
            return response;
        }

        public IServiceResponse<CustomerModel> GetById(long id)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<CustomerModel> GetById(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<Top5OrderModel> GetCustomerOrderByRawSql()
        {
            string sqlQuery = @"select top 5 cus.CustomerID, cus.CompanyName, ord.ShipCountry,sum(ordd.Quantity) as Total  from [dbo].[Orders] as ord
                                inner join [dbo].[Customers] as cus on ord.CustomerID=cus.CustomerID
                                inner join [dbo].[Order Details] as ordd on ord.CustomerID=cus.CustomerID
                                group by ShipCountry,CompanyName,cus.CustomerID
                                order by Total desc";
            var result = _customerOrderRepository.GetSql(sqlQuery);
            var response = new ServiceResponse<Top5OrderModel>(null);
            response.List = result.ToList();
            return response;
        }

        public IServiceResponse<CustomerModel> Insert(CustomerModel model, int userId)
        {
            throw new NotImplementedException();
        }

        public List<DB.Entities.Customers> List(CustomerFilter filter)
        {
            string name = string.IsNullOrWhiteSpace(filter.CustomerID) ? string.Empty : filter.CustomerID.ToLower(CultureInfo.CurrentCulture);
            string companyName = string.IsNullOrWhiteSpace(filter.CompanyName) ? string.Empty : filter.CompanyName.ToLower(CultureInfo.CurrentCulture);
            string contactName = string.IsNullOrWhiteSpace(filter.ContactName) ? string.Empty : filter.ContactName.ToLower(CultureInfo.CurrentCulture);
            var response = _customerRepository.Table
                .Include(s => s.Orders)
                .Where(k => (EF.Functions.Like(k.CustomerId ?? string.Empty, $"%{name}%")) &&
                 (EF.Functions.Like(k.CompanyName ?? string.Empty, $"%{companyName}%")) &&
                 (EF.Functions.Like(k.ContactName ?? string.Empty, $"%{contactName}%")))
                .ToList();
            return response;
        }

        public ServiceResponse<CustomerListModel> SearchCustomer(string name, int pageNo, int pageSize)
        {
            name = string.IsNullOrWhiteSpace(name) ? string.Empty : name.ToLower(CultureInfo.CurrentCulture);
            var query = _customerProductRepository.Table
             .Where(k => EF.Functions.Like(k.CustomerId ?? string.Empty, $"%{name}%"))
             .OrderBy(c => c.CustomerId)
             .Skip(pageNo * pageSize)
             .Take(pageSize)
             .ToList();
            var response = new ServiceResponse<CustomerListModel>(null);
            var models = query.Select(s => new CustomerListModel()
            {
                CustomerID = s.CustomerId,
                ProductName = s.ProductName,
                Quantity = s.Quantity,
                ShipCountry = s.ShipCountry,
                Total = (decimal)s.Total,
                UnitPrice = s.UnitPrice
            }).ToList();
            response.List = models;
            return response;
        }

        public IServiceResponse<CustomerModel> Update(CustomerModel model, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
