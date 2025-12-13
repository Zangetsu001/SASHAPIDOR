using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduMaster.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduMaster.Controllers
{
    [Authorize] // Только для вошедших
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var cartItems = await _cartService.GetUserCartAsync(userId);
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(Guid enrollmentId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _cartService.RemoveFromCartAsync(userId, enrollmentId);
            return RedirectToAction("Index");
        }
    }
}