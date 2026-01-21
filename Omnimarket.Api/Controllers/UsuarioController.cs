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
using Omnimarket.Api.Services;



namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICpfService _cpfService;


        public UsuarioController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
            public UsuarioController(DataContext context, ICpfService cpfService)
        {
            _context = context;
            _cpfService = cpfService;
        }


        private async Task<bool> UsuarioExistente(string nomeUsuario)
        {
            return await _context.TBL_USUARIO
                .AnyAsync(x => x.NomeUsuario.ToLower() == nomeUsuario.ToLower());
        }

        private async Task<bool> EmailExistente(string email)
        {
            return await _context.TBL_USUARIO
                .AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        private async Task<bool> CpfExistente(string cpf)
        {
            string cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
            return await _context.TBL_USUARIO
                .AnyAsync(x => x.Cpf == cpfLimpo);
        }


        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarUsuario(UsuarioRegistroDto userDto)
        {
            try
            {
                // 1. Validação do ModelState
                if (!ModelState.IsValid)
                {
                    var erros = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new { 
                        mensagem = "Dados inválidos", 
                        erros = erros 
                    });
                }

                // 2. Validação de CPF
                if (!CpfValidador.ValidarCpf(userDto.Cpf))
                {
                    return BadRequest(new { mensagem = "CPF inválido." });
                }

                var dadosReceita = await _cpfService.ConsultarCpf(userDto.Cpf);
                
                if (!dadosReceita.Sucesso)
                {
                    // Se a API cair, você decide: Bloqueia ou deixa passar com aviso?
                    // Para segurança total, bloqueia.
                    return BadRequest("Não foi possível validar o CPF na Receita Federal. Tente novamente.");
                }

                // 2,1. VERIFICAÇÃO ANTI-FRAUDE (A Lógica que você pediu)
                // Compara o nome que o usuário digitou (userDto.Nome + Sobrenome) com o da API
                string nomeCompletoUsuario = $"{userDto.Nome} {userDto.Sobrenome}";
                
                if (!StringExtensions.NomeCompativel(nomeCompletoUsuario, dadosReceita.Nome))
                {
                    // Retorna erro vago por segurança, ou específico para ajudar o usuário honesto
                    return BadRequest(new { 
                        mensagem = "Divergência de titularidade.", 
                        detalhes = $"O nome informado não confere com o CPF. O nome registrado na Receita Federal começa com: {EsconderNome(dadosReceita.Nome)}" 
                    });
                }

                // 2,2. Opcional: Atualizar o nome no seu banco com o oficial da Receita
                // Isso garante que no seu banco os dados estejam 100% corretos
                userDto.Nome = dadosReceita.Nome; // Ou separar em Nome/Sobrenome

                // 3. Limpa e verifica CPF duplicado
                string cpfLimpo = userDto.Cpf.Replace(".", "").Replace("-", "").Trim();
                if (await CpfExistente(cpfLimpo))
                {
                    return BadRequest(new { mensagem = "CPF já cadastrado." });
                }

                // 4. Verifica nome de usuário duplicado
                if (await UsuarioExistente(userDto.NomeUsuario))
                {
                    return BadRequest(new { mensagem = "Nome de usuário já existe." });
                }

                // 5. Verifica email duplicado
                if (await EmailExistente(userDto.Email))
                {
                    return BadRequest(new { mensagem = "Email já cadastrado." });
                }

                // 6. Criação do Hash e Salt da senha
                Criptografia.CriarPasswordHash(
                    userDto.Password, 
                    out byte[] hash, 
                    out byte[] salt
                );

                // 7. Cria o novo usuário
                var novoUsuario = new Usuario
                {
                    Cpf = cpfLimpo,
                    Nome = userDto.Nome.Trim(),
                    Sobrenome = userDto.Sobrenome.Trim(),
                    NomeUsuario = userDto.NomeUsuario.Trim(),
                    Email = userDto.Email.ToLower().Trim(),
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    DataCadastro = DateTime.Now,
                    DataAcesso = null
                };

                // 8. Persiste no banco de dados
                await _context.TBL_USUARIO.AddAsync(novoUsuario);
                await _context.SaveChangesAsync();

                // 9. Retorna sucesso (sem expor dados sensíveis)
                return Ok(new
                {
                    mensagem = "Usuário registrado com sucesso!",
                    usuario = new
                    {
                        cpf = novoUsuario.Cpf,
                        nomeUsuario = novoUsuario.NomeUsuario,
                        email = novoUsuario.Email,
                        nomeCompleto = $"{novoUsuario.Nome} {novoUsuario.Sobrenome}"
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { 
                    mensagem = "Erro ao salvar no banco de dados.",
                    detalhes = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    mensagem = "Erro interno ao registrar usuário.",
                    detalhes = ex.Message
                });
            }
        }
        private string EsconderNome(string nome)
        {
            if (string.IsNullOrEmpty(nome) || nome.Length < 3) return "***";
            return nome.Substring(0, 3) + "***";
        }

        [HttpGet("VerificarDisponibilidade/{nomeUsuario}")]
        public async Task<IActionResult> VerificarDisponibilidadeUsuario(string nomeUsuario)
        {
            bool existe = await UsuarioExistente(nomeUsuario);
            return Ok(new { disponivel = !existe });
        }
    

        
    }
}