using Mango.Services.ShoppingCart.Models.Dto;
using System.Threading.Tasks;

namespace Mango.Services.ShoppingCart.Repository
{
    public interface ICouponRepository
    {
        Task<CouponDto> GetCoupon(string couponName);
    }
}
