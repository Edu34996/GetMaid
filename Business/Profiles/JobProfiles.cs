using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class JobProfiles : Profile
    {
        public JobProfiles()
        {
            // Mapping from the Create Form to the Database Entity
            CreateMap<JobPostingCreateDTO, JobPosting>();

            // Mapping from the Database Entity to the View DTO
            // We map the Customer's FirstName to the CustomerName property
            CreateMap<JobPosting, JobPostingDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName : "Unknown"));
        }
    }
}