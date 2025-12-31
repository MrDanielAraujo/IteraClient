using IteraClient.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using IteraClient.Services;

namespace IteraClient.Utils;

public static class UsingEndPoins
{
    public static async Task<IteraStatusResponse> GetIteraStatusAsync(this IteraAuthService iteraAuthService, Guid id)
    {
        using var httpClient = await iteraAuthService.CreateAuthorizedClientAsync();

        var url = string.Format(iteraAuthService.EndPoints.IteraStatus, id);

        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<IteraStatusResponse>(await response.Content.ReadAsStringAsync()) ?? new IteraStatusResponse();
    }

    public static async Task<List<ControladoraResponse>> GetExportJsonAsync(this IteraAuthService iteraAuthService, long cnpj)
    {
        using var httpClient = await iteraAuthService.CreateAuthorizedClientAsync();
        
        var url = string.Format(iteraAuthService.EndPoints.ExportJson, cnpj);

        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        response.EnsureSuccessStatusCode();

        var jsonNode = JsonNode.Parse(await response.Content.ReadAsStringAsync());

        var listasContainer = (JsonObject)jsonNode!;

        var listaControladora = new List<ControladoraResponse>();

        foreach (var prop in listasContainer)
        {
            if (prop.Value is JsonArray jsonArray )
            {
                listaControladora.AddRange(JsonSerializer.Deserialize<List<ControladoraResponse>>(jsonArray.ToJsonString())!);
            }
        }
        
        return listaControladora;
    }
    
    public static async Task<string> GetIteraDeParaAsync(this IteraAuthService iteraAuthService, Guid id)
    {
        using var httpClient = await iteraAuthService.CreateAuthorizedClientAsync();

        var url = string.Format(iteraAuthService.EndPoints.IteraDePara, id);

        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        response.EnsureSuccessStatusCode();

        //return JsonSerializer.Deserialize<IteraStatusResponse>(await response.Content.ReadAsStringAsync()) ?? new IteraStatusResponse();
        return await response.Content.ReadAsStringAsync();
    }
}
