﻿using AutoMapper;
using Core.ApiResponse;
using Core.Models.Roles;
using Repository;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<DB.Entities.UserRoles> _userRolesRepository;
        private readonly IRepository<DB.Entities.Roles> _rolesRepository;
        private readonly IMapper _mapper;
        public RoleService(IRepository<DB.Entities.UserRoles> userRolesRepository, IRepository<DB.Entities.Roles> rolesRepository, IMapper mapper)
        {
            _userRolesRepository = userRolesRepository;
            _rolesRepository = rolesRepository;
            _mapper = mapper;
        }

        public IServiceResponse<bool> Delete(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<RoleModel> GetAll(int page, int pageSize, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<RoleModel> GetById(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<RoleModel> GetRoleById(int userId, int roleGroupID, Int64 roleID)
        {
            var response = new ServiceResponse<RoleModel>(null);
            RoleModel model = new RoleModel();
            var userRole = _userRolesRepository.Table
                .Include(r => r.RoleGroup)
                .FirstOrDefault(ur => ur.UserId == userId && ur.RoleGroupId == roleGroupID);
            if (userRole != null)
            {
                if (roleID == (userRole.Roles & roleID))
                {
                    var role = _rolesRepository.Table.Where(r => r.RoleId == roleID).FirstOrDefault();
                    if (role != null)
                    {
                        model = new RoleModel() { Id = role.Id, RoleName = role.RoleName, RoleGroupID = (int)userRole.RoleGroupId, RoleID = roleID, UserID = userId, GroupName = userRole.RoleGroup.GroupName };
                    }
                }
                response.Entity = model;
            }
            return response;
        }

        public IServiceResponse<RoleModel> GetRoleListByGroupId(int userId, int roleGroupID)
        {
            var response = new ServiceResponse<RoleModel>(null);
            List<RoleModel> model = new List<RoleModel>();
            var userRole = _userRolesRepository.Table.FirstOrDefault(ur => ur.UserId == userId && ur.RoleGroupId == roleGroupID);
            if (userRole != null)
            {
                var allRoles = _rolesRepository.Table
                    .Include(r => r.Group)
                    .Where(r => r.GroupId == roleGroupID).ToList();
                foreach (var role in allRoles)
                {
                    if (role.RoleId == (userRole.Roles & role.RoleId))
                    {
                        model.Add(new RoleModel() { Id = role.Id, RoleName = role.RoleName, RoleGroupID = (int)role.GroupId, RoleID = (int)role.RoleId, UserID = userId, GroupName = role.Group.GroupName });
                    }
                }
                response.List = model;
            }
            return response;
        }

        public IServiceResponse<RoleModel> Insert(RoleModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<RoleModel> Update(RoleModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
