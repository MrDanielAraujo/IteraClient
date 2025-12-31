using IteraClient.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IteraClient.Utils;


public static class UsingEndPoins
{
    public static async Task<IteraStatusResponse> GetIteraStatusAsync(this HttpClient httpClient, Guid id)
    {
        var endpoints = GetEndPoints();

        var url = string.Format(endpoints.IteraStatus, id);

        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<IteraStatusResponse>(await response.Content.ReadAsStringAsync()) ?? new IteraStatusResponse();
    }

    public static async Task<List<ControladoraResponse>> GetExportJsonAsync(this HttpClient httpClient, long cnpj)
    {
        var endpoints = GetEndPoints();

        var url = string.Format(endpoints.ExportJson, cnpj);

        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        response.EnsureSuccessStatusCode();

        var jsonNode = JsonNode.Parse(await response.Content.ReadAsStringAsync());

        var listasContainer = (JsonObject)jsonNode;

        var listaControladora = new List<ControladoraResponse>();

        foreach (var prop in listasContainer)
        {
            if (prop.Value is JsonArray jsonArray )
            {
                listaControladora.AddRange(JsonSerializer.Deserialize<List<ControladoraResponse>>(jsonArray.ToJsonString()));
            }
        }
        
        return listaControladora;
    }


    private static EndPoints GetEndPoints()
    {
        var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

        var endpoints = new EndPoints();
        configuration.GetSection("EndPoints").Bind(endpoints);

        return endpoints;
    }
}
