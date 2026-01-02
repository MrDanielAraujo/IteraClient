using IteraClient.Interfaces;
using IteraClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace IteraClient.Controllers;

/// <summary>
/// Controller responsável pelos endpoints diretos da API Itera.
/// Fornece acesso às funcionalidades básicas de autenticação, consulta de status,
/// exportação de dados e upload de documentos.
/// </summary>
/// <remarks>
/// Este controller segue os princípios SOLID:
/// - **SRP**: Apenas gerencia requisições HTTP e delega para serviços
/// - **DIP**: Depende de abstrações (interfaces) injetadas via construtor
/// </remarks>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Tags("Itera - Endpoints Diretos")]
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
    /// Obtém um token de acesso válido para autenticação na API Itera.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna um token JWT válido para autenticação nas demais chamadas à API Itera.
    /// 
    /// **Características:**
    /// - O token é automaticamente cacheado para evitar requisições desnecessárias
    /// - Tokens expirados são renovados automaticamente
    /// - O token é validado antes de ser retornado
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /Itera/GetAccessToken
    /// ```
    /// 
    /// **Resposta de sucesso:**
    /// ```
    /// eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// ```
    /// </remarks>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Token JWT de acesso</returns>
    /// <response code="200">Token obtido com sucesso</response>
    /// <response code="500">Erro interno ao obter o token</response>
    [HttpGet("GetAccessToken")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccessToken(CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
        return Ok(token);
    }

    /// <summary>
    /// Consulta o status de processamento de um documento na Itera.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite verificar em qual estágio de processamento um documento se encontra.
    /// 
    /// **Status possíveis:**
    /// - `Pendente`: Documento aguardando processamento
    /// - `Processando`: Documento em processamento
    /// - `Concluido`: Processamento finalizado com sucesso
    /// - `Erro`: Ocorreu um erro no processamento
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /Itera/GetStatus?documentId=490d3ec0-fe70-4e40-b286-09246f5e6d5d
    /// ```
    /// 
    /// **Quando usar:**
    /// - Após fazer upload de um documento, use este endpoint para monitorar o progresso
    /// - Faça polling periódico (ex: a cada 10 segundos) até o status ser "Concluido" ou "Erro"
    /// </remarks>
    /// <param name="documentId">ID único do documento na Itera (GUID). Se não informado, usa um valor padrão para testes.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Objeto contendo o status atual do documento</returns>
    /// <response code="200">Status obtido com sucesso</response>
    /// <response code="500">Erro interno ao consultar o status</response>
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
    /// Exporta os dados extraídos de documentos processados pelo CNPJ.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todos os dados extraídos dos documentos processados para um determinado CNPJ.
    /// 
    /// **Dados retornados incluem:**
    /// - Informações financeiras (valores, moeda, escala)
    /// - Dados da empresa (nome, CNPJ)
    /// - Metadados do documento (página, seção, subseção)
    /// - Classificações (tipo de balanço, consolidado)
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /Itera/GetExport?cnpj=34413970000130
    /// ```
    /// 
    /// **Importante:**
    /// - O CNPJ deve ser informado apenas com números (sem pontos, barras ou traços)
    /// - Só retorna dados de documentos que já foram processados com sucesso
    /// - Verifique o status do documento antes de chamar este endpoint
    /// </remarks>
    /// <param name="cnpj">CNPJ da empresa (apenas números, 14 dígitos). Se não informado, usa um valor padrão para testes.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Lista de dados extraídos dos documentos</returns>
    /// <response code="200">Dados exportados com sucesso</response>
    /// <response code="500">Erro interno ao exportar os dados</response>
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
    /// Obtém o mapeamento De-Para de um documento processado.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna o mapeamento De-Para que relaciona os dados extraídos
    /// com as categorias e classificações padrão do sistema.
    /// 
    /// **O que é o De-Para:**
    /// - Mapeia termos encontrados no documento para termos padronizados
    /// - Permite normalização dos dados extraídos
    /// - Facilita a integração com outros sistemas
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /Itera/GetDePara?documentId=490d3ec0-fe70-4e40-b286-09246f5e6d5d
    /// ```
    /// 
    /// **Quando usar:**
    /// - Após o documento ser processado com sucesso
    /// - Para entender como os dados foram classificados
    /// - Para integração com sistemas que usam nomenclatura diferente
    /// </remarks>
    /// <param name="documentId">ID único do documento na Itera (GUID). Se não informado, usa um valor padrão para testes.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Dados do mapeamento De-Para em formato JSON</returns>
    /// <response code="200">Mapeamento obtido com sucesso</response>
    /// <response code="500">Erro interno ao obter o mapeamento</response>
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
    /// Realiza o upload de um documento para processamento na API Itera.
    /// </summary>
    /// <remarks>
    /// Este endpoint envia um documento para a Itera processar e extrair informações.
    /// 
    /// **Formatos aceitos:**
    /// - PDF (recomendado)
    /// - Imagens (PNG, JPG, JPEG)
    /// 
    /// **Parâmetros do form-data:**
    /// | Campo | Tipo | Obrigatório | Descrição |
    /// |-------|------|-------------|-----------|
    /// | File | file | Sim | Arquivo do documento |
    /// | Cnpj | text | Sim | CNPJ da empresa (14 dígitos) |
    /// | Source | text | Não | Origem/sistema de onde vem o documento |
    /// | Description | text | Não | Descrição do documento |
    /// 
    /// **Exemplo de uso com cURL:**
    /// ```bash
    /// curl -X POST "http://localhost:5000/Itera/UploadDocument" \
    ///   -F "File=@documento.pdf" \
    ///   -F "Cnpj=34413970000130" \
    ///   -F "Source=Sistema Interno" \
    ///   -F "Description=Balanço 2024"
    /// ```
    /// 
    /// **Fluxo após upload:**
    /// 1. Guarde o `Uid` retornado na resposta
    /// 2. Use `GET /Itera/GetStatus` para monitorar o processamento
    /// 3. Quando status for "Concluido", use `GET /Itera/GetExport` para obter os dados
    /// </remarks>
    /// <param name="request">Dados do documento em formato multipart/form-data</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Resposta contendo o ID do documento criado e status inicial</returns>
    /// <response code="200">Upload realizado com sucesso</response>
    /// <response code="400">Dados inválidos (arquivo vazio ou CNPJ não informado)</response>
    /// <response code="500">Erro interno ao realizar o upload</response>
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
