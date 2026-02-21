using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Telefones
{
    public class UsuarioTelefoneCriacaoDto
    {
        [Required(ErrorMessage = "DDD é obrigatório.")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "DDD deve ter 2 dígitos.")]
        public string Ddd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número é obrigatório.")]
        [RegularExpression(@"^\d{8,9}$", ErrorMessage = "Número deve ter 8 ou 9 dígitos (somente números).")]
        public string Numero { get; set; } = string.Empty;

        public bool? IsPrincipal { get; set; }
    }
}