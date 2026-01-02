using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IteraClient.Data.Entities;

/// <summary>
/// Entidade que representa um documento armazenado no banco de dados.
/// </summary>
[Table("Documents")]
public class Document
{
    /// <summary>
    /// Identificador único do documento.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do arquivo.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo do arquivo em Base64.
    /// </summary>
    [Required]
    public string ContentBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Tipo MIME do arquivo.
    /// </summary>
    [MaxLength(100)]
    public string ContentType { get; set; } = "application/pdf";

    /// <summary>
    /// CNPJ associado ao documento.
    /// </summary>
    [Required]
    [MaxLength(14)]
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Origem do documento.
    /// </summary>
    [MaxLength(200)]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do documento.
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ID retornado pela Itera após o upload.
    /// </summary>
    public Guid? IteraDocumentId { get; set; }

    /// <summary>
    /// Status do processamento na Itera.
    /// </summary>
    [MaxLength(50)]
    public string? IteraStatus { get; set; }

    /// <summary>
    /// Indica se o documento foi processado com sucesso.
    /// </summary>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Mensagem de erro, se houver.
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }
}
