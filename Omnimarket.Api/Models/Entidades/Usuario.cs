using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Omnimarket.Api.Models
{
    public class Usuario
    {
        public int Id { get; private set; }

        [Required, StringLength(11)]
        public string Cpf { get; private set; } = string.Empty;

        [Required, StringLength(100)]
        public string Nome { get; private set; } = string.Empty;

        [Required, StringLength(100)]
        public string Sobrenome { get; private set; } = string.Empty;

        public byte[]? PasswordHash { get; private set; }
        public byte[]? PasswordSalt { get; private set; }

        public byte[]? Foto { get; private set; }
        public DateTime? DataAcesso { get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.Now;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; private set; } = string.Empty;

        [NotMapped]
        public string Token { get; set; } = string.Empty;

        [NotMapped]
        public string PasswordString { get; set; } = string.Empty;

        // Backing fields (encapsulação)
        private readonly List<Telefone> _telefones = new();
        public IReadOnlyCollection<Telefone> Telefones => _telefones;

        private readonly List<Endereco> _enderecos = new();
        public IReadOnlyCollection<Endereco> Enderecos => _enderecos;

        private Usuario() { } // EF Core

        public Usuario(string cpf, string nome, string sobrenome, string email, byte[] hash, byte[] salt)
        {
            Cpf = cpf;
            Nome = nome;
            Sobrenome = sobrenome;
            Email = email;
            PasswordHash = hash;
            PasswordSalt = salt;
            DataCadastro = DateTime.Now;
            DataAcesso = null;
        }

        // ---- Telefones (regras do domínio) ----
        public void AdicionarTelefone(string numeroE164, short ddd, bool tornarPrincipal)
        {
            if (string.IsNullOrWhiteSpace(numeroE164)) throw new ArgumentException("Telefone inválido.");
            if (ddd <= 0) throw new ArgumentException("DDD inválido.");

            if (tornarPrincipal)
                foreach (var t in _telefones) t.DesmarcarPrincipal();

            var isPrincipal = tornarPrincipal || _telefones.Count == 0;

            _telefones.Add(new Telefone(ddd, numeroE164, isPrincipal));
        }

        public Telefone ObterTelefone(int telefoneId)
            => _telefones.FirstOrDefault(t => t.Id == telefoneId)
               ?? throw new KeyNotFoundException("Telefone não encontrado.");

        public void AtualizarTelefone(int telefoneId, string numeroE164, short ddd, bool? tornarPrincipal)
        {
            var tel = ObterTelefone(telefoneId);

            if (tornarPrincipal == true)
                foreach (var t in _telefones) t.DesmarcarPrincipal();

            tel.Atualizar(ddd, numeroE164);

            if (tornarPrincipal.HasValue)
            {
                if (tornarPrincipal.Value) tel.MarcarPrincipal();
                // se false, mantém possível “nenhum principal”? aqui vamos impedir:
                // se desmarcar o único principal, escolhe outro principal
                else tel.DesmarcarPrincipal();
            }

            GarantirPrincipal();
        }

        public void RemoverTelefone(int telefoneId)
        {
            if (_telefones.Count <= 1)
                throw new InvalidOperationException("Não é possível remover o último telefone do usuário.");

            var tel = ObterTelefone(telefoneId);
            var eraPrincipal = tel.IsPrincipal;

            _telefones.Remove(tel);

            if (eraPrincipal)
                GarantirPrincipal();
        }

        public void DefinirTelefonePrincipal(int telefoneId)
        {
            var tel = ObterTelefone(telefoneId);
            foreach (var t in _telefones) t.DesmarcarPrincipal();
            tel.MarcarPrincipal();
        }

        private void GarantirPrincipal()
        {
            if (_telefones.Count == 0) return;
            if (_telefones.Any(t => t.IsPrincipal)) return;
            _telefones[0].MarcarPrincipal();
        }

        // ---- Endereços (mesma lógica de principal) ----
        public void AdicionarEndereco(Endereco novo, bool tornarPrincipal)
        {
            if (tornarPrincipal)
                foreach (var e in _enderecos) e.DesmarcarPrincipal();

            var isPrincipal = tornarPrincipal || _enderecos.Count == 0;
            novo.DefinirPrincipal(isPrincipal);

            _enderecos.Add(novo);
        }

        public Endereco ObterEndereco(int enderecoId)
            => _enderecos.FirstOrDefault(e => e.Id == enderecoId)
               ?? throw new KeyNotFoundException("Endereço não encontrado.");

        public void AtualizarEndereco(int enderecoId, Action<Endereco> alterar, bool? tornarPrincipal)
        {
            var end = ObterEndereco(enderecoId);

            if (tornarPrincipal == true)
                foreach (var e in _enderecos) e.DesmarcarPrincipal();

            alterar(end);

            if (tornarPrincipal == true) end.MarcarPrincipal();
            if (tornarPrincipal == false) end.DesmarcarPrincipal();

            GarantirEnderecoPrincipal();
        }

        public void RemoverEndereco(int enderecoId)
        {
            var end = ObterEndereco(enderecoId);
            var eraPrincipal = end.IsPrincipal;

            _enderecos.Remove(end);

            if (eraPrincipal)
                GarantirEnderecoPrincipal();
        }

        public void DefinirEnderecoPrincipal(int enderecoId)
        {
            var end = ObterEndereco(enderecoId);
            foreach (var e in _enderecos) e.DesmarcarPrincipal();
            end.MarcarPrincipal();
        }

        private void GarantirEnderecoPrincipal()
        {
            if (_enderecos.Count == 0) return;
            if (_enderecos.Any(e => e.IsPrincipal)) return;
            _enderecos[0].MarcarPrincipal();
        }
    }
}