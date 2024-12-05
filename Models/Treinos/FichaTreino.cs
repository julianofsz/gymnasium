using gymnasium_academia.Models.Treinos;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace gymnasium_academia.Models.Treinos;

public class FichaTreino
{
    [BsonId]
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    [Required]
    [StringLength(50)]
    public string? Nome { get; set; }

    [StringLength(300)]
    public string? Descricao { get; set; }

    public List<Exercicio> Exercicios { get; set; } = new List<Exercicio>();

    public CategoriaTreino Categoria { get; set; }

    public List<DayOfWeek> DiasTreino { get; set; } = new List<DayOfWeek>();

    public bool IsPublica { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }
}

public enum CategoriaTreino
{
    Resistencia,
    Forca,
    Hipertrofia
}
