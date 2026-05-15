using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class ReviewProfiles : Profile
    {
        public ReviewProfiles()
        {
            CreateMap<ReviewCreateDTO, Review>();

            // Map the entity to the DTO. 
            // Note: To get the ReviewerName, we will need to load the User data in our Service.
            CreateMap<Review, ReviewDTO>(); 
        }
    }
}