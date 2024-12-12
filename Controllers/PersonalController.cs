using gymnasium_academia.Models.Identity;
using gymnasium_academia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace gymnasium_academia.Controllers
{

    public class PersonalController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private RoleManager<ApplicationRole> roleManager;

        public PersonalController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [Authorize(Roles = "Personal")]
        [HttpGet("personal/alunos")]
        public async Task<IActionResult> VerAlunos()
        {
            var users = userManager.Users.ToList();
            var alunos = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Contains("Aluno"))
                {
                    alunos.Add(user);
                }
            }

            return View(alunos);
        }

        [Authorize(Roles = "Personal")]
        [HttpGet("personal/usuario/ficha-de-aluno")]
        public async Task<IActionResult> ExibirFichaAluno(string id)
        {
            var aluno = await userManager.FindByIdAsync(id);
            if (aluno == null)
            {
                TempData["Error"] = "Aluno não encontrado.";
                return RedirectToAction("VerAlunos", "Personal");
            }

            if (aluno.FichasAluno != null && aluno.FichasAluno.Any())
            {
                var fichaAluno = aluno.FichasAluno.First();
                return View("ExibirFichaAluno", fichaAluno);
            }

            // Se a ficha não existir, redirecionar para criar nova ficha
            TempData["Info"] = "Nenhuma ficha encontrada.";
            return RedirectToAction("VerAlunos", "Personal");
        }


        [HttpGet]
        [Authorize(Roles = "Personal")]
        public async Task<IActionResult> VerFichasTreino(string id)
        {
            Console.WriteLine($"ID recebido: {id}");

            var aluno = await userManager.FindByIdAsync(id);
            if (aluno == null)
            {
                Console.WriteLine("Usuário não encontrado. Redirecionando para a Home.");
                return RedirectToAction("VerAlunos", "Personal");
            }

            Console.WriteLine($"Aluno encontrado: {aluno.UserName}");
            var fichaTreino = aluno.FichasTreino;
            return View(fichaTreino);
        }

        [HttpGet]
        [Authorize(Roles = "Personal")]
        public async Task<IActionResult> CriarFichaTreino(string id)
        {
            var aluno = await userManager.FindByIdAsync(id);
            if (aluno == null)
            {
                return RedirectToAction("VerAlunos", "Personal"); // Redireciona se o aluno não for encontrado
            }

            // Verifica se o aluno já possui 3 fichas de treino
            if (aluno.FichasTreino.Count >= 3)
            {
                TempData["ErrorMessage"] = "O aluno já possui o limite de 3 fichas de treino.";
                return RedirectToAction("VerAlunos", "Personal");
            }

            // Cria uma nova ficha de treino vazia
            var fichaTreino = new FichaTreino();

            return View(fichaTreino); // Retorna a view para o personal preencher a ficha
        }


        [HttpPost]
        [Authorize(Roles = "Personal")]
        public async Task<IActionResult> CriarFichaTreino(FichaTreino model, string id)
        {
            try
            {
                // Validação e processamento do modelo
                if (ModelState.IsValid)
                {
                    // Verificar se o aluno existe
                    var aluno = await userManager.FindByIdAsync(id);
                    if (aluno == null) return NotFound("Aluno não encontrado.");

                    // Criar a ficha de treino
                    var fichaTreino = new FichaTreino
                    {
                        Id = Guid.NewGuid(),
                        UsuarioId = id,
                        NomeTreino = model.NomeTreino,
                        Descricao = model.Descricao,
                        DataInicio = DateTime.Now,
                        DiasDeTreino = model.DiasDeTreino,  // Aqui os dias e exercícios são mapeados corretamente
                        Observacoes = model.Observacoes
                    };

                    // Adicionar a ficha ao aluno
                    aluno.FichasTreino.Add(fichaTreino);
                    await userManager.UpdateAsync(aluno);

                    return RedirectToAction("VerAlunos", "Personal");
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


        [HttpGet]
        [Authorize(Roles = "Personal")]
        public async Task<IActionResult> DetalhesFichaTreino(string id, string fichaId)
        {
            // Encontre o usuário pelo ID
            var aluno = await userManager.FindByIdAsync(id);

            if (aluno == null)
            {
                // Se o usuário não for encontrado, redireciona para a página inicial
                return RedirectToAction("VerAlunos", "Personal");
            }

            // Convertendo fichaId de string para Guid
            if (!Guid.TryParse(fichaId, out var fichaGuid))
            {
                // Se não conseguir converter o ID da ficha, redireciona para a lista de fichas
                return RedirectToAction("VerFichasTreino", new { id = id });
            }

            // Encontre a ficha de treino dentro da lista de FichasTreino do aluno
            var fichaTreino = aluno.FichasTreino.FirstOrDefault(f => f.Id == fichaGuid);

            if (fichaTreino == null)
            {
                // Se a ficha não for encontrada, redireciona para a lista de fichas
                return RedirectToAction("VerFichasTreino", new { id = id });
            }

            // Passa a ficha de treino para a view
            return View(fichaTreino);
        }


        [HttpPost]
        [Authorize(Roles = "Personal")]
        public async Task<IActionResult> DeletarFichaTreino(string fichaId, string usuarioId)
        {
            // Verifica se o ID da ficha foi fornecido
            if (string.IsNullOrEmpty(fichaId) || string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToAction("VerFichasTreino", new { id = usuarioId });
            }

            // Encontre o usuário pelo ID
            var aluno = await userManager.FindByIdAsync(usuarioId);
            if (aluno == null)
            {
                return RedirectToAction("VerAlunos", "Personal");
            }

            // Encontre a ficha de treino dentro da lista de FichasTreino do aluno
            var fichaTreino = aluno.FichasTreino.FirstOrDefault(f => f.Id.ToString() == fichaId);

            if (fichaTreino == null)
            {
                // Se a ficha não for encontrada, redireciona para a lista de fichas
                return RedirectToAction("VerFichasTreino", new { id = usuarioId });
            }

            // Remove a ficha de treino
            aluno.FichasTreino.Remove(fichaTreino);

            // Salve as alterações no banco de dados
            var result = await userManager.UpdateAsync(aluno);
            if (result.Succeeded)
            {
                // Redireciona para a página de visualização de fichas
                return RedirectToAction("VerFichasTreino", new { id = usuarioId });
            }

            // Se algo deu errado, mostra uma mensagem de erro
            ModelState.AddModelError("", "Erro ao excluir a ficha de treino.");
            return RedirectToAction("VerFichasTreino", new { id = usuarioId });
        }
    }
}
