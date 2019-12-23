using Core.ApiResponse;
using Core.Filters.Customers;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Employees
{
    public interface IEmployeesService : IEntityService<EmployeesModel>
    {
        ServiceResponse<EmployeesModel> SearchEmployees(string name, int pageNo, int pageSize);
        ServiceResponse<EmployeeTerritory> GetEmployeeTerritory(string lastName, int pageNo, int pageSize);
    }
}
