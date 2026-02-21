using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Models
{
    public class Endereco
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        public int UsuarioId { get; private set; }

        public Usuario Usuario { get; private set; } = null!;

        [Required, StringLength(200)]
        public TiposLogradouroBR TipoLogradouro { get; private set; }

        [Required, StringLength(200)]
        public string NomeEndereco { get; private set; } = string.Empty;

        [Required, StringLength(20)]
        public string Numero { get; private set; } = string.Empty;

        [StringLength(80)]
        public string? Complemento { get; private set; }

        [Required, StringLength(10)]
        public string Cep { get; private set; } = string.Empty;

        [Required, StringLength(120)]
        public string Cidade { get; private set; } = string.Empty;

        [Required, StringLength(2)]
        public string Uf { get; private set; } = string.Empty;

        public bool IsPrincipal { get; private set; } = false;

        private Endereco() { } // EF

        public Endereco(
            int usuarioId,
            string cep,
            TiposLogradouroBR tipoLogradouro,
            string nomeEndereco,
            string numero,
            string? complemento,
            string cidade,
            string uf,
            bool isPrincipal)
        {
            UsuarioId = usuarioId;
            Atualizar(cep, tipoLogradouro, nomeEndereco, numero, complemento, cidade, uf);
            IsPrincipal = isPrincipal;
        }

        public void Atualizar(string cep, TiposLogradouroBR tipoLogradouro, string nomeEndereco, string numero,
            string? complemento, string cidade, string uf)
        {
            Cep = cep;
            TipoLogradouro = tipoLogradouro;
            NomeEndereco = nomeEndereco;
            Numero = numero;
            Complemento = complemento;
            Cidade = cidade;
            Uf = uf;
        }

        public void MarcarPrincipal() => IsPrincipal = true;
        public void DesmarcarPrincipal() => IsPrincipal = false;
        public void DefinirPrincipal(bool v) => IsPrincipal = v;
    }
}