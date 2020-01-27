using AutoMapper;
using Core.ApiResponse;
using Core.Models.Users;
using Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<DB.Entities.Users> _usersRepository;
        private readonly IMapper _mapper;
        public UserService(IRepository<DB.Entities.Users> usersRepository, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _mapper = mapper;
        }
        public IServiceResponse<bool> Delete(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<UserModel> GetAll(int page, int pageSize, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<UserModel> GetById(long id, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<UserModel> GetById(int userId)
        {
            var response = new ServiceResponse<UserModel>(null);
            var user = _usersRepository.GetById(userId);
            if (user != null)
            {
                var model = _mapper.Map<UserModel>(user);
                response.Entity = model;
            }
            return response;
        }

        public IServiceResponse<UserModel> Insert(UserModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }

        public IServiceResponse<UserModel> Update(UserModel entityViewModel, int userId)
        {
            throw new NotImplementedException();
        }
        public IServiceResponse<UserModel> UpdateAdmin(int userId, bool isAdmin = true)
        {
            var response = new ServiceResponse<UserModel>(null);
            var user = _usersRepository.GetById(userId);
            if (user != null)
            {
                user.IsAdmin = isAdmin;
                _usersRepository.Update(user);
                var model = _mapper.Map<UserModel>(user);
                response.IsSuccessful = true;
                response.Entity = model;
            }
            return response;
        }
    }
}
