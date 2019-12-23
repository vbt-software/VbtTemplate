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
using Core.Caching;

namespace Services.Employees
{
    public class EmployeesService : IEmployeesService
    {
        private readonly IRepository<DB.Entities.Employees> _employeesRepository;
        private readonly IRepository<DB.Entities.Territories> _territoriesRepository;
        private readonly IRepository<DB.Entities.EmployeeTerritories> _employeeTerritoriesRepository;
        private readonly IRepository<DB.Entities.Region> _regionRepository;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redisCacheManager;
        public EmployeesService(IRepository<DB.Entities.Employees> employeesRepository, IMapper mapper, IRedisCacheService redisCacheManager, IRepository<DB.Entities.Territories> territoriesRepository,
            IRepository<DB.Entities.EmployeeTerritories> employeeTerritoriesRepository,
            IRepository<DB.Entities.Region> regionRepository)
        {
            _employeesRepository = employeesRepository;
            _territoriesRepository = territoriesRepository;
            _employeeTerritoriesRepository = employeeTerritoriesRepository;
            _regionRepository = regionRepository;

            _mapper = mapper;
            _redisCacheManager = redisCacheManager;
        }

        public ServiceResponse<EmployeesModel> SearchEmployees(string lastName, int pageNo, int pageSize)
        {
            lastName = string.IsNullOrWhiteSpace(lastName) ? string.Empty : lastName.ToLower(CultureInfo.CurrentCulture);

            //Check Redis
            var cacheKey = string.Format(CacheKeys.EmployeeList, lastName, pageNo, pageSize);
            var result = _redisCacheManager.Get<IList<EmployeesModel>>(cacheKey);
            //-------------------------------
            if (result != null)
            {
                var response = new ServiceResponse<EmployeesModel>(null);
                response.List = result;
                return response;
            }
            else
            {
                var query = _employeesRepository.Table
             .Where(k => EF.Functions.Like(k.LastName ?? string.Empty, $"%{lastName}%"))
             .OrderBy(c => c.EmployeeId)
             .Skip(pageNo * pageSize)
             .Take(pageSize)
             .ToList();
                var response = new ServiceResponse<EmployeesModel>(null);
                var models = _mapper.Map<List<EmployeesModel>>(query);
                response.List = models;
                _redisCacheManager.Set(cacheKey, response.List);
                return response;
            }

        }

        public ServiceResponse<EmployeeTerritory> GetEmployeeTerritory(string lastName, int pageNo, int pageSize)
        {
            lastName = string.IsNullOrWhiteSpace(lastName) ? string.Empty : lastName.ToLower(CultureInfo.CurrentCulture);

            //Check Redis
            var cacheKey = string.Format(CacheKeys.EmployeeTerritory, lastName, pageNo, pageSize);
            var result = _redisCacheManager.Get<IList<EmployeeTerritory>>(cacheKey);
            //-------------------------------
            if (result != null)
            {
                var response = new ServiceResponse<EmployeeTerritory>(null);
                response.List = result;
                return response;
            }
            else
            {             
                //BURAYI LINQ QUERY YAP
                var query = (from emp in _employeesRepository.Table
                            join emt in _employeeTerritoriesRepository.Table
                            on emp.EmployeeId equals emt.EmployeeId
                            join ter in _territoriesRepository.Table
                            on emt.TerritoryId equals ter.TerritoryId
                            join reg in _regionRepository.Table
                            on ter.RegionId equals reg.RegionId
                            where EF.Functions.Like(emp.LastName ?? string.Empty, $"%{lastName}%")
                            orderby emp.EmployeeId
                            select new EmployeeTerritory()
                            {
                                EmployeeId = emp.EmployeeId,
                                FirstName=emp.FirstName,
                                LastName = emp.LastName,
                                Title = emp.Title,
                                Territory = ter.TerritoryDescription,
                                Region = reg.RegionDescription
                            })
                            .Skip(pageNo * pageSize)
                            .Take(pageSize)
                            .ToList();            
                var response = new ServiceResponse<EmployeeTerritory>(null); 
                response.List = query;
                _redisCacheManager.Set(cacheKey, response.List);
                return response;
            }
        }

        public ServiceResponse<int> Update(CustomerModel model, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<EmployeesModel> GetAll(int page, int pageSize, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<EmployeesModel> GetById(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<EmployeesModel> Insert(EmployeesModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<EmployeesModel> Update(EmployeesModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<bool> Delete(long id, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
