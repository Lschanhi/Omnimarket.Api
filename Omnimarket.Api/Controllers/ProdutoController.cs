using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Omnimarket.Api.Controllers
{
    [ApiController]
    [Route("api/produtos")]    //rota da api para a pagina dos produtos()
    public class ProdutoController : ControllerBase
    {
        private readonly DataContext _context;

        public ProdutoController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]   //método para mostrar todos os produtos do site
        public async Task<IActionResult> GetProduto()
        {
            var produto = await _context.Produto
            .Where(p => p.Disponivel)
            .ToListAsync();

            return Ok(produto);
        }

        [HttpGet("{id:int}")]   //para buscar um produto especifico
        public async Task<IActionResult> GetProduto(int id)
        {
            var produto = await _context.Produto
            .FirstOrDefaultAsync(p => p.Id == id && p.Disponivel);

            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        [HttpPost]  //postar um produto e dar um http 201
        public async Task<IActionResult> CriarProduto(Produto produto)
        {
            _context.Produto.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduto), new{id = produto.Id}, produto);
        }

        [HttpPut("{id:int}")]   //alterar infos do produtos
        public async Task<IActionResult> AtualizarProduto(int id, Produto produto)
        {
            if(id != produto.Id)
                return BadRequest();

            var produtoBanco = await _context.Produto.FindAsync(id);
            
            if(produtoBanco == null)
                return NotFound();

            produtoBanco.Nome = produto.Nome;
            produtoBanco.Preco = produto.Preco;
            produtoBanco.Descricao = produto.Descricao;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("desativar/{id}")]     //para indisponibilizar um produto para os clientes ao invés de deleta-lo
        public async Task<IActionResult> DesativarProduto(int id)
        {
            var produto = await _context.Produto.FindAsync(id);

            if(produto == null)
                return NotFound();

            produto.Disponivel = false;

            await _context.SaveChangesAsync();

            return Ok("Produto marcado como indisponível para compra.");
        }

        [HttpPut("disponibilizar/{id}")]    //para disponibilizar dnv
        public async Task<IActionResult> Disponibilizar(int id)
        {
            var produto = await _context.Produto.FindAsync(id);

            if (produto == null)
                return NotFound();

            produto.Disponivel = true;

            await _context.SaveChangesAsync();

            return Ok("Produto reativado.");
        }

        //Combinamos q n iriamos permitir q produtos fossem deletados
        /*[HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletarProduto(int id)
        {
            var produto = await _context.Produto.FindAsync(id);

            if (produto == null)
                return NotFound();

            _context.Produto.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }*/
    }
}