using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;

namespace Business.Profiles
{
    public class WorkerProfiles : Profile
    {
        public WorkerProfiles()
        {
            // Maps the database entity to the display DTO
            CreateMap<Worker, WorkerDashboardDTO>();

            // Maps the incoming UI data onto the database entity
            CreateMap<WorkerProfileUpdateDTO, Worker>();
        }
    }
}