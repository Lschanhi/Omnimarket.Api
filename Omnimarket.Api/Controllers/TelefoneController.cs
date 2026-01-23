using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Data;
using Omnimarket.Api.Models;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId:guid}/telefones")]
    public class TelefonesController : ControllerBase
    {
        private readonly DataContext _context;

        public TelefonesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(Guid usuarioId)
        {
            var telefones = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId)
                .Select(t => new
                {
                    t.Id,
                    t.Ddd,
                    t.Numero,
                    t.Tipo,
                    t.IsPrincipal,
                    t.E164
                })
                .ToListAsync();

            return Ok(telefones);
        }

        [HttpGet("{telefoneId:guid}")]
        public async Task<IActionResult> Obter(Guid usuarioId, Guid telefoneId)
        {
            var telefone = await _context.TBL_TELEFONE
                .Where(t => t.UsuarioId == usuarioId && t.Id == telefoneId)
                .Select(t => new
                {
                    t.Id,
                    t.Ddd,
                    t.Numero,
                    t.Tipo,
                    t.IsPrincipal,
                    t.E164
                })
                .FirstOrDefaultAsync();

            return telefone is null ? NotFound() : Ok(telefone);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(Guid usuarioId, [FromBody] UsuarioTelefoneDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExiste = await _context.TBL_USUARIO.AnyAsync(u => u.Id == usuarioId);
            if (!usuarioExiste) return NotFound(new { mensagem = "Usuário não encontrado." });

            var telefone = new Telefone
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Ddd = dto.Ddd.Trim(),
                Numero = dto.Numero.Trim(),
                Tipo = string.IsNullOrWhiteSpace(dto.Tipo) ? "Celular" : dto.Tipo.Trim(),
                IsPrincipal = dto.IsPrincipal ?? false
                
            };

            await _context.TBL_TELEFONE.AddAsync(telefone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Obter), new { usuarioId, telefoneId = telefone.Id }, new { 
            telefone.Id,
            telefone.Ddd,
            telefone.Numero,
            telefone.Tipo,
            telefone.IsPrincipal,
            e164 = telefone.E164 });
        }

        [HttpPut("{telefoneId:guid}")]
        public async Task<IActionResult> Atualizar(Guid usuarioId, Guid telefoneId, [FromBody] UsuarioTelefoneDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null) return NotFound();

            telefone.Ddd = dto.Ddd.Trim();
            telefone.Numero = dto.Numero.Trim();
            telefone.Tipo = string.IsNullOrWhiteSpace(dto.Tipo) ? telefone.Tipo : dto.Tipo.Trim();
            telefone.IsPrincipal = dto.IsPrincipal ?? telefone.IsPrincipal;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{telefoneId:guid}")]
        public async Task<IActionResult> Remover(Guid usuarioId, Guid telefoneId)
        {
            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null) return NotFound();

            // Regra: não pode ficar com 0 telefones
            var totalTelefones = await _context.TBL_TELEFONE.CountAsync(t => t.UsuarioId == usuarioId);
            if (totalTelefones <= 1)
                return BadRequest(new { mensagem = "Não é possível remover o último telefone do usuário." });

            _context.TBL_TELEFONE.Remove(telefone);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Opcional: marcar como principal (facilita pro front)
        [HttpPatch("{telefoneId:guid}/principal")]
        public async Task<IActionResult> DefinirPrincipal(Guid usuarioId, Guid telefoneId)
        {
            var telefone = await _context.TBL_TELEFONE
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Id == telefoneId);

            if (telefone is null) return NotFound();

            // Desmarca todos e marca este
            var telefones = await _context.TBL_TELEFONE.Where(t => t.UsuarioId == usuarioId).ToListAsync();
            foreach (var t in telefones) t.IsPrincipal = (t.Id == telefoneId);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
