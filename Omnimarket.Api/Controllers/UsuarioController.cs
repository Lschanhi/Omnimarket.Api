using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Services;
using Omnimarket.Api.Utils;
using Omnimarket.Api.Models.Enum;


namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ICpfService _cpfService;

        public UsuarioController(DataContext context, ICpfService cpfService)
        {
            _context = context;
            _cpfService = cpfService;
        }

        private async Task<bool> UsuarioExistente(string nomeUsuario) =>
            await _context.TBL_USUARIO.AnyAsync(x => x.NomeUsuario.ToLower() == nomeUsuario.ToLower());

        private async Task<bool> EmailExistente(string email) =>
            await _context.TBL_USUARIO.AnyAsync(x => x.Email.ToLower() == email.ToLower());

        private async Task<bool> CpfExistente(string cpf)
        {
            string cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
            return await _context.TBL_USUARIO.AnyAsync(x => x.Cpf == cpfLimpo);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Telefones)
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario is null) return NotFound();

            return Ok(new
            {
                usuario.Id,
                usuario.Cpf,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.NomeUsuario,
                usuario.Email,
                Telefones = usuario.Telefones.Select(t => new { t.Id, t.Numero, t.Tipo, t.IsPrincipal }),
                Enderecos = usuario.Enderecos.Select(e => new { e.Id, e.TipoLogradouro,e.NomeLogradouro, e.Numero, e.Cep, e.Cidade, e.Uf, e.IsPrincipal })
            });
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroComContatoDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var erros = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { mensagem = "Dados inválidos", erros });
                }

                if (userDto.Telefones is null || userDto.Telefones.Count < 1)
                    return BadRequest(new { mensagem = "Informe pelo menos 1 telefone." });

                if (!CpfValidador.ValidarCpf(userDto.Cpf))
                    return BadRequest(new { mensagem = "CPF inválido." });

                var dadosReceita = await _cpfService.ConsultarCpf(userDto.Cpf);
                if (!dadosReceita.Sucesso)
                    return BadRequest("Não foi possível validar o CPF na Receita Federal. Tente novamente.");

                // (opcional) anti-fraude que você fez
                string nomeCompletoUsuario = $"{userDto.Nome} {userDto.Sobrenome}";
                if (!StringExtensions.NomeCompativel(nomeCompletoUsuario, dadosReceita.Nome))
                {
                    return BadRequest(new
                    {
                        mensagem = "Divergência de titularidade.",
                        detalhes = $"O nome informado não confere com o CPF. O nome registrado na Receita Federal começa com: {EsconderNome(dadosReceita.Nome)}"
                    });
                }

                // Normalização
                string cpfLimpo = userDto.Cpf.Replace(".", "").Replace("-", "").Trim();

                if (await CpfExistente(cpfLimpo))
                    return BadRequest(new { mensagem = "CPF já cadastrado." });

                if (await UsuarioExistente(userDto.NomeUsuario))
                    return BadRequest(new { mensagem = "Nome de usuário já existe." });

                if (await EmailExistente(userDto.Email))
                    return BadRequest(new { mensagem = "Email já cadastrado." });

                Criptografia.CriarPasswordHash(userDto.Password, out byte[] hash, out byte[] salt);

                var novoUsuario = new Usuario
                {
                    Id = Guid.NewGuid(),
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

                // Telefones
                for (int i = 0; i < userDto.Telefones.Count; i++)
                {
                    var t = userDto.Telefones[i];

                    novoUsuario.Telefones.Add(new Telefone
                    {
                        Id = Guid.NewGuid(),
                        Numero = (t.Numero ?? "").Trim(),
                        Tipo = string.IsNullOrWhiteSpace(t.Tipo) ? "Celular" : t.Tipo.Trim(),
                        IsPrincipal = t.IsPrincipal ?? (i == 0)
                    });
                }

                // Endereços (opcional)
                if (userDto.Enderecos is not null)
                {
                    foreach (var e in userDto.Enderecos)
                    {
                        novoUsuario.Enderecos.Add(new Endereco
                        {
                            Id = Guid.NewGuid(),
                            TipoLogradouro = (e.TipoLogradouro ?? "").Trim(),
                            NomeLogradouro = (e.NomeLogradouro ?? "").Trim(),
                            Numero = (e.Numero ?? "").Trim(),
                            Complemento = e.Complemento?.Trim(),
                            Cep = (e.Cep ?? "").Trim(),
                            Cidade = (e.Cidade ?? "").Trim(),
                            Uf = (e.Uf ?? "").Trim(),
                            IsPrincipal = e.IsPrincipal ?? false
                        });
                    }
                }

                await _context.TBL_USUARIO.AddAsync(novoUsuario);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensagem = "Usuário registrado com sucesso!",
                    usuario = new
                    {
                        id = novoUsuario.Id,
                        cpf = novoUsuario.Cpf,
                        nomeUsuario = novoUsuario.NomeUsuario,
                        email = novoUsuario.Email,
                        nomeCompleto = $"{novoUsuario.Nome} {novoUsuario.Sobrenome}"
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao salvar no banco de dados.",
                    detalhes = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
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

        [HttpPost("{id:guid}/telefones")]
        public async Task<IActionResult> AdicionarTelefone(Guid id, [FromBody] UsuarioTelefoneDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == id);
            if (!usuarioExiste) return NotFound();

            var telefone = new Telefone
            {
                Id = Guid.NewGuid(),
                UsuarioId = id,
                Numero = (dto.Numero ?? "").Trim(),
                Tipo = string.IsNullOrWhiteSpace(dto.Tipo) ? "Celular" : dto.Tipo.Trim(),
                IsPrincipal = dto.IsPrincipal ?? false
            };

            await _context.TBL_TELEFONE.AddAsync(telefone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id }, new { telefone.Id });
        }

        [HttpPost("{id:guid}/enderecos")]
        public async Task<IActionResult> AdicionarEndereco(Guid id, [FromBody] UsuarioEnderecoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == id);
            if (!usuarioExiste) return NotFound();

            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                UsuarioId = id,
                TipoLogradouro = (dto.TipoLogradouro ?? "").Trim(),
                NomeLogradouro = (dto.NomeLogradouro ?? "").Trim(),
                Numero = (dto.Numero ?? "").Trim(),
                Complemento = dto.Complemento?.Trim(),
                Cep = (dto.Cep ?? "").Trim(),
                Cidade = (dto.Cidade ?? "").Trim(),
                Uf = (dto.Uf ?? "").Trim(),
                IsPrincipal = dto.IsPrincipal ?? false
            };

            await _context.TBL_ENDERECO.AddAsync(endereco);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id }, new { endereco.Id });
        }

        [HttpGet("tipos-logradouro")]
        public IActionResult GetTiposLogradouro()
        {
            var itens = TiposLogradouroBR.Itens
                .Select(x => new { codigo = x.Codigo, descricao = x.Descricao })
                .ToList();

            return Ok(itens);
        }

    }
}
