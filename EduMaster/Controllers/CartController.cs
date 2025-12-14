using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduMaster.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduMaster.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // ПЕРЕИМЕНОВАНО: Index -> UserCart
        public async Task<IActionResult> UserCart()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetUserCartAsync(userId);
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Guid courseId)
        {
            var userId = GetUserId();
            await _cartService.AddToCartAsync(userId, courseId);
            return RedirectToAction("UserCart"); // Редирект на новое имя
        }

        [HttpPost]
        public async Task<IActionResult> Remove(Guid enrollmentId)
        {
            var userId = GetUserId();
            await _cartService.RemoveFromCartAsync(userId, enrollmentId);
            return RedirectToAction("UserCart"); // Редирект на новое имя
        }

        [HttpPost]
        public async Task<IActionResult> Purchase()
        {
            var userId = GetUserId();
            await _cartService.PurchaseCartAsync(userId);
            return RedirectToAction("Dashboard", "Home");
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }
    }
}