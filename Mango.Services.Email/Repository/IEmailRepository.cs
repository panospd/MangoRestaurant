using Mango.Services.Email.Messages;
using System.Threading.Tasks;

namespace Mango.Services.OrderAPI.Repository
{
    public interface IEmailRepository
    {
        Task SendAndLogEmail(UpdatePaymentResultMessage message);
    }
}
