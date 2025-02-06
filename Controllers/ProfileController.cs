using gymnasium_academia.Models.Identity;
using gymnasium_academia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using gymnasium_academia.Filters;
using System.Security.Claims;

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
        [HttpGet("conta/historico-de-pedido")]
        public IActionResult HistoricoDePedido()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> VerFichasTreino()
        {
            // Obter o ID do usuário logado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"ID do usuário logado: {userId}");

            var usuario = await userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                Console.WriteLine("Usuário não encontrado. Redirecionando para a Home.");
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"Usuário encontrado: {usuario.UserName}");
            var fichaTreino = usuario.FichasTreino;
            return View(fichaTreino);
        }

        [HttpGet]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> DetalhesFichaTreino(string fichaId)
        {
            var aluno = await userManager.GetUserAsync(User);  // Pega o aluno logado
            if (aluno == null || !aluno.FichasTreino.Any())
            {
                TempData["Error"] = "Você não tem uma ficha.";
                return RedirectToAction("CriarFichaTreino");
            }

            // Verifica se o fichaId foi fornecido e é um GUID válido
            if (!Guid.TryParse(fichaId, out var fichaGuid))
            {
                TempData["Error"] = "ID da ficha inválido.";
                return RedirectToAction("VerFichasTreino");
            }

            // Encontra a ficha de treino correspondente ao fichaId
            var fichaTreino = aluno.FichasTreino.FirstOrDefault(f => f.Id == fichaGuid);
            if (fichaTreino == null)
            {
                TempData["Error"] = "Ficha de treino não encontrada.";
                return RedirectToAction("VerFichasTreino");
            }

            return View(fichaTreino);
        }

        [HttpGet]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> CriarFichaTreino()
        {
            var aluno = await userManager.GetUserAsync(User);
            if (aluno == null)
            {
                return RedirectToAction("Index", "Home"); // Redireciona se o aluno não for encontrado
            }

            // Verifica se o aluno já possui 3 fichas de treino
            if (aluno.FichasTreino.Count >= 3)
            {
                TempData["ErrorMessage"] = "Você já possui o limite de 3 fichas de treino.";
                return RedirectToAction("ExibirFichasTreino");
            }

            // Cria uma nova ficha de treino vazia
            var fichaTreino = new FichaTreino();

            return View(fichaTreino); // Retorna a view para o aluno preencher a ficha
        }

        [HttpPost]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> CriarFichaTreino(FichaTreino model)
        {
            try
            {
                // Validação e processamento do modelo
                if (ModelState.IsValid)
                {
                    // Obter o aluno logado
                    var aluno = await userManager.GetUserAsync(User);
                    if (aluno == null) return RedirectToAction("Index", "Home");

                    // Criar a ficha de treino
                    var fichaTreino = new FichaTreino
                    {
                        Id = Guid.NewGuid(),
                        UsuarioId = aluno.Id.ToString(),
                        NomeTreino = model.NomeTreino,
                        Descricao = model.Descricao,
                        DataInicio = DateTime.Now,
                        DiasDeTreino = model.DiasDeTreino,  // Aqui os dias e exercícios são mapeados corretamente
                        Observacoes = model.Observacoes
                    };

                    // Adicionar a ficha ao aluno
                    aluno.FichasTreino.Add(fichaTreino);
                    await userManager.UpdateAsync(aluno);

                    return RedirectToAction("VerFichasTreino");
                }

                // Se houver erro de validação
                return View(model);
            }
            catch (Exception ex)
            {
                // Log de erro
                return StatusCode(500, "Erro interno ao criar a ficha de treino.");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> DeletarFichaTreino(string fichaId)
        {
            var aluno = await userManager.GetUserAsync(User);
            // Verifica se o ID da ficha foi fornecido
            if (string.IsNullOrEmpty(fichaId) || string.IsNullOrEmpty(aluno.ToString()))
            {
                return RedirectToAction("VerFichasTreino", new { id = aluno });
            }

            // Encontre a ficha de treino dentro da lista de FichasTreino do aluno
            var fichaTreino = aluno.FichasTreino.FirstOrDefault(f => f.Id.ToString() == fichaId);

            if (fichaTreino == null)
            {
                // Se a ficha não for encontrada, redireciona para a lista de fichas
                return RedirectToAction("VerFichasTreino", new { id = aluno });
            }

            // Remove a ficha de treino
            aluno.FichasTreino.Remove(fichaTreino);

            // Salve as alterações no banco de dados
            var result = await userManager.UpdateAsync(aluno);
            if (result.Succeeded)
            {
                // Redireciona para a página de visualização de fichas
                return RedirectToAction("VerFichasTreino", new { id = aluno });
            }

            // Se algo deu errado, mostra uma mensagem de erro
            ModelState.AddModelError("", "Erro ao excluir a ficha de treino.");
            return RedirectToAction("VerFichasTreino", new { id = aluno });
        }


    }
}
