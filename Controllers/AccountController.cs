using gymnasium_academia.Filters;
using gymnasium_academia.Models.Identity;
using gymnasium_academia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

namespace gymnasium_academia.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private RoleManager<ApplicationRole> roleManager;
        private SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }

        [RedirectIfAuthenticated]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {

                var existingUser = userManager.Users.FirstOrDefault(u => u.Cpf == user.Cpf);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Este CPF já está cadastrado.");
                    return View(user);
                }

                ApplicationUser appUser = new ApplicationUser
                {
                    UserName = user.Email,
                    Email = user.Email,
                    NomeCompleto = user.NomeCompleto,
                    Cpf = user.Cpf,
                    DataCadastro = user.DataCadastro,
                    Assinante = user.Assinante,
                    DataNascimento = user.DataNascimento,
                    PhoneNumber = user.Telefone,
                    Tipo = user.Tipo = TipoUsuario.Usuario,
                    SecurityStamp = Guid.NewGuid().ToString()

                };

                await userManager.AddToRoleAsync(appUser, TipoUsuario.Usuario.ToString());

                IdentityResult result = await userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    ViewBag.Message = "Usuário criado com sucesso!";
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        [RedirectIfAuthenticated]
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> VerTodos()
        {
            var users = userManager.Users.ToList();
            return View(users); // Retorna a View com o modelo
        }

        [RedirectIfAuthenticated]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Required][EmailAddress] string email, [Required] string password, string? returnurl)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = await userManager.FindByEmailAsync(email);
                if (appUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(appUser, password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(returnurl ?? "/");
                    }
                }
                ModelState.AddModelError(nameof(email), "Falha de login: E-mail e/ou senha incorretos");
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        
    }
}
