using Microsoft.AspNetCore.Http;

namespace IteraClient.Models;

/// <summary>
/// Modelo de requisição para upload de documento.
/// </summary>
public class UploadDocumentRequest
{
    /// <summary>
    /// Arquivo a ser enviado.
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Origem do documento.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do documento.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ da empresa.
    /// </summary>
    public string Cnpj { get; set; } = string.Empty;
}
