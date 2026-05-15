using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;
using Utils.Generics;

namespace Data.Repositories
{
    public class WorkerRepository(GetMaidContext context) : Repository<Worker>(context), IWorkerRepository
    {
    }
}