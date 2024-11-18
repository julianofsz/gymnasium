using System.ComponentModel.DataAnnotations;

namespace gymnasium_academia.Models.ViewModels

{
    public class UserUpdate
    {
        public string NomeCompleto { get; set; }

        public string Email { get; set; }

        [RegularExpression(@"^\(?\d{2}\)?\s?\d{4,5}-\d{4}$", ErrorMessage = "Formato de telefone inválido.")]
        public string Telefone { get; set; }

        public string FotoPerfil { get; set; } // Caminho ou URL da foto

        // Campos para mudança de senha
        [DataType(DataType.Password)]
        public string SenhaAtual { get; set; }

        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmarNovaSenha { get; set; }
    }
}
