using System;
using System.Threading.Tasks;
using Core.Abstracts.IRepositories;
using Utils.Responses; // Required for IResult

namespace Core.Abstracts
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // Core Users
        ICustomerRepository Customers { get; }
        IWorkerRepository Workers { get; }
        IChildRepository Children { get; }
        
        // Platform Features
        IBookingRepository Bookings { get; }
        IReviewRepository Reviews { get; }
        IMessageRepository Messages { get; }
        IJobPostingRepository JobPostings { get; }
        IJobApplicationRepository JobApplications { get; }
        
        
        // The method your service is trying to call
        Task<IResult> CommitAsync();

        // Include your new transaction methods here as well so services can use them
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        
        
    }
}