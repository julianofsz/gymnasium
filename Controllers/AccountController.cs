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
        [HttpGet("registrar")]
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

        [HttpGet("conta/editar")]
        [Authorize]
        public async Task<IActionResult> Editar()
        {
            // Obtém o email do usuário logado
            var user = await userManager.FindByEmailAsync(User.Identity.Name);  // Usa o email do usuário logado
            if (user == null)
            {
                TempData["Error"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            var model = new UserUpdate
            {
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                Telefone = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost("conta/editar")]
        [Authorize]
        public async Task<IActionResult> Editar(UserUpdate model)
        {
            if (string.IsNullOrEmpty(model.SenhaAtual))
            {
                ModelState.AddModelError("SenhaAtual", "A senha atual é obrigatória.");
                return View(model);
            }

            // Obtém o email do usuário logado (usado como UserName)
            var email = User.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Erro: Não foi possível identificar o usuário logado.";
                return RedirectToAction("Index", "Home");
            }

            // Busca o usuário pelo email
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "Usuário não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            // Verifica a senha atual
            var senhaValida = await userManager.CheckPasswordAsync(user, model.SenhaAtual);
            if (!senhaValida)
            {
                ModelState.AddModelError("SenhaAtual", "A senha atual está incorreta.");
                return View(model);
            }

            // Atualiza somente os campos que foram preenchidos
            bool isUpdated = false;

            if (!string.IsNullOrWhiteSpace(model.NomeCompleto))
            {
                user.NomeCompleto = model.NomeCompleto;
                isUpdated = true;
            }

            // Verifica e altera o email
            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
            {
                var emailExistente = await userManager.FindByEmailAsync(model.Email);
                if (emailExistente != null)
                {
                    ModelState.AddModelError("Email", "Este email já está em uso.");
                }
                else
                {
                    user.Email = model.Email;
                    user.UserName = model.Email; // Atualiza também o UserName
                    isUpdated = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Telefone))
            {
                user.PhoneNumber = model.Telefone;
                isUpdated = true;
            }

            // Caso nenhum campo tenha sido atualizado
            if (!isUpdated)
            {
                TempData["Warning"] = "Nenhuma alteração foi feita.";
                return View(model);
            }

            // Salva as alterações
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Reautentica o usuário após a alteração de email/username
                await signInManager.RefreshSignInAsync(user);

                TempData["Success"] = "Dados atualizados com sucesso!";
                return RedirectToAction("Perfil"); // Redireciona para evitar reenvio de formulário
            }

            // Caso haja erros no update, adiciona ao ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Perfil");
        }

        [Authorize]
        [HttpGet("conta/mudar-senha")]
        public IActionResult MudarSenha()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MudarSenha(string senhaAtual, string novaSenha, string confirmarNovaSenha)
        {
            if (string.IsNullOrWhiteSpace(senhaAtual) || string.IsNullOrWhiteSpace(novaSenha) || string.IsNullOrWhiteSpace(confirmarNovaSenha))
            {
                ModelState.AddModelError(string.Empty, "Todos os campos são obrigatórios.");
                return View();
            }

            if (novaSenha != confirmarNovaSenha)
            {
                ModelState.AddModelError(string.Empty, "A nova senha e a confirmação não coincidem.");
                return View();
            }

            // Obter o usuário autenticado
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tentar alterar a senha
            var result = await userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            TempData["SuccessMessage"] = "Senha atualizada com sucesso.";
            return View(user);
        }

        public IActionResult BuscarAlunosPorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return PartialView("_EmailSearchResults", new List<ApplicationUser>());
            }

            var alunos = userManager.Users
                .Where(u => u.Email.Contains(email))
                .ToList();

            return PartialView("_EmailSearchResults", alunos);
        }

        [Authorize]
        [HttpGet("conta/meu-perfil")]
        public async Task<IActionResult> Perfil()
        {
            var user = await userManager.GetUserAsync(User);

            return View(user);
        }

    }
}
