using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
namespace gymnasium_academia.Models.Treinos

{
    public class FichaTreino
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public Guid UsuarioId { get; set; }

        [Required] // Torna esse campo obrigatório
        [StringLength(50)] // Limita o tamanho
        public string Nome { get; set; }

        [StringLength(300)]
        public string Descricao { get; set; }

        public List<Exercicio> Exercicios { get; set; } = new List<Exercicio>();

        public enum Categoria { resistencia, forca, hipertrofia }

        [Required]
        public int Series { get; set; }

        [Required]
        [StringLength(5)]
        public string Repeticoes { get; set; }

        public bool IsPublica { get; set; }
    }
}

