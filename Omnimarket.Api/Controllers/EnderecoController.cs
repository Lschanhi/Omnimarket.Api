using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Enum;
using Omnimarket.Api.Utils;
using Omnimarket.Api.Models.Dtos.Enderecos;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId:int}/enderecos")]
    public class EnderecosController : ControllerBase
    {
        private readonly DataContext _context;
        public EnderecosController(DataContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Listar(int usuarioId)
        {
            var enderecos = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId)
                .Select(e => new
                {
                    e.Id,
                    e.TipoLogradouro,
                    e.NomeEndereco,
                    e.Numero,
                    e.Complemento,
                    e.Cep,
                    e.Cidade,
                    e.Uf,
                    e.IsPrincipal
                })
                .ToListAsync();

            return Ok(enderecos);
        }

        [HttpGet("{enderecoId:int}")]
        public async Task<IActionResult> Obter(int usuarioId, int enderecoId)
        {
            var endereco = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId && e.Id == enderecoId)
                .Select(e => new
                {
                    e.Id,
                    e.TipoLogradouro,
                    e.NomeEndereco,
                    e.Numero,
                    e.Complemento,
                    e.Cep,
                    e.Cidade,
                    e.Uf,
                    e.IsPrincipal
                })
                .FirstOrDefaultAsync();

            return endereco is null ? NotFound() : Ok(endereco);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(int usuarioId, [FromBody] UsuarioEnderecoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound(new { mensagem = "Usuário não encontrado." });

            var novo = new Endereco(
                usuarioId: usuarioId,
                cep: dto.Cep.Trim(),
                tipoLogradouro: dto.TipoLogradouro,
                nomeEndereco: dto.NomeEndereco.Trim(),
                numero: dto.Numero.Trim(),
                complemento: dto.Complemento?.Trim(),
                cidade: dto.Cidade.Trim(),
                uf: dto.Uf.Trim(),
                isPrincipal: false
            );

            usuario.AdicionarEndereco(novo, dto.IsPrincipal == true);

            await _context.SaveChangesAsync();

            var criado = usuario.Enderecos.OrderByDescending(e => e.Id).First();
            return CreatedAtAction(nameof(Obter),
                new { usuarioId, enderecoId = criado.Id },
                new { criado.Id });
        }

        [HttpPut("{enderecoId:int}")]
        public async Task<IActionResult> Atualizar(int usuarioId, int enderecoId, [FromBody] UsuarioEnderecoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound();

            try
            {
                usuario.AtualizarEndereco(enderecoId,
                    alterar: e => e.Atualizar(
                        cep: dto.Cep.Trim(),
                        tipoLogradouro: dto.TipoLogradouro,
                        nomeEndereco: dto.NomeEndereco.Trim(),
                        numero: dto.Numero.Trim(),
                        complemento: dto.Complemento?.Trim(),
                        cidade: dto.Cidade.Trim(),
                        uf: dto.Uf.Trim()
                    ),
                    tornarPrincipal: dto.IsPrincipal
                );
            }
            catch (KeyNotFoundException) { return NotFound(); }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{enderecoId:int}")]
        public async Task<IActionResult> Remover(int usuarioId, int enderecoId)
        {
            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound();

            try
            {
                usuario.RemoverEndereco(enderecoId);
            }
            catch (KeyNotFoundException) { return NotFound(); }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{enderecoId:int}/principal")]
        public async Task<IActionResult> DefinirPrincipal(int usuarioId, int enderecoId)
        {
            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound();

            try
            {
                usuario.DefinirEnderecoPrincipal(enderecoId);
            }
            catch (KeyNotFoundException) { return NotFound(); }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("tipos-logradouro")]
        public IActionResult GetTiposLogradouro()
        {
            var itens = Enum.GetValues<TiposLogradouroBR>()
                .Select(v => new { codigo = v.ToString(), descricao = v.GetDisplayName() })
                .ToList();

            return Ok(itens);
        }
    }
}
