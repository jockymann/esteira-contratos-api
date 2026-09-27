using Microsoft.EntityFrameworkCore;
using EsteiraAluguel.Domain.Entities;

namespace EsteiraAluguel.Infrastructure.Data
{
    /// <summary>
    /// Contexto do banco de dados configurado para PostgreSQL.
    /// </summary>
    public class EsteiraDbContext : DbContext
    {
        public DbSet<Imovel> Imoveis { get; set; }
        public DbSet<Proposta> Propostas { get; set; }

        public EsteiraDbContext(DbContextOptions<EsteiraDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapeamento do Imóvel e configuração de concorrência (Race Condition)
            modelBuilder.Entity<Imovel>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // O PostgreSQL usa uma coluna oculta chamada 'xmin' para controle de transação.
                // Ao mapearmos a 'Versao' para essa coluna com IsRowVersion(), o EF Core lançará
                // uma exceção se duas requisições tentarem alterar o mesmo imóvel ao mesmo tempo.
                entity.Property(e => e.Versao)
                      .IsRowVersion()
                      .HasColumnName("xmin")
                      .HasColumnType("xid");
            });

            // Mapeamento da Proposta e relacionamento
            modelBuilder.Entity<Proposta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Imovel)
                      .WithMany()
                      .HasForeignKey(e => e.ImovelId);
            });
        }
    }
}