using IteraClient.Interfaces;
using IteraClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace IteraClient.Controllers;

/// <summary>
/// Controller responsável pelos endpoints da API Itera.
/// Segue os princípios SOLID:
/// - SRP: Apenas gerencia requisições HTTP e delega para serviços
/// - DIP: Depende de abstrações (interfaces) injetadas via construtor
/// </summary>
[ApiController]
[Route("[controller]")]
public class IteraController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IIteraApiClient _iteraApiClient;

    public IteraController(
        ITokenService tokenService,
        IIteraApiClient iteraApiClient)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _iteraApiClient = iteraApiClient ?? throw new ArgumentNullException(nameof(iteraApiClient));
    }

    /// <summary>
    /// Obtém um token de acesso válido.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token de acesso</returns>
    [HttpGet("GetAccessToken")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccessToken(CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
        return Ok(token);
    }

    /// <summary>
    /// Obtém o status de um documento pelo ID.
    /// </summary>
    /// <param name="documentId">ID do documento (opcional, usa valor padrão se não informado)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status do documento</returns>
    [HttpGet("GetStatus")]
    [ProducesResponseType(typeof(IteraStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatus(
        [FromQuery] Guid? documentId,
        CancellationToken cancellationToken)
    {
        // Usa o ID padrão se não for fornecido (mantém compatibilidade com versão anterior)
        var id = documentId ?? Guid.Parse("490d3ec0-fe70-4e40-b286-09246f5e6d5d");
        var status = await _iteraApiClient.GetStatusAsync(id, cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Exporta dados em JSON pelo CNPJ.
    /// </summary>
    /// <param name="cnpj">CNPJ da empresa (opcional, usa valor padrão se não informado)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de dados da controladora</returns>
    [HttpGet("GetExport")]
    [ProducesResponseType(typeof(List<ControladoraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExport(
        [FromQuery] long? cnpj,
        CancellationToken cancellationToken)
    {
        // Usa o CNPJ padrão se não for fornecido (mantém compatibilidade com versão anterior)
        var cnpjValue = cnpj ?? 34413970000130;
        var export = await _iteraApiClient.GetExportJsonAsync(cnpjValue, cancellationToken);
        return Ok(export);
    }

    /// <summary>
    /// Obtém o mapeamento De-Para pelo ID do documento.
    /// </summary>
    /// <param name="documentId">ID do documento (opcional, usa valor padrão se não informado)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do mapeamento De-Para</returns>
    [HttpGet("GetDePara")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDePara(
        [FromQuery] Guid? documentId,
        CancellationToken cancellationToken)
    {
        // Usa o ID padrão se não for fornecido (mantém compatibilidade com versão anterior)
        var id = documentId ?? Guid.Parse("490d3ec0-fe70-4e40-b286-09246f5e6d5d");
        var dePara = await _iteraApiClient.GetDeParaAsync(id, cancellationToken);
        return Ok(dePara);
    }

    /// <summary>
    /// Realiza o upload de um documento para a API Itera.
    /// </summary>
    /// <param name="request">Dados do documento a ser enviado (form-data)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta do upload contendo informações do documento criado</returns>
    [HttpPost("UploadDocument")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadDocument(
        [FromForm] UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("O arquivo é obrigatório e não pode estar vazio.");
        }

        if (string.IsNullOrWhiteSpace(request.Cnpj))
        {
            return BadRequest("O CNPJ é obrigatório.");
        }

        var result = await _iteraApiClient.UploadDocumentAsync(
            request.File,
            request.Source,
            request.Description,
            request.Cnpj,
            cancellationToken);

        return Ok(result);
    }
}
