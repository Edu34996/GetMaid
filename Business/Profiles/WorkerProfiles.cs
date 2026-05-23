using System;
using System.Linq;
using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class WorkerProfiles : Profile
    {
        public WorkerProfiles()
        {
            // ==========================================
            // 1. REGISTRATION MAP
            // ==========================================
            CreateMap<WorkerRegisterDTO, Worker>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IdentityDocumentPath, opt => opt.Ignore());

            // ==========================================
            // 2. LOAD DASHBOARD FORM: Entity -> Update DTO
            // ==========================================
            
            /*CreateMap<Worker, WorkerProfileUpdateDTO>()
                .ForMember(dest => dest.ProfilePictureUrl,
                    opt => opt.MapFrom(src => src.ProfilePictureUrl));*/

            // ==========================================
            // 3. SAVE DASHBOARD FORM: Update DTO -> Entity
            // ==========================================
            CreateMap<WorkerProfileUpdateDTO, Worker>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FirstName, opt => opt.Ignore())
                .ForMember(dest => dest.LastName, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.IdentityVerificationStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Ignore());

            // ==========================================
            // 4. MARKETPLACE CARD: Entity -> Card DTO
            // ==========================================
            CreateMap<Worker, WorkerCardDTO>()
                .ForMember(dest => dest.NumberOfSkills,
                    opt => opt.MapFrom(src => src.Skills != null ? src.Skills.Count : 0))
                .ForMember(dest => dest.NumberOfLanguages,
                    opt => opt.MapFrom(src => src.LanguagesSpoken != null ? src.LanguagesSpoken.Count : 0))
                .ForMember(dest => dest.TotalReviews,
                    opt => opt.MapFrom(src => src.ReviewsReceived != null ? src.ReviewsReceived.Count : 0))
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src =>
                        src.ReviewsReceived != null && src.ReviewsReceived.Any()
                            ? Math.Round(src.ReviewsReceived.Average(r => r.Rating), 1)
                            : 0.0))
                .ForMember(dest => dest.AverageHourlyRate,
                    opt => opt.MapFrom(src =>
                        src.MinHourlyRate.HasValue && src.MaxHourlyRate.HasValue
                            ? (decimal?)((src.MinHourlyRate.Value + src.MaxHourlyRate.Value) / 2m)
                            : src.MinHourlyRate ?? src.MaxHourlyRate));

            // ==========================================
            // 5. MARKETPLACE DETAILS: Entity -> Details DTO
            // ==========================================
            CreateMap<Worker, WorkerDetailsDTO>()
                .ForMember(dest => dest.TotalReviews,
                    opt => opt.MapFrom(src => src.ReviewsReceived != null ? src.ReviewsReceived.Count : 0))
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src =>
                        src.ReviewsReceived != null && src.ReviewsReceived.Any()
                            ? Math.Round(src.ReviewsReceived.Average(r => r.Rating), 1)
                            : 0.0));
        }
    }
}