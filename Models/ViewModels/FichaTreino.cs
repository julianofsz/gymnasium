using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gymnasium_academia.Models.ViewModels
{
    public class FichaTreino
    {
        public Guid Id { get; set; }

        public string? UsuarioId { get; set; }

        [Required(ErrorMessage = "Nome do treino é obrigatório.")]
        public string NomeTreino { get; set; }

        [Required(ErrorMessage = "Descrição do treino é obrigatória.")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória.")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "É necessário preencher os exercícios para cada dia da semana.")]
        public List<DiaTreino> DiasDeTreino { get; set; } = new();

        public string? Observacoes { get; set; }

        // Validação personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DiasDeTreino == null || DiasDeTreino.Count == 0)
            {
                yield return new ValidationResult("É necessário incluir pelo menos um dia de treino.", new[] { nameof(DiasDeTreino) });
            }

            foreach (var dia in DiasDeTreino)
            {
                if (dia.Exercicios == null || dia.Exercicios.Count == 0)
                {
                    yield return new ValidationResult($"O dia {dia.Dia} deve conter pelo menos um exercício.", new[] { nameof(DiasDeTreino) });
                }
            }
        }
    }


    public class Exercicio
    {
        [Required(ErrorMessage = "Nome do exercício é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Número de repetições é obrigatório.")]
        public int Repeticoes { get; set; }

        [Required(ErrorMessage = "Número de séries é obrigatório.")]
        public int Series { get; set; }

        [Required(ErrorMessage = "Grupo muscular é obrigatório.")]
        public string GrupoMuscular { get; set; } // Exemplo: Peitoral, Costas, Pernas
    }

    public class DiaTreino
    {
        public string Dia { get; set; }
        public List<Exercicio> Exercicios { get; set; } = new();
    }

}
