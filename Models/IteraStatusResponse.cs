namespace IteraClient.Models;

/// <summary>
/// Modelo que representa a resposta de status de um documento da API Itera.
/// </summary>
public class IteraStatusResponse
{
    public string uid { get; set; } = string.Empty;
    public string customId { get; set; } = string.Empty;
    public string cnpj { get; set; } = string.Empty;
    public string path { get; set; } = string.Empty;
    public string fileName { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
    public string aprovedBy { get; set; } = string.Empty;
    public string insertedBy { get; set; } = string.Empty;
    public string lastModifiedBy { get; set; } = string.Empty;
    public string retifiedBy { get; set; } = string.Empty;
    public string synonymsList { get; set; } = string.Empty;
    public string processingStartDate { get; set; } = string.Empty;
    public string processingEndDate { get; set; } = string.Empty;
    public int processingAttempts { get; set; }
    public string date { get; set; } = string.Empty;
    public string insertionDate { get; set; } = string.Empty;
    public string retifiedDate { get; set; } = string.Empty;
    public string concludedDate { get; set; } = string.Empty;
    public string modifyDate { get; set; } = string.Empty;
    public bool aprovedByAlice { get; set; }
    public string rulesCreatedBy { get; set; } = string.Empty;
    public string rulesCreatedDate { get; set; } = string.Empty;
    public string spreadsheet_path { get; set; } = string.Empty;
    public bool active { get; set; }
    public string company { get; set; } = string.Empty;
    public string rejectionDate { get; set; } = string.Empty;
    public string batch { get; set; } = string.Empty;
    public string enterpriseName { get; set; } = string.Empty;
    public string language { get; set; } = string.Empty;
    public string currency { get; set; } = string.Empty;
    public string docType { get; set; } = string.Empty;
    public List<RetificationHistoryResponse> retificationHistory { get; set; } = new();
    public string reviewer { get; set; } = string.Empty;
    public string auditor { get; set; } = string.Empty;
    public string idType { get; set; } = string.Empty;
    public string priority { get; set; } = string.Empty;
    public string priorityDate { get; set; } = string.Empty;
}
