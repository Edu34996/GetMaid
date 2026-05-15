using Core.Concretes.Entities;
using Utils.Generics;

namespace Core.Abstracts.IRepositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        // You can add specific methods later, like:
        // Task<List<Booking>> GetCompletedBookingsAsync(string customerId, string workerId);
    }
}