using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Omnimarket.Api.Models.Entidades
{
    public class Produto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public decimal Preco { get; set; }
        public bool Disponivel { get; set; }

        [StringLength(100)]
        public string Descricao { get; set; }
        public byte[]? Foto { get; set; }
        public int QtdProdutos { get; set; }
        
        //[Required]
        public double MediaAvaliacao { get; set; }  //Media de avaliações que um produto tem (0,2; 4,8, 5)
        public int Avaliacao { get; set; }  //avaliação individual que o cliente pode dar (0 - 5)
        public DateTime DtCriacao { get; set; } = DateTime.UtcNow; //Quando o vendedor criou o produto no sistema(DateTime.UtcNow serve para não ter problema com fuso horário)
        public string Comentarios { get; set; }
    }
}