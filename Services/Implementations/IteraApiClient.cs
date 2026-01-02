using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using IteraClient.Configuration;
using IteraClient.Interfaces;
using IteraClient.Models;
using Microsoft.Extensions.Options;

namespace IteraClient.Services.Implementations;

/// <summary>
/// Implementação do cliente da API Itera.
/// Segue os princípios SOLID:
/// - SRP: Apenas realiza chamadas à API Itera
/// - OCP: Pode ser estendido através da interface IIteraApiClient
/// - DIP: Depende de abstrações (IAuthorizedHttpClientFactory, IOptions)
/// </summary>
public class IteraApiClient(
    IAuthorizedHttpClientFactory httpClientFactory,
    IOptions<IteraSettings> settings)
    : IIteraApiClient
{
    private readonly IAuthorizedHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly IteraSettings _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

    /// <inheritdoc />
    public async Task<IteraStatusResponse> GetStatusAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);
        
        var url = string.Format(_settings.Endpoints.Status, documentId);
        
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<IteraStatusResponse>(content) ?? new IteraStatusResponse();
    }

    /// <inheritdoc />
    public async Task<List<ControladoraResponse>> GetExportJsonAsync(long cnpj, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);
        
        var url = string.Format(_settings.Endpoints.ExportJson, cnpj);
        
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonNode = JsonNode.Parse(content);
        
        if (jsonNode is not JsonObject listasContainer)
        {
            return new List<ControladoraResponse>();
        }
        
        var listaControladora = new List<ControladoraResponse>();
        
        foreach (var prop in listasContainer)
        {
            if (prop.Value is JsonArray jsonArray)
            {
                var items = JsonSerializer.Deserialize<List<ControladoraResponse>>(jsonArray.ToJsonString());
                if (items != null)
                {
                    listaControladora.AddRange(items);
                }
            }
        }
        
        return listaControladora;
    }

    /// <inheritdoc />
    public async Task<string> GetDeParaAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);
        
        var url = string.Format(_settings.Endpoints.DePara, documentId);
        
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UploadDocumentResponse> UploadDocumentAsync(
        IFormFile file,
        string? source,
        string? description,
        string? cnpj,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("O arquivo é obrigatório e não pode estar vazio.", nameof(file));
        }

        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);
        
        using var formData = new MultipartFormDataContent();
        
        // Adiciona o arquivo - copia para MemoryStream para evitar problemas com stream fechado
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        
        var streamContent = new StreamContent(memoryStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        formData.Add(streamContent, "file", file.FileName);
        
        // Adiciona os campos de texto
        formData.Add(new StringContent(source ?? string.Empty), "source");
        formData.Add(new StringContent(description ?? string.Empty), "description");
        formData.Add(new StringContent(cnpj ?? string.Empty), "cnpj");
        
        var response = await httpClient.PostAsync(_settings.Endpoints.UploadDoc, formData, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Trata resposta vazia ou não-JSON
        if (string.IsNullOrWhiteSpace(content))
        {
            return new UploadDocumentResponse
            {
                Status = "Success",
                Message = "Upload realizado com sucesso",
                FileName = file.FileName,
                Cnpj = cnpj ?? string.Empty
            };
        }
        
        try
        {
            return JsonSerializer.Deserialize<UploadDocumentResponse>(content) ?? new UploadDocumentResponse
            {
                Status = "Success",
                Message = content,
                FileName = file.FileName,
                Cnpj = cnpj ?? string.Empty
            };
        }
        catch (JsonException)
        {
            // Se a resposta não for JSON válido, retorna com a mensagem raw
            return new UploadDocumentResponse
            {
                Status = "Success",
                Message = content,
                FileName = file.FileName,
                Cnpj = cnpj ?? string.Empty
            };
        }
    }
}
