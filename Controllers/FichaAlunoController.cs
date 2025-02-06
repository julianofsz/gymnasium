using gymnasium_academia.Models.Identity;
using gymnasium_academia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace gymnasium_academia.Controllers
{
    public class FichaAlunoController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private RoleManager<ApplicationRole> roleManager;

        public FichaAlunoController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // GET: Profile/CriarFichaAluno
        [Authorize]
        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar(FichaAluno fichaAluno)
        {
            if (ModelState.IsValid)
            {
                var aluno = await userManager.GetUserAsync(User);  // Pega o aluno logado
                if (aluno != null)
                {
                    if (aluno.FichasAluno == null)
                    {
                        aluno.FichasAluno = new List<FichaAluno>();
                    }

                    if (aluno.FichasAluno.Count > 0)
                    {
                        ViewBag.Message = "Você já tem uma ficha cadastrada!";
                        return View(fichaAluno);  // Retorna o formulário com a mensagem de erro
                    }

                    // Aqui, adicionamos a ficha ao usuário logado
                    aluno.FichasAluno.Add(fichaAluno);
                    
                    // Salva o usuário com a nova ficha
                    var result = await userManager.UpdateAsync(aluno);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = "Ficha do aluno cadastrada com sucesso!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            return View(fichaAluno);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Editar()
        {
            var aluno = await userManager.GetUserAsync(User);  // Pega o aluno logado
            if (aluno != null && aluno.FichasAluno.Any())
            {
                var fichaAluno = aluno.FichasAluno.First();  // Pega a primeira ficha, já que só pode haver uma
                return View(fichaAluno);
            }

            TempData["Error"] = "Você não tem uma ficha para editar.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Editar(FichaAluno fichaAluno)
        {
            if (ModelState.IsValid)
            {
                var aluno = await userManager.GetUserAsync(User);
                if (aluno != null && aluno.FichasAluno.Any())
                {
                    var fichaExistente = aluno.FichasAluno.First();
                    // Atualiza a ficha com os novos dados
                    fichaExistente.Endereco = fichaAluno.Endereco;
                    fichaExistente.EstadoCivil = fichaAluno.EstadoCivil;
                    fichaExistente.TemAnamnese = fichaAluno.TemAnamnese;

                    var result = await userManager.UpdateAsync(aluno);
                    if (result.Succeeded)
                    {
                        ViewBag.Message = "Ficha atualizada com sucesso!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            return View(fichaAluno);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Detalhes()
        {
            var aluno = await userManager.GetUserAsync(User);  // Pega o aluno logado
            if (aluno != null && aluno.FichasAluno.Any())
            {
                var fichaAluno = aluno.FichasAluno.First();
                return View(fichaAluno);
            }

            TempData["Error"] = "Você não tem uma ficha preenchida.";
            return RedirectToAction("Criar", "FichaAluno");
        }
    }
}
