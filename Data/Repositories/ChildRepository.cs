using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;
using Utils.Generics;

namespace Data.Repositories
{
    public class ChildRepository(GetMaidContext context) 
        : Repository<Child>(context), IChildRepository
    {
    }
}