using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class CustomerProfiles : Profile
    {
        public CustomerProfiles()
        {
            // Register → Entity
            CreateMap<CustomerRegisterDTO, Customer>();

            // Entity → Dashboard
            CreateMap<Customer, CustomerDashboardDTO>();

            // Entity → Details (public)
            CreateMap<Customer, CustomerDetailsDTO>()
                .ForMember(d => d.MemberSince, o => o.MapFrom(s => s.CreatedAt));

            // Entity → Card
            CreateMap<Customer, CustomerCardDTO>();

            // Update DTO → Entity
            CreateMap<CustomerProfileUpdateDTO, Customer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Child mappings
            // Entity -> DTO
            CreateMap<Child, ChildDTO>();

            // DTO -> Entity
            // Ignore null source members so optional fields don't wipe existing values on edit.
            // Ignore Id so EF keeps tracked entity identity untouched on updates.
            CreateMap<ChildDTO, Child>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}