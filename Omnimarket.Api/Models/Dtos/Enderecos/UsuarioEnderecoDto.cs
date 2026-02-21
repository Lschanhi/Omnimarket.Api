using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models.Dtos.Enderecos
{
    public class UsuarioEnderecoDto
    {
        public int Id { get; set; }
        public TiposLogradouroBR TipoLogradouro { get; set; }
        public string NomeEndereco { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Cep { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public bool IsPrincipal { get; set; }
    }
}