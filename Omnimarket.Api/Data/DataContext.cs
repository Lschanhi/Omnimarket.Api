using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Models;

namespace Omnimarket.Api.Data
{
    public class DataContext: DbContext
    {
         public DataContext (DbContextOptions<DataContext> options) : base(options)
        {
            
        }
        public DbSet<Usuario> TBL_USUARIO { get; set; }
        public DbSet<Endereco> TBL_ENDERECO { get; set; }
        public DbSet<Telefone> TBL_TELEFONE { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("TBL_USUARIO");
            modelBuilder.Entity<Endereco>().ToTable("TBL_ENDERECO");
            modelBuilder.Entity<Telefone>().ToTable("TBL_TELEFONE");
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Telefones)
                .WithOne(t => t.Usuario)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Enderecos)
                .WithOne(e => e.Usuario)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices úteis (opcional)
            modelBuilder.Entity<Usuario>().HasIndex(x => x.Cpf).IsUnique();
            modelBuilder.Entity<Usuario>().HasIndex(x => x.Email).IsUnique();
            modelBuilder.Entity<Usuario>().HasIndex(x => x.NomeUsuario).IsUnique();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveColumnType("varchar").HaveMaxLength(200);
        }
        
    }
}