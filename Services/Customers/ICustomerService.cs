using Core.ApiResponse;
using Core.Filters.Customers;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using DB.Entities;

namespace Services.Customers
{
    public interface ICustomerService : IEntityService<CustomerModel>
    {
        ServiceResponse<CustomerModel> GetById(string customerId, int userId);
        //ServiceResponse<int> Insert(CustomerModel model, int userId);
        //ServiceResponse<int> Update(CustomerModel model, int userId);
        //ServiceResponse<bool> Delete(int id, int userId);
        List<DB.Entities.Customers> List(CustomerFilter filter);
        ServiceResponse<CustomerListModel> SearchCustomer(string name, int pageNo, int pageSize);
        ServiceResponse<Top5OrderModel> GetCustomerOrderByRawSql();
    }
}
