using IteraClient.Configuration;
using IteraClient.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao container
builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI com documentação XML
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Itera Client API",
        Version = "v1",
        Description = @"API cliente para integração com os serviços Itera.

## Visão Geral
Esta API fornece endpoints para:
- **Autenticação**: Obtenção de tokens de acesso para a API Itera
- **Consultas**: Verificação de status, exportação de dados e mapeamento De/Para
- **Upload**: Envio de documentos para processamento na Itera
- **Processamento em Lote**: Gerenciamento completo de documentos com monitoramento de status

## Fluxo de Processamento em Lote
1. Adicione documentos ao banco via `POST /DocumentProcessing/Documents`
2. Inicie o processamento via `POST /DocumentProcessing/ProcessBatch`
3. Monitore o status via `GET /DocumentProcessing/Status/{id}`
4. Obtenha os resultados via `GET /DocumentProcessing/Export/{id}`

## Banco de Dados
Atualmente utiliza banco em memória (InMemory), preparado para migração para PostgreSQL.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Suporte Técnico",
            Email = "suporte@exemplo.com"
        }
    });

    // Inclui comentários XML na documentação do Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Agrupa endpoints por tag/controller
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    options.DocInclusionPredicate((name, api) => true);
});

// Adiciona os serviços do Itera (configuração centralizada seguindo SOLID)
builder.Services.AddIteraServices(builder.Configuration);

// Adiciona o banco de dados e repositórios
// Atualmente usa banco em memória, preparado para migração para PostgreSQL
builder.Services.AddIteraDatabase(builder.Configuration);

var app = builder.Build();

// Garante que o banco de dados seja criado (útil para banco em memória)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IteraDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configura o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Itera Client API v1");
        options.DocumentTitle = "Itera Client API - Documentação";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
