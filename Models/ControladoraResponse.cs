namespace IteraClient.Models;

/// <summary>
/// Modelo que representa os dados de uma controladora retornados pela API de exportação.
/// </summary>
public class ControladoraResponse
{
    public string codigo { get; set; } = string.Empty;
    public string tempo { get; set; } = string.Empty;
    public string conf { get; set; } = string.Empty;
    public string data { get; set; } = string.Empty;
    public string data_ano { get; set; } = string.Empty;
    public string termo_total { get; set; } = string.Empty;
    public string valor { get; set; } = string.Empty;
    public string page { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string subsection { get; set; } = string.Empty;
    public string tipo { get; set; } = string.Empty;
    public string section { get; set; } = string.Empty;
    public string moeda { get; set; } = string.Empty;
    public string escala { get; set; } = string.Empty;
    public string empresa { get; set; } = string.Empty;
    public string cnpj { get; set; } = string.Empty;
    public string consolidado { get; set; } = string.Empty;
    public string tipo_balanco { get; set; } = string.Empty;
    public string y { get; set; } = string.Empty;
    public string x { get; set; } = string.Empty;
    public string unique_id { get; set; } = string.Empty;
    public string parent { get; set; } = string.Empty;
    public string is_total { get; set; } = string.Empty;
    public string over_threshold_h { get; set; } = string.Empty;
    public string over_threshold_v { get; set; } = string.Empty;
    public string bbox_left { get; set; } = string.Empty;
    public string bbox_top { get; set; } = string.Empty;
    public string bbox_height { get; set; } = string.Empty;
    public string bbox_width { get; set; } = string.Empty;
    public string data_init { get; set; } = string.Empty;
    public string formula { get; set; } = string.Empty;
}
