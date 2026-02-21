using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Omnimarket.Api.Models.Dtos.Telefones
{
    public class UsuarioTelefoneDto
    {
        public int Id { get; set; }
        public short Ddd { get; set; }
        public string NumeroE164 { get; set; } = string.Empty; // canonical [web:86]
        public bool IsPrincipal { get; set; }
    }
}
