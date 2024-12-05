using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace gymnasium_academia.Models.ViewModels

{
    public class UserUpdate
    {
        public string Id { get; set; }

        [Display(Name = "Nome completo (opcional)")]
        public string? NomeCompleto { get; set; }

        [EmailAddress(ErrorMessage = "Insira um email válido.")]
        public string? Email { get; set; }

        [RegularExpression(@"^\(?\d{2}\)?\s?\d{4,5}-\d{4}$", ErrorMessage = "Formato de telefone inválido.")]
        [Phone(ErrorMessage = "Insira um número de telefone válido.")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "Confirmação da senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string? SenhaAtual { get; set; }

        [DataType(DataType.Password)]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "As senhas não coincidem.")]
        public string? ConfirmarNovaSenha { get; set; }

        public TipoUsuario Tipo { get; set; }

        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

    }
}
