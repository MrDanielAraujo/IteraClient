namespace IteraClient.Models;

/// <summary>
/// Modelo de requisição para adicionar um documento ao banco de dados.
/// </summary>
public class AddDocumentRequest
{
    /// <summary>
    /// Nome do arquivo (ex: "balanco-2024.pdf").
    /// </summary>
    /// <example>balanco-2024.pdf</example>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo do arquivo codificado em Base64.
    /// </summary>
    /// <example>JVBERi0xLjQKJeLjz9MK...</example>
    public string ContentBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Tipo MIME do arquivo (padrão: application/pdf).
    /// </summary>
    /// <example>application/pdf</example>
    public string? ContentType { get; set; }

    /// <summary>
    /// CNPJ da empresa associada ao documento (14 dígitos, apenas números).
    /// </summary>
    /// <example>34413970000130</example>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Sistema ou origem do documento.
    /// </summary>
    /// <example>Sistema Contábil</example>
    public string? Source { get; set; }

    /// <summary>
    /// Descrição do documento.
    /// </summary>
    /// <example>Balanço Patrimonial 2024</example>
    public string? Description { get; set; }
}