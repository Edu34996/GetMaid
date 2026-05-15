using System.Threading.Tasks;
using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.IServices
{
    public interface IAuthService
    {
        Task<IResult> RegisterCustomerAsync(CustomerRegisterDTO model);

        Task<IResult> RegisterWorkerAsync(WorkerRegisterDTO model);

        Task<IResult> LoginAsync(LoginDTO model);

        Task LogoutAsync();
    }
}