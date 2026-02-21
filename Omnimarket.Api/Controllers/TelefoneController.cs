using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;
using Omnimarket.Api.Models.Dtos.Telefones;
using Omnimarket.Api.Utils;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId:int}/telefones")]
    public class TelefonesController : ControllerBase
    {
        private readonly DataContext _context;
        public TelefonesController(DataContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Listar(int usuarioId)
        {
            var telefones = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId)
                .Select(t => new { t.Id, t.Ddd, numeroE164 = t.NumeroE164, t.IsPrincipal })
                .ToListAsync();

            return Ok(telefones);
        }

        [HttpGet("{telefoneId:int}")]
        public async Task<IActionResult> Obter(int usuarioId, int telefoneId)
        {
            var telefone = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId && t.Id == telefoneId)
                .Select(t => new { t.Id, t.Ddd, numeroE164 = t.NumeroE164, t.IsPrincipal })
                .FirstOrDefaultAsync();

            return telefone is null ? NotFound() : Ok(telefone);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(int usuarioId, [FromBody] UsuarioTelefoneDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Telefones)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound(new { mensagem = "Usuário não encontrado." });

            var r = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!r.Valido) return BadRequest(new { mensagem = "Telefone inválido (apenas celular BR)." });

            usuario.AdicionarTelefone(r.E164!, r.Ddd!.Value, dto.IsPrincipal == true);

            await _context.SaveChangesAsync();

            var criado = usuario.Telefones.OrderByDescending(t => t.Id).First();
            return CreatedAtAction(nameof(Obter),
                new { usuarioId, telefoneId = criado.Id },
                new { criado.Id, criado.Ddd, numeroE164 = criado.NumeroE164, criado.IsPrincipal });
        }

        [HttpPut("{telefoneId:int}")]
        public async Task<IActionResult> Atualizar(int usuarioId, int telefoneId, [FromBody] UsuarioTelefoneDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Telefones)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound();

            var r = ValidadorTelefone.ValidarCelularBr(dto.Ddd, dto.Numero);
            if (!r.Valido) return BadRequest(new { mensagem = "Telefone inválido (apenas celular BR)." });

            try
            {
                usuario.AtualizarTelefone(telefoneId, r.E164!, r.Ddd!.Value, dto.IsPrincipal);
            }
            catch (KeyNotFoundException) { return NotFound(); }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{telefoneId:int}")]
        public async Task<IActionResult> Remover(int usuarioId, int telefoneId)
        {
            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Telefones)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound();

            try
            {
                usuario.RemoverTelefone(telefoneId);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return BadRequest(new { mensagem = ex.Message }); }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{telefoneId:int}/principal")]
        public async Task<IActionResult> DefinirPrincipal(int usuarioId, int telefoneId)
        {
            var usuario = await _context.TBL_USUARIO
                .Include(u => u.Telefones)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario is null) return NotFound();

            try
            {
                usuario.DefinirTelefonePrincipal(telefoneId);
            }
            catch (KeyNotFoundException) { return NotFound(); }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
