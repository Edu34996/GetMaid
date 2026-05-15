using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Core.Abstracts;
using Core.Abstracts.IRepositories;
using Data.Contexts;
using Data.Repositories;
using Utils.Responses;

namespace Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GetMaidContext _context;
        private IDbContextTransaction? _transaction;

        // Private fields for lazy loading
        private ICustomerRepository? _customers;
        private IWorkerRepository? _workers;
        private IChildRepository? _children;
        private IBookingRepository? _bookings;
        private IReviewRepository? _reviews;
        private IMessageRepository? _messages;
        private IJobPostingRepository? _jobPostings;
        private IJobApplicationRepository? _jobApplications;


        public UnitOfWork(GetMaidContext context)
        {
            _context = context;
        }

        // Repository Properties with Lazy Loading logic
        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
        public IWorkerRepository Workers => _workers ??= new WorkerRepository(_context);
        public IChildRepository Children => _children ??= new ChildRepository(_context);
        public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
        public IMessageRepository Messages => _messages ??= new MessageRepository(_context);
        public IJobPostingRepository JobPostings => _jobPostings ??= new JobPostingRepository(_context);
        public IJobApplicationRepository JobApplications => _jobApplications ??= new JobApplicationRepository(_context);
        
        
        public async Task<IResult> CommitAsync()
        {try
            {
                // Entity Framework Core handles the actual transaction logic here
                await _context.SaveChangesAsync();
                return Result.Success(200);
            }
            catch (Exception ex)
            {
                // Captures database-level exceptions (e.g., constraint violations)
                return Result.Failure([ex.Message], 500);
            }
        }

        // Transaction Methods
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}