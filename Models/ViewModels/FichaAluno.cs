using System;
using System.ComponentModel.DataAnnotations;

namespace gymnasium_academia.Models.ViewModels
{
    public class FichaAluno
    {
        public Guid UsuarioId { get; set; } // Id do aluno

        [Required(ErrorMessage = "Endereço é obrigatório.")]
        public Endereco Endereco { get; set; }

        [Required(ErrorMessage = "Estado civil é obrigatório.")]
        public EstadoCivil EstadoCivil { get; set; }

        [Required(ErrorMessage = "Informar se possui anamnese é obrigatório.")]
        public bool TemAnamnese { get; set; }

        public enum Sexo { Outros, Feminino, Masculino }

        public DateTime DataNascimento { get; set; }

        public int Idade => DateTime.Now.Year - DataNascimento.Year - (DateTime.Now.DayOfYear < DataNascimento.DayOfYear ? 1 : 0);
    }

    // Estado Civil do Aluno
    public enum EstadoCivil
    {
        Solteiro,
        Casado,
        UniaoEstavel,
        Divorciado,
        Viúvo
    }

    // Classe para os dados de Endereço
    public class Endereco
    {
        [Required(ErrorMessage = "Logradouro é obrigatório.")]
        public string Logradouro { get; set; }

        [Required(ErrorMessage = "Número é obrigatório.")]
        public string Numero { get; set; }

        public string? Complemento { get; set; }

        [Required(ErrorMessage = "Bairro é obrigatório.")]
        public string Bairro { get; set; }

        [Required(ErrorMessage = "Cidade é obrigatória.")]
        public string Cidade { get; set; }

        [Required(ErrorMessage = "Estado é obrigatório.")]
        public string Estado { get; set; }

        [Required(ErrorMessage = "CEP é obrigatório.")]
        [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP inválido. O formato correto é 00000-000.")]
        public string Cep { get; set; }
    }
}
