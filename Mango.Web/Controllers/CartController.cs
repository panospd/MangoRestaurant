﻿using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public CartController(IProductService productService, ICartService cartService, ICouponService couponService)
        {
            _productService = productService;
            _cartService = cartService;
            _couponService = couponService;
        }

        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        [HttpPost]
        [ActionName("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _cartService.ApplyCouponAsync<ResponseDto>(cartDto, accessToken);

            if (response != default && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }

        [HttpPost]
        [ActionName("RemoveCoupon")]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _cartService.RemoveCouponAsync<ResponseDto>(cartDto.CartHeader.UserId, accessToken);

            if (response != default && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _cartService.RemoveFromCartAsync<ResponseDto>(cartDetailsId, accessToken);

            if (response != default && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }

        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var response = await _cartService.GetCartByUserIdAsync<ResponseDto>(userId, accessToken);

            CartDto cartDto = new();
            if(response != default && response.IsSuccess)
            {
                cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
            }

            if (cartDto.CartHeader != default)
            {
                if(!string.IsNullOrEmpty(cartDto.CartHeader.CouponCode))
                {
                    var couponResponse = await _couponService.GetCouponAsync<ResponseDto>(cartDto.CartHeader.CouponCode, accessToken);
                    if (couponResponse != default && couponResponse.IsSuccess)
                    {
                        var couponDto = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(couponResponse.Result));
                        cartDto.CartHeader.DiscountTotal = couponDto.DiscountAmount;
                    }
                }
                foreach (var detail in cartDto.CartDetails)
                {
                    cartDto.CartHeader.OrderTotal += (detail.Count * detail.Product.Price);
                }

                cartDto.CartHeader.OrderTotal -= cartDto.CartHeader.DiscountTotal;
            }

            return cartDto;
        }
    }
}