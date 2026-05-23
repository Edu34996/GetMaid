using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class JobProfiles : Profile
    {
        public JobProfiles()
        {
            // Entity -> lightweight card for list screens
            CreateMap<JobPosting, JobPostingCardDTO>();

            // Entity -> detail view
            CreateMap<JobPosting, JobPostingDetailDTO>()
                .ForMember(
                    dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName : "Unknown")
                );

            // Optional: create/update from service request DTO to entity.
            // Useful if you later switch CreateJobPostingAsync to use _mapper.Map<JobPosting>(model).
            CreateMap<ServiceRequestCreateDTO, JobPosting>()
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.PostInactive, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedWorkerId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedWorker, opt => opt.Ignore())
                .ForMember(dest => dest.Applications, opt => opt.Ignore());
        }
    }
}