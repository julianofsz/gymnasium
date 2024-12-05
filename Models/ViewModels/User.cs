using System.ComponentModel.DataAnnotations;

namespace gymnasium_academia.Models.ViewModels
{
    public class User
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        public string? NomeCompleto { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirmação de senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "CPF é obrigatório.")]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "Formato de CPF inválido.")]
        public string? Cpf { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório.")]
        [RegularExpression(@"^\(?\d{2}\)?\s?\d{4,5}-\d{4}$", ErrorMessage = "Formato de telefone inválido.")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "Data de nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public bool Assinante { get; set; } = false;

        public TipoUsuario Tipo { get; set; }

        public int Idade => DateTime.Today.Year - DataNascimento.Year - (DateTime.Today < DataNascimento.AddYears(DateTime.Today.Year - DataNascimento.Year) ? 1 : 0);

    }

    public enum TipoUsuario
    {
        Usuario = 0,
        Aluno = 1,
        Personal = 3,
        Admin = 4
    }
}
