using gymnasium_academia.Models.Identity;
using gymnasium_academia.Models.Treinos;
using gymnasium_academia.Models.ViewModels;
using gymnasium_academia.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace gymnasium_academia.Controllers
{
    [Authorize]
    public class FichaTreinoController : Controller
    {
        private readonly IMongoCollection<FichaTreino> _fichasCollection;
        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private UserManager<ApplicationUser> userManager;

        public FichaTreinoController(IMongoDatabase mongoDatabase, UserManager<ApplicationUser> userManager)
        {
            _fichasCollection = mongoDatabase.GetCollection<FichaTreino>("FichasTreino");
            _usersCollection = mongoDatabase.GetCollection<ApplicationUser>("AspNetUsers");
            this.userManager = userManager;
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private bool IsUserAdmin() => User.IsInRole("Admin");
        private bool IsUserPersonal() => User.IsInRole("Personal");
        private bool IsUserAluno() => User.IsInRole("Aluno");

        public async Task<IActionResult> Index()
        {
            var fichas = await _fichasCollection
                .Find(f => f.IsPublica || f.UsuarioId == GetCurrentUserId())
                .ToListAsync();

            return View(fichas);
        }

        [Authorize(Roles = "Admin, Aluno, Personal")]
        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Criar(FichaTreino ficha, string alunoEmail = null)
        {
            if (!ModelState.IsValid)
                return View(ficha);

            ficha.DataCriacao = DateTime.UtcNow;

            if (IsUserAluno())
            {
                ficha.UsuarioId = GetCurrentUserId();
                ficha.IsPublica = false;
            }
            else if (IsUserAdmin() || IsUserPersonal())
            {
                if (!string.IsNullOrEmpty(alunoEmail))
                {
                    var aluno = await _usersCollection.Find(u => u.Email == alunoEmail).FirstOrDefaultAsync();
                    if (aluno == null) return BadRequest("Aluno não encontrado.");
                    ficha.UsuarioId = aluno.Id;
                }
                ficha.IsPublica = IsUserAdmin() ? ficha.IsPublica : false;
            }

            await _fichasCollection.InsertOneAsync(ficha);

            // Usando TempData para passar a mensagem de sucesso
            TempData["SuccessMessage"] = "Ficha de treino criada com sucesso!";

            // Redirecionando de volta para a mesma página
            return RedirectToAction("Criar");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var ficha = await _fichasCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (ficha == null || (!ficha.IsPublica && ficha.UsuarioId != GetCurrentUserId()))
                return NotFound();

            return View(ficha);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, FichaTreino fichaAtualizada)
        {
            if (!ModelState.IsValid) return View(fichaAtualizada);

            var ficha = await _fichasCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (ficha == null || (!IsUserAdmin() && ficha.UsuarioId != GetCurrentUserId()))
                return NotFound();

            ficha.Nome = fichaAtualizada.Nome;
            ficha.Descricao = fichaAtualizada.Descricao;
            ficha.DiasTreino = fichaAtualizada.DiasTreino;
            ficha.Exercicios = fichaAtualizada.Exercicios;
            ficha.Categoria = fichaAtualizada.Categoria;
            ficha.DataAtualizacao = DateTime.UtcNow;

            await _fichasCollection.ReplaceOneAsync(f => f.Id == id, ficha);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var ficha = await _fichasCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (ficha == null || (!IsUserAdmin() && ficha.UsuarioId != GetCurrentUserId()))
                return NotFound();

            await _fichasCollection.DeleteOneAsync(f => f.Id == id);
            return RedirectToAction(nameof(Index));
        }

    }
}
