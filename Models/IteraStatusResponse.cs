using IteraClient.Models;

namespace IteraClient.Models;

public class IteraStatusResponse
{
    public string uid { get; set; }
    public string customId { get; set; }
    public string cnpj { get; set; }
    public string path { get; set; }
    public string fileName { get; set; }
    public string status { get; set; }
    public string aprovedBy { get; set; }
    public string insertedBy { get; set; }
    public string lastModifiedBy { get; set; }
    public string retifiedBy { get; set; }
    public string synonymsList { get; set; }
    public string processingStartDate { get; set; }
    public string processingEndDate { get; set; }
    public int processingAttempts { get; set; }
    public string date { get; set; }
    public string insertionDate { get; set; }
    public string retifiedDate { get; set; }
    public string concludedDate { get; set; }
    public string modifyDate { get; set; }
    public bool aprovedByAlice { get; set; }
    public string rulesCreatedBy { get; set; }
    public string rulesCreatedDate { get; set; }
    public string spreadsheet_path { get; set; }
    public bool active { get; set; }
    public string company { get; set; }
    public string rejectionDate { get; set; }
    public string batch { get; set; }
    public string enterpriseName { get; set; }
    public string language { get; set; }
    public string currency { get; set; }
    public string docType { get; set; }
    public List<RetificationHistoryResponse> retificationHistory { get; set; }
    public string reviewer { get; set; }
    public string auditor { get; set; }
    public string idType { get; set; }
    public string priority { get; set; }
    public string priorityDate { get; set; }
}
