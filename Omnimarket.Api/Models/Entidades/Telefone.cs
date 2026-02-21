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

      public int Id { get; private set; }

      [Required]
      public int UsuarioId { get; private set; }

      public Usuario Usuario { get; private set; } = null!;

      [Required]
      public short Ddd { get; private set; } // 11, 21, ... [web:61]

      [Required, StringLength(20)]
      public string NumeroE164 { get; private set; } = string.Empty; // +55119XXXXXXXX [web:61]

      public bool IsPrincipal { get; private set; } = false;

      private Telefone() { } // EF

      public Telefone(short ddd, string numeroE164, bool isPrincipal)
      {
         Atualizar(ddd, numeroE164);
         IsPrincipal = isPrincipal;
      }

      public void Atualizar(short ddd, string numeroE164)
      {
         if (ddd <= 0) throw new ArgumentException("DDD inválido.");
         if (string.IsNullOrWhiteSpace(numeroE164)) throw new ArgumentException("Telefone inválido.");

         Ddd = ddd;
         NumeroE164 = numeroE164;
      }

      public void MarcarPrincipal() => IsPrincipal = true;
      public void DesmarcarPrincipal() => IsPrincipal = false;

   }

}