using System;
using DAL.Models;
using DAL.Models.Dto.User;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace DAL.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            
            CreateMap<UserRegisterDto, User>();

            
        }
    }
}
