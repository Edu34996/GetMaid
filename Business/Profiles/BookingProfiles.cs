using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class BookingProfiles : Profile
    {
        public BookingProfiles()
        {
            // 1. Create Mapping
            // Map the unified Service Request to the Booking entity
            CreateMap<ServiceRequestCreateDTO, Booking>()
                .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.TargetWorkerId));

            // 2. List View Mapping (customer tracks worker)
            CreateMap<Booking, BookingListItemDTO>()
                .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.WorkerId))
                .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src =>
                    src.Worker != null ? $"{src.Worker.FirstName} {src.Worker.LastName}" : "Unassigned"))
                .ForMember(dest => dest.WorkerProfilePictureUrl, opt => opt.MapFrom(src =>
                    src.Worker != null ? src.Worker.ProfilePictureUrl : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // 3. Detail View Mapping
            CreateMap<Booking, BookingDetailDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName : "N/A"))
                .ForMember(dest => dest.CustomerAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Address : "N/A"))
                // IdentityUser (which Customer inherits from) has a PhoneNumber property
                .ForMember(dest => dest.CustomerPhoneNumber, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.PhoneNumber : "N/A"));
        }
    }
}