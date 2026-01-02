namespace IteraClient.Configuration;

/// <summary>
/// Classe de configuração que agrupa todas as configurações do Itera.
/// Utiliza o Options Pattern do .NET para configuração tipada.
/// </summary>
public class IteraSettings
{
    /// <summary>
    /// Nome da seção no appsettings.json
    /// </summary>
    public const string SectionName = "IteraSettings";

    /// <summary>
    /// Configurações de autenticação
    /// </summary>
    public AuthSettings Auth { get; set; } = new();

    /// <summary>
    /// Configurações de endpoints da API
    /// </summary>
    public EndpointSettings Endpoints { get; set; } = new();
}

/// <summary>
/// Configurações de autenticação para a API Itera.
/// </summary>
public class AuthSettings
{
    /// <summary>
    /// Nome de usuário para autenticação
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha para autenticação
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Configurações de endpoints da API Itera.
/// </summary>
public class EndpointSettings
{
    /// <summary>
    /// URL do endpoint de autenticação
    /// </summary>
    public string Auth { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de upload de documentos
    /// </summary>
    public string UploadDoc { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de status do documento (com placeholder {0} para o ID)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de exportação JSON (com placeholder {0} para o CNPJ)
    /// </summary>
    public string ExportJson { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint De-Para (com placeholder {0} para o ID)
    /// </summary>
    public string DePara { get; set; } = string.Empty;
}
