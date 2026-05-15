using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class CustomerProfiles : Profile
    {
        public CustomerProfiles()
        {
            // Map Entity to Dashboard DTO
            CreateMap<Customer, CustomerDashboardDTO>();

            // Map Update DTO to Entity
            CreateMap<CustomerProfileUpdateDTO, Customer>();

            // Map Child Entity to Child DTO (ReverseMap allows bi-directional mapping)
            CreateMap<Child, ChildDTO>().ReverseMap();
        }
    }
}