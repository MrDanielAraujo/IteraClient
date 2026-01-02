namespace IteraClient.Models;

/// <summary>
/// Resultado do processamento em lote de documentos.
/// </summary>
public class BatchProcessingResult
{
    /// <summary>
    /// Total de documentos processados.
    /// </summary>
    public int TotalDocuments { get; set; }

    /// <summary>
    /// Documentos processados com sucesso.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Documentos com erro.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Documentos em processamento.
    /// </summary>
    public int ProcessingCount { get; set; }

    /// <summary>
    /// Detalhes de cada documento processado.
    /// </summary>
    public List<DocumentProcessingStatus> DocumentStatuses { get; set; } = new();

    /// <summary>
    /// Indica se o processamento foi concluído com sucesso.
    /// </summary>
    public bool IsSuccess => ErrorCount == 0 && ProcessingCount == 0;

    /// <summary>
    /// Mensagem geral do processamento.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Status de processamento de um documento individual.
/// </summary>
public class DocumentProcessingStatus
{
    /// <summary>
    /// ID do documento local.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// ID do documento na Itera.
    /// </summary>
    public Guid? IteraDocumentId { get; set; }

    /// <summary>
    /// Nome do arquivo.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Status atual.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Indica se foi processado com sucesso.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Indica se ainda está em processamento.
    /// </summary>
    public bool IsProcessing { get; set; }

    /// <summary>
    /// Mensagem de erro, se houver.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Quantidade de resultados exportados.
    /// </summary>
    public int ExportedResultsCount { get; set; }
}

/// <summary>
/// Resultado da exportação de dados.
/// </summary>
public class ExportResult
{
    /// <summary>
    /// ID do documento.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// CNPJ associado.
    /// </summary>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a exportação foi bem sucedida.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Quantidade de registros exportados.
    /// </summary>
    public int RecordsCount { get; set; }

    /// <summary>
    /// Dados exportados.
    /// </summary>
    public List<ControladoraResponse> Data { get; set; } = new();

    /// <summary>
    /// Mensagem de erro, se houver.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request para processamento em lote.
/// </summary>
public class BatchProcessingRequest
{
    /// <summary>
    /// Lista de IDs dos documentos a processar.
    /// </summary>
    public List<Guid> DocumentIds { get; set; } = new();

    /// <summary>
    /// Indica se deve aguardar a conclusão do processamento.
    /// </summary>
    public bool WaitForCompletion { get; set; } = true;

    /// <summary>
    /// Tempo máximo de espera em segundos (padrão: 300 = 5 minutos).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Intervalo entre verificações de status em segundos (padrão: 10).
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 10;
}
