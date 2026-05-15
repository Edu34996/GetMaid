using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class BookingProfiles : Profile
    {
        public BookingProfiles()
        {
            // Map the Create form to the Entity
            CreateMap<BookingCreateDTO, Booking>();

            // Map the Entity to the Display DTO, extracting names safely
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName : "Unknown"))
                .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.Worker != null ? src.Worker.FirstName : "Unknown"));
        }
    }
}