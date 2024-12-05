using System.ComponentModel.DataAnnotations;

namespace gymnasium_academia.Models.Treinos
{
    public class Exercicio
    {
        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Required]
        public int Series { get; set; }

        [Required]
        [StringLength(5)]
        public string? Repeticoes { get; set; }

        [Required]
        public string? GrupoMuscular { get; set; }
    }
}