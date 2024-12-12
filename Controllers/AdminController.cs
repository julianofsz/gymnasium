using gymnasium_academia.Filters;
using gymnasium_academia.Models.Identity;
using gymnasium_academia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace gymnasium_academia.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private RoleManager<ApplicationRole> roleManager;
        private SignInManager<ApplicationUser> signInManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }

        public IActionResult Admin()
        {
            return View();
        }

        //Lista todos usuários
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/gerenciar-alunos")]
        public async Task<IActionResult> VerTodos()
        {
            var users = userManager.Users.ToList();
            var userRoles = new List<(ApplicationUser User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userRoles.Add((user, roles));
            }

            return View(userRoles);
        }


        //Atualizar permissões
        [HttpPost("admin/update-role/{userId}")]
        public async Task<IActionResult> UpdateRole(string userId, [FromBody] RoleUpdateModel model)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var roleExists = await roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                return BadRequest($"A role {model.Role} não existe.");
            }

            var currentRoles = await userManager.GetRolesAsync(user);

            // Remover todas as roles atuais
            var removeRolesResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRolesResult.Succeeded)
            {
                var errors = string.Join(", ", removeRolesResult.Errors.Select(e => e.Description));
                return BadRequest($"Erro ao remover roles: {errors}");
            }

            // Adicionar a nova role
            var addRoleResult = await userManager.AddToRoleAsync(user, model.Role);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                return BadRequest($"Erro ao adicionar role: {errors}");
            }

            return Ok("Role atualizada com sucesso!");
        }


        public class RoleUpdateModel
        {
            public string Role { get; set; }
        }


        //Buscador de usuários
        public IActionResult Search(string searchString)
        {
            var users = from u in userManager.Users
                        select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.NomeCompleto.Contains(searchString));
            }

            return View("Index", users.ToList()); // Retorna à view com os resultados filtrados
        }


        //Mostra todos detalhes do usuário
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/usuario/detalhes")]
        public async Task<IActionResult> Detalhes(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var usuario = await userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/criar-usuario")]
        public IActionResult CriarUsuario()
        {
            return View();
        }


        //Cadastra um novo usuário
        [HttpPost("admin/criar-usuario")]
        public async Task<IActionResult> CriarUsuario(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = userManager.Users.FirstOrDefault(u => u.Cpf == user.Cpf);
                    if (existingUser != null)
                    {
                        ViewBag.ErrorMessage = "Este CPF já está cadastrado.";
                        return View(user);
                    }

                    var appUser = new ApplicationUser
                    {
                        UserName = user.Email,
                        Email = user.Email,
                        NomeCompleto = user.NomeCompleto,
                        Cpf = user.Cpf,
                        PhoneNumber = user.Telefone,
                        DataNascimento = user.DataNascimento,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        Tipo = user.Tipo // Atribuindo o tipo selecionado na view
                    };

                    var result = await userManager.CreateAsync(appUser, user.Password);
                    if (result.Succeeded)
                    {
                        // Atribui a role com base no tipo selecionado
                        var addRoleResult = await userManager.AddToRoleAsync(appUser, user.Tipo.ToString());
                        if (!addRoleResult.Succeeded)
                        {
                            ViewBag.ErrorMessage = "Erro ao adicionar a role: " + string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                            return View(user);
                        }

                        ViewBag.SuccessMessage = "Usuário criado com sucesso!";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Erro ao criar usuário: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Erro inesperado: " + ex.Message;
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Dados inválidos.";
            }

            return View(user);
        }


        //Atualizar usuário
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/usuario/editar")]
        public async Task<IActionResult> Editar(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Usuário não encontrado.";
                return RedirectToAction("VerTodos", "Admin");
            }

            var model = new UserUpdate
            {
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                Telefone = user.PhoneNumber
            };
            ViewData["UserId"] = user.Id; // Envia o ID para a View
            return View(model);
        }


        [HttpPost("admin/usuario/editar")]
        public async Task<IActionResult> Editar(string id, UserUpdate model)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Usuário não encontrado.";
                return RedirectToAction("VerTodos", "Admin");
            }

            if (!string.IsNullOrWhiteSpace(model.NomeCompleto))
                user.NomeCompleto = model.NomeCompleto;

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
                user.Email = model.Email;
                user.UserName = model.Email;

            if (!string.IsNullOrWhiteSpace(model.Telefone))
                user.PhoneNumber = model.Telefone;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Dados atualizados com sucesso!";
                return RedirectToAction("VerTodos", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        //Método para excluir o usuário
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Excluir(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["Error"] = "Usuário não encontrado.";
                return RedirectToAction("VerTodos");
            }

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Usuário excluído com sucesso.";
            }
            else
            {
                TempData["Error"] = "Erro ao excluir usuário.";
            }
            return RedirectToAction("VerTodos");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/usuario/ficha-de-aluno")]
        public async Task<IActionResult> ExibirFichaAluno(string id)
        {
            var aluno = await userManager.FindByIdAsync(id);
            if (aluno == null)
            {
                TempData["Error"] = "Aluno não encontrado.";
                return RedirectToAction("Index", "Admin");
            }

            if (aluno.FichasAluno != null && aluno.FichasAluno.Any())
            {
                var fichaAluno = aluno.FichasAluno.First();
                return View("ExibirFichaAluno", fichaAluno);
            }

            // Se a ficha não existir, redirecionar para criar nova ficha
            TempData["Info"] = "Nenhuma ficha encontrada. Por favor, crie uma nova ficha.";
            return RedirectToAction("CriarFichaAluno", new { id = aluno.Id });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CriarFichaAluno(string id)
        {
            var usuario = await userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                // Lógica para lidar com usuário inexistente
                return NotFound("Usuário não encontrado.");
            }

            var model = new FichaAluno();
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CriarFichaAluno(FichaAluno fichaAluno, string id)
        {
            if (ModelState.IsValid)
            {
                var aluno = await userManager.FindByIdAsync(id);
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

                    var usuarioId = User.Identity.Name;
                    fichaAluno.UsuarioId = id;

                    // Adiciona a ficha ao usuário
                    aluno.FichasAluno.Add(fichaAluno);

                    // Salva o usuário com a nova ficha
                    var result = await userManager.UpdateAsync(aluno);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = "Ficha do aluno cadastrada com sucesso!";
                        return RedirectToAction("VerTodos", "Admin");
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

            return RedirectToAction("ExibirFichaAluno", "Admin");
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarFichaAluno(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID do usuário não foi fornecido.";
                return RedirectToAction("Index", "Home");
            }

            // Encontre o usuário pelo ID
            var aluno = await userManager.FindByIdAsync(id);
            if (aluno != null && aluno.FichasAluno != null && aluno.FichasAluno.Any())
            {
                // Como há apenas uma ficha por usuário, vamos pegar a primeira
                var fichaAluno = aluno.FichasAluno.First();
                return View(fichaAluno);  // Passa a ficha para a view de edição
            }

            TempData["Error"] = "Ficha do aluno não encontrada.";
            return RedirectToAction("VerTodos", "Admin");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarFichaAluno(FichaAluno fichaAluno, string id)
        {
            if (ModelState.IsValid)
            {
                var aluno = await userManager.FindByIdAsync(id);
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
                        return RedirectToAction("VerTodos", "Admin");
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerFichasTreino(string id)
        {
            Console.WriteLine($"ID recebido: {id}");

            var aluno = await userManager.FindByIdAsync(id);
            if (aluno == null)
            {
                Console.WriteLine("Usuário não encontrado. Redirecionando para a Home.");
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"Aluno encontrado: {aluno.UserName}");
            var fichaTreino = aluno.FichasTreino;
            return View(fichaTreino);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarFichaTreino(string id)
        {
            var aluno = await userManager.FindByIdAsync(id);
            if (aluno == null)
            {
                return RedirectToAction("VerTodos", "Admin"); // Redireciona se o aluno não for encontrado
            }

            // Verifica se o aluno já possui 3 fichas de treino
            if (aluno.FichasTreino.Count >= 3)
            {
                TempData["ErrorMessage"] = "O aluno já possui o limite de 3 fichas de treino.";
                return RedirectToAction("VerTodos", "Admin");
            }

            // Cria uma nova ficha de treino vazia
            var fichaTreino = new FichaTreino();

            return View(fichaTreino); // Retorna a view para o admin preencher a ficha
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarFichaTreino(FichaTreino model, string id)
        {
            try
            {
                Console.WriteLine("Iniciando a criação da ficha de treino.");

                // Verifique se o modelo é válido
                if (ModelState.IsValid)
                {
                    Console.WriteLine("Modelo válido, processando...");

                    // Verifique se o aluno já possui 3 fichas
                    var aluno = await userManager.FindByIdAsync(id);
                    if (aluno == null)
                    {
                        Console.WriteLine("Aluno não encontrado!");
                        return NotFound("Aluno não encontrado.");
                    }

                    Console.WriteLine($"Aluno encontrado: {aluno.UserName}");

                    if (aluno.FichasTreino.Count >= 3)
                    {
                        Console.WriteLine("O aluno já tem 3 fichas de treino.");
                        ModelState.AddModelError(string.Empty, "O aluno já tem 3 fichas de treino.");
                        return View(model);
                    }

                    // Validar dias e exercícios
                    var diasComExercicios = model.DiasDeTreino
                        .Where(d => d.Exercicios != null && d.Exercicios.Count > 0)
                        .ToList();

                    Console.WriteLine("Verificando dias e exercícios adicionados:");

                    foreach (var dia in model.DiasDeTreino)
                    {
                        Console.WriteLine($"Dia: {dia.Dia}, Exercícios: {dia.Exercicios?.Count ?? 0}");
                        if (dia.Exercicios != null)
                        {
                            foreach (var ex in dia.Exercicios)
                            {
                                Console.WriteLine($"  Exercício: {ex.Nome}, Séries: {ex.Series}, Repetições: {ex.Repeticoes}");
                            }
                        }
                    }

                    if (diasComExercicios.Count == 0)
                    {
                        Console.WriteLine("Nenhum exercício foi adicionado.");
                        ModelState.AddModelError(string.Empty, "A ficha deve ter pelo menos um exercício.");
                        return View(model);
                    }

                    // Criar a ficha de treino para o aluno
                    var fichaTreino = new FichaTreino
                    {
                        Id = Guid.NewGuid(),
                        UsuarioId = id,
                        NomeTreino = model.NomeTreino,
                        Descricao = model.Descricao,
                        DataInicio = DateTime.Now,
                        DiasDeTreino = diasComExercicios, // Garantir que apenas dias válidos sejam salvos
                        Observacoes = model.Observacoes
                    };

                    // Adicionar a ficha ao aluno
                    aluno.FichasTreino.Add(fichaTreino);
                    await userManager.UpdateAsync(aluno);
                    Console.WriteLine("Ficha de treino criada e salva com sucesso!");

                    return RedirectToAction("VerTodos", "Admin");
                }
                else
                {
                    Console.WriteLine("Modelo inválido. Verifique os erros abaixo:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"Erro: {error.ErrorMessage}");
                    }

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar ficha de treino: {ex.Message}");
                return StatusCode(500, "Erro interno ao criar a ficha de treino.");
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> DetalhesFichaTreino(string id, string fichaId)
        {
            // Encontre o usuário pelo ID
            var aluno = await userManager.FindByIdAsync(id);

            if (aluno == null)
            {
                // Se o usuário não for encontrado, redireciona para a página inicial
                return RedirectToAction("Index", "Home");
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
        [Authorize(Roles = "Admin")]
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
                return RedirectToAction("Index", "Home");
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
