using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Enum;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId:guid}/enderecos")]
    public class EnderecosController : ControllerBase
    {
        private readonly DataContext _context;

        public EnderecosController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(Guid usuarioId)
        {
            var enderecos = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId)
                .Select(e => new
                {
                    e.Id,
                    e.TipoLogradouro,
                    e.NomeLogradouro,
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

        [HttpGet("{enderecoId:guid}")]
        public async Task<IActionResult> Obter(Guid usuarioId, Guid enderecoId)
        {
            var endereco = await _context.TBL_ENDERECO
                .Where(e => e.UsuarioId == usuarioId && e.Id == enderecoId)
                .Select(e => new
                {
                    e.Id,
                    e.TipoLogradouro,
                    e.NomeLogradouro,
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
        public async Task<IActionResult> Criar(Guid usuarioId, [FromBody] UsuarioEnderecoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == usuarioId);
            if (!usuarioExiste) return NotFound(new { mensagem = "Usuário não encontrado." });

            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                TipoLogradouro = dto.TipoLogradouro.Trim(),
                NomeLogradouro = dto.NomeLogradouro.Trim(),
                Numero = dto.Numero.Trim(),
                Complemento = dto.Complemento?.Trim(),
                Cep = dto.Cep.Trim(),
                Cidade = dto.Cidade.Trim(),
                Uf = dto.Uf.Trim(),
                IsPrincipal = dto.IsPrincipal ?? false
            };

            await _context.TBL_ENDERECO.AddAsync(endereco);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Obter), new { usuarioId, enderecoId = endereco.Id }, new { endereco.Id });
        }

        [HttpPut("{enderecoId:guid}")]
        public async Task<IActionResult> Atualizar(Guid usuarioId, Guid enderecoId, [FromBody] UsuarioEnderecoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var endereco = await _context.TBL_ENDERECO
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && e.Id == enderecoId);

            if (endereco is null) return NotFound();

            endereco.TipoLogradouro = dto.TipoLogradouro.Trim();
            endereco.NomeLogradouro = dto.NomeLogradouro.Trim();
            endereco.Numero = dto.Numero.Trim();
            endereco.Complemento = dto.Complemento?.Trim();
            endereco.Cep = dto.Cep.Trim();
            endereco.Cidade = dto.Cidade.Trim();
            endereco.Uf = dto.Uf.Trim();
            endereco.IsPrincipal = dto.IsPrincipal ?? endereco.IsPrincipal;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{enderecoId:guid}")]
        public async Task<IActionResult> Remover(Guid usuarioId, Guid enderecoId)
        {
            var endereco = await _context.TBL_ENDERECO
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId && e.Id == enderecoId);

            if (endereco is null) return NotFound();

            _context.TBL_ENDERECO.Remove(endereco);
            await _context.SaveChangesAsync();

            return NoContent();
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
