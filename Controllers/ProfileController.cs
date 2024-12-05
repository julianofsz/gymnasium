using gymnasium_academia.Models.Identity;
using gymnasium_academia.Models.ViewModels;
using gymnasium_academia.Models.Treinos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using gymnasium_academia.Filters;

namespace gymnasium_academia.Controllers
{
    public class ProfileController : Controller
    {
        private UserManager<ApplicationUser> userManager;

        public ProfileController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Detalhes()
        {
            var user = await userManager.GetUserAsync(User);

            return View(user);
        }

        [Authorize]
        [RedirectIfAuthenticated]
        [HttpGet]
        public async Task<IActionResult> FichaTreino()
        {
            var ficha = await userManager.GetUserAsync(User);

            return View(ficha);
        }

        [Authorize]
        [HttpGet("conta/historico-de-pedido")]
        public IActionResult HistoricoDePedido()
        {
            return View();
        }


    }
}
