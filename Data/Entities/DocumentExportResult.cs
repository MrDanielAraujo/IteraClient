using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IteraClient.Data.Entities;

/// <summary>
/// Entidade que representa os resultados exportados da Itera.
/// </summary>
[Table("DocumentExportResults")]
public class DocumentExportResult
{
    /// <summary>
    /// Identificador único do registro.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID do documento relacionado.
    /// </summary>
    [Required]
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Documento relacionado.
    /// </summary>
    [ForeignKey(nameof(DocumentId))]
    public virtual Document? Document { get; set; }

    /// <summary>
    /// Código do item.
    /// </summary>
    [MaxLength(100)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Tempo de processamento.
    /// </summary>
    [MaxLength(50)]
    public string Tempo { get; set; } = string.Empty;

    /// <summary>
    /// Confiança do resultado.
    /// </summary>
    [MaxLength(50)]
    public string Conf { get; set; } = string.Empty;

    /// <summary>
    /// Data do documento.
    /// </summary>
    [MaxLength(50)]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Ano da data.
    /// </summary>
    [MaxLength(50)]
    public string DataAno { get; set; } = string.Empty;

    /// <summary>
    /// Termo total.
    /// </summary>
    [MaxLength(500)]
    public string TermoTotal { get; set; } = string.Empty;

    /// <summary>
    /// Valor do item.
    /// </summary>
    [MaxLength(100)]
    public string Valor { get; set; } = string.Empty;

    /// <summary>
    /// Página onde foi encontrado.
    /// </summary>
    [MaxLength(20)]
    public string Page { get; set; } = string.Empty;

    /// <summary>
    /// ID do item na Itera.
    /// </summary>
    [MaxLength(100)]
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Subseção do documento.
    /// </summary>
    [MaxLength(200)]
    public string Subsection { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do item.
    /// </summary>
    [MaxLength(100)]
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Seção do documento.
    /// </summary>
    [MaxLength(200)]
    public string Section { get; set; } = string.Empty;

    /// <summary>
    /// Moeda utilizada.
    /// </summary>
    [MaxLength(20)]
    public string Moeda { get; set; } = string.Empty;

    /// <summary>
    /// Escala dos valores.
    /// </summary>
    [MaxLength(50)]
    public string Escala { get; set; } = string.Empty;

    /// <summary>
    /// Empresa relacionada.
    /// </summary>
    [MaxLength(500)]
    public string Empresa { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ da empresa.
    /// </summary>
    [MaxLength(20)]
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Indica se é consolidado.
    /// </summary>
    [MaxLength(20)]
    public string Consolidado { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de balanço.
    /// </summary>
    [MaxLength(100)]
    public string TipoBalanco { get; set; } = string.Empty;

    /// <summary>
    /// Coordenada Y.
    /// </summary>
    [MaxLength(20)]
    public string Y { get; set; } = string.Empty;

    /// <summary>
    /// Coordenada X.
    /// </summary>
    [MaxLength(20)]
    public string X { get; set; } = string.Empty;

    /// <summary>
    /// ID único do item.
    /// </summary>
    [MaxLength(100)]
    public string UniqueId { get; set; } = string.Empty;

    /// <summary>
    /// ID do item pai.
    /// </summary>
    [MaxLength(100)]
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// Indica se é total.
    /// </summary>
    [MaxLength(10)]
    public string IsTotal { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
