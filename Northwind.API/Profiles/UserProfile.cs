﻿using AutoMapper;
using Northwind.API.Models;
using Northwind.API.Models.Auth;
using Northwind.Data.Entities;

namespace Northwind.API.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RefreshToken, RefreshTokenModel>();

            CreateMap<AuthRequestModel, User>();

            CreateMap<User, AuthResponseModel>()
                .ForMember(authResponse => authResponse.RefreshToken,
                           opt => opt.MapFrom(u => u.RefreshToken.Value));
        }
    }
}