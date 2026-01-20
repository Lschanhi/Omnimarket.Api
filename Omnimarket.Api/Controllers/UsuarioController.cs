using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens; // Necessário para assinar o token
using Omnimarket.Api.Utils; 



namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
         private readonly DataContext _context;

        private readonly IConfiguration _configuration;

        public UsuarioController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private async Task<bool> UsuarioExistente(string nomeUsuario)
        {
            if (await _context.TBL_USUARIO.AnyAsync(x => x.NomeUsuario.ToLower() == nomeUsuario.ToLower()))
            {
                return true;
            }
            return false;
        }

    [HttpPost("Registrar")]
    public async Task<IActionResult> RegistrarUsuario(Usuario user)
    {
        try
        {
            // 1. Validação básica
            if (await UsuarioExistente(user.NomeUsuario))
                return BadRequest("Nome de usuário já existe"); // Melhor retornar BadRequest direto do que lançar Exception

            if (string.IsNullOrEmpty(user.PasswordString))
                return BadRequest("A senha é obrigatória.");

            // 2. Criação do Hash e Salt (Descomentei e declarei as variáveis aqui)
            Criptografia.CriarPasswordHash(user.PasswordString, out byte[] hash, out byte[] salt);

            // 3. Atribuição dos valores
            user.PasswordString = string.Empty; // Limpa a senha em texto puro por segurança
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            
            // Defina outros valores padrão se necessário (ex: DataCadastro)
            // user.DataCadastro = DateTime.Now; 

            // 4. Persistência
            await _context.TBL_USUARIO.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok(user.Id);
        }
        catch (System.Exception ex)
        {
            // Dica: Em produção, evite retornar 'ex.InnerException' diretamente para não expor detalhes do banco
            return BadRequest($"Erro ao registrar: {ex.Message}");
        }
}

        
    }
}