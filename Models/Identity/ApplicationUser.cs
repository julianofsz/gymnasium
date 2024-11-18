using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;
using gymnasium_academia.Models.Treinos;
using gymnasium_academia.Models.ViewModels;

namespace gymnasium_academia.Models.Identity
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public string NomeCompleto { get; set; }

        public string Telefone { get; set; }

        public string Cpf { get; set; }

        public DateTime DataNascimento { get; set; }

        public DateTime DataCadastro { get; set; }

        public bool Assinante { get; set; }

        public List<FichaTreino> FichasTreino { get; set; } = new List<FichaTreino>();

        public TipoUsuario Tipo { get; set; }

        public int Idade => DateTime.Today.Year - DataNascimento.Year - (DateTime.Today < DataNascimento.AddYears(DateTime.Today.Year - DataNascimento.Year) ? 1 : 0);

    }
}
