using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class AuthProfiles : Profile
    {
        public AuthProfiles()
        {
            // Mapping from CustomerRegisterDto to the Customer Entity
            // We use the Email as the UserName as per standard Identity patterns
            CreateMap<CustomerRegisterDTO, Customer>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // Mapping from WorkerRegisterDto to the Worker Entity
            CreateMap<WorkerRegisterDTO, Worker>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        }
    }
}