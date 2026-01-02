using IteraClient.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IteraClient.Data;

/// <summary>
/// Contexto do banco de dados da aplicação.
/// Configurado para usar banco em memória, mas preparado para migração para PostgreSQL.
/// </summary>
public class IteraDbContext : DbContext
{
    public IteraDbContext(DbContextOptions<IteraDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Documentos armazenados no sistema.
    /// </summary>
    public DbSet<Document> Documents { get; set; } = null!;

    /// <summary>
    /// Resultados exportados da Itera.
    /// </summary>
    public DbSet<DocumentExportResult> DocumentExportResults { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Cnpj);
            entity.HasIndex(e => e.IteraDocumentId);
            entity.HasIndex(e => e.IsProcessed);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
        });

        // Configuração da entidade DocumentExportResult
        modelBuilder.Entity<DocumentExportResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.Cnpj);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.HasOne(e => e.Document)
                .WithMany()
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
