using System.Text.Json.Serialization;

namespace IteraClient.Models;

/// <summary>
/// Modelo de resposta do upload de documento.
/// </summary>
public class UploadDocumentResponse
{
    /// <summary>
    /// ID único do documento criado.
    /// </summary>
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem de retorno.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Status do upload.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Nome do arquivo enviado.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ associado ao documento.
    /// </summary>
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = string.Empty;
}
