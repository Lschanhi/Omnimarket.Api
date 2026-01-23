using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Omnimarket.Api.Models
{
    public class UsuarioRegistroDto
    {
        [Required(ErrorMessage = "CPF é obrigatório")]
        [StringLength(14, ErrorMessage = "CPF deve ter no máximo 14 caracteres")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sobrenome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Sobrenome deve ter entre 2 e 100 caracteres")]
        public string Sobrenome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Nome de usuário deve ter entre 3 e 50 caracteres")]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UsuarioRegistroComContatoDto : UsuarioRegistroDto
    {
        [Required(ErrorMessage = "Informe pelo menos 1 telefone.")]
        [MinLength(1, ErrorMessage = "Informe pelo menos 1 telefone.")]
        public List<UsuarioTelefoneDto> Telefones { get; set; } = new();

        public List<UsuarioEnderecoDto>? Enderecos { get; set; }
    }

    public class UsuarioTelefoneDto
    {
        [Required(ErrorMessage = "DDD é obrigatório.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "DDD deve ter 2 dígitos.")]
        public string Ddd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número é obrigatório.")]
        [StringLength(9, MinimumLength = 8, ErrorMessage = "Número deve ter 8 (fixo) ou 9 dígitos (celular).")]
        public string Numero { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Tipo { get; set; }

        public bool? IsPrincipal { get; set; }
    }

    public class UsuarioEnderecoDto
    {
        [Required(ErrorMessage = "Tipo de logradouro é obrigatório.")]
        [StringLength(10)]
        public string TipoLogradouro { get; set; } = string.Empty; // "AV", "R", ...

        [Required(ErrorMessage = "Nome do logradouro é obrigatório.")]
        [StringLength(200)]
        public string NomeLogradouro { get; set; } = string.Empty; // "Paulista"

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Complemento { get; set; }

        [Required, StringLength(10)]
        public string Cep { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Cidade { get; set; } = string.Empty;

        [Required, StringLength(2)]
        public string Uf { get; set; } = string.Empty;

        public bool? IsPrincipal { get; set; }
    }
}