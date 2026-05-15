using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;
using Utils.Generics;

namespace Data.Repositories
{
    public class CustomerRepository(GetMaidContext context) 
        : Repository<Customer>(context), ICustomerRepository
    {
        // The primary key for Customer is a string (from IdentityUser)
    }
}