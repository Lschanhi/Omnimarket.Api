using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models
{
    public class Usuarios
    {
        public int Id { get; set; }
        public String CPF { get; set; } = string.Empty;
        public String Nome { get; set; } = string.Empty;
        public String Sobrnome { get; set; } = string.Empty;

        public String NomeUsuario { get; set; } =string.Empty;
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public DateTime? DataAcesso { get; set; }
       // [NotMapped] // using System.ComponentModel.DataAnnotations.Schema
        public string PasswordString { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;

        //[NotMapped]
        public string Token { get; set; } = string.Empty;
    }
}