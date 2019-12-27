using AutoMapper;
using Core.Models;
using Core.Models.Users;
using DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateProject.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Order Mapper
            CreateMap<Orders, OrderModel>();
            CreateMap<OrderModel, Orders>();
            #endregion

            #region Customer Mapper
            CreateMap<Customers, CustomerModel>()
            .ForMember(c => c.MobilePhone, t => t.MapFrom(src => src.Phone));
            CreateMap<CustomerModel, Customers>()
            .ForMember(c => c.Phone, t => t.MapFrom(src => src.MobilePhone));
            #endregion

            #region Employees Mapper
            CreateMap<Employees, EmployeesModel>();
            CreateMap<EmployeesModel, Employees>();
            #endregion

            #region User Mapper
            CreateMap<Users, UserModel>();
            CreateMap<UserModel, Users>();
            #endregion
        }
    }
}
