using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Omnimarket.Api.Models.Dtos.Enderecos;
using Omnimarket.Api.Models.Dtos.Telefones;


namespace Omnimarket.Api.Models.Dtos.Usuarios
{
    public class UsuarioRegistroDto
    {
        [Required(ErrorMessage = "CPF é obrigatório")]
        [StringLength(14, ErrorMessage = "CPF deve ter no máximo 14 caracteres")]
        public string Cpf { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 2)]
        public string Sobrenome { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UsuarioRegistroComContatoDto : UsuarioRegistroDto
    {
        [Required, MinLength(1, ErrorMessage = "Informe pelo menos 1 telefone.")]
        public List<UsuarioTelefoneCriacaoDto> Telefones { get; set; } = new();

        public List<UsuarioEnderecoCriacaoDto>? Enderecos { get; set; }
    }


}