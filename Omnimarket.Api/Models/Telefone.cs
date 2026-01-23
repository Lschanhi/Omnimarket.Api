using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace Omnimarket.Api.Models
{
   public class Telefone
{
      public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required, StringLength(2)]
        public string Ddd { get; set; } = string.Empty;

        [Required, StringLength(9)]
        public string Numero { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Tipo { get; set; } = "Celular";

        public bool IsPrincipal { get; set; } = false;

        // Não mapear no banco (é calculado)
        [NotMapped]
        public string E164 => $"+55{Ddd}{Numero}";
}

}