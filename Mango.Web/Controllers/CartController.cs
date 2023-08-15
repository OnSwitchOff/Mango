using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUserAsync());
        }

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);

            if (response is { IsSuccess: true })
            {
                TempData["success"] = "Cart was updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

            if (response is { IsSuccess: true })
            {
                TempData["success"] = "Cart was applied successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            cartDto.CartHeader.CouponCode = "";
            ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);

            if (response is { IsSuccess: true })
            {
                TempData["success"] = "Cart was removed successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUserAsync()
        {
            var userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
            ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId);

            if (response is { IsSuccess: true})
            {
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                return cartDto;
            }
            return new CartDto();
        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            CartDto cart = await LoadCartDtoBasedOnLoggedInUserAsync();
            cart.CartHeader.Email = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            ResponseDto ? response = await _cartService.EmailCart(cart);

            if (response is { IsSuccess: true })
            {
                TempData["success"] = "Email will be proccessed and sent shortly.";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }
    }
}
