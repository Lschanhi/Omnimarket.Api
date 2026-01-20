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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("TBL_USUARIO");
            /*
            modelBuilder.Entity<Usuario>().HasData(
                
            );
            */
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveColumnType("varchar").HaveMaxLength(200);
        }
        
    }
}