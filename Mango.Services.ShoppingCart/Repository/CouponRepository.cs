using Mango.Services.ShoppingCart.Models.Dto;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mango.Services.ShoppingCart.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly HttpClient client;

        public CouponRepository(HttpClient client)
        {
            this.client = client;
        }

        public async Task<CouponDto> GetCoupon(string couponName)
        {
            var response = await client.GetAsync($"/api/coupon/{couponName}");

            var apiContent = await response.Content.ReadAsStringAsync();

            var res = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

            if (res != default && res.IsSuccess)
            {
                return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(res.Result));
            }

            return new CouponDto();
        }
    }
}
