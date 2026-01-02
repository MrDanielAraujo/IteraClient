# IteraClient API

API cliente para integração com os serviços Itera, desenvolvida em .NET 8 seguindo os princípios SOLID.

## Índice

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Princípios SOLID](#princípios-solid)
- [Requisitos](#requisitos)
- [Configuração](#configuração)
- [Endpoints](#endpoints)
  - [Itera - Endpoints Diretos](#itera---endpoints-diretos)
  - [Processamento em Lote](#processamento-em-lote)
- [Fluxo de Processamento](#fluxo-de-processamento)
- [Banco de Dados](#banco-de-dados)
- [Migração para PostgreSQL](#migração-para-postgresql)
- [Exemplos de Uso](#exemplos-de-uso)

---

## Visão Geral

O **IteraClient** é uma API REST que fornece integração completa com os serviços da Itera para processamento de documentos financeiros. A aplicação permite:

- **Autenticação**: Obtenção e gerenciamento de tokens JWT
- **Upload de Documentos**: Envio de arquivos para processamento
- **Monitoramento**: Acompanhamento do status de processamento
- **Exportação**: Obtenção dos dados extraídos dos documentos
- **Processamento em Lote**: Gerenciamento completo de múltiplos documentos

---

## Arquitetura

O projeto segue uma arquitetura em camadas com separação clara de responsabilidades:

```
IteraClient/
├── Controllers/                    # Camada de apresentação (API)
│   ├── IteraController.cs         # Endpoints diretos da Itera
│   └── DocumentProcessingController.cs  # Processamento em lote
│
├── Services/Implementations/       # Camada de serviços (lógica de negócio)
│   ├── TokenService.cs            # Gerenciamento de tokens
│   ├── JwtValidator.cs            # Validação de JWT
│   ├── AuthorizedHttpClientFactory.cs  # Criação de clientes HTTP
│   ├── IteraApiClient.cs          # Cliente da API Itera
│   └── DocumentProcessingService.cs    # Orquestração de processamento
│
├── Interfaces/                     # Contratos (abstrações)
│   ├── ITokenService.cs
│   ├── IJwtValidator.cs
│   ├── IAuthorizedHttpClientFactory.cs
│   ├── IIteraApiClient.cs
│   ├── IDocumentProcessingService.cs
│   ├── IRepository.cs             # Repositório genérico
│   ├── IDocumentRepository.cs
│   └── IDocumentExportResultRepository.cs
│
├── Data/                           # Camada de dados
│   ├── IteraDbContext.cs          # Contexto do Entity Framework
│   ├── Entities/                  # Entidades do banco
│   │   ├── Document.cs
│   │   └── DocumentExportResult.cs
│   └── Repositories/Implementations/  # Implementações dos repositórios
│       ├── Repository.cs
│       ├── DocumentRepository.cs
│       └── DocumentExportResultRepository.cs
│
├── Models/                         # DTOs e modelos
│   ├── AccessToken.cs
│   ├── ControladoraResponse.cs
│   ├── IteraStatusResponse.cs
│   ├── UploadDocumentRequest.cs
│   ├── UploadDocumentResponse.cs
│   └── ProcessingModels.cs
│
├── Configuration/                  # Configurações
│   ├── IteraSettings.cs           # Options Pattern
│   └── ServiceCollectionExtensions.cs  # Registro de DI
│
└── Program.cs                      # Ponto de entrada
```

---

## Princípios SOLID

O projeto foi desenvolvido seguindo rigorosamente os princípios SOLID:

### Single Responsibility Principle (SRP)
Cada classe tem uma única responsabilidade:
- `TokenService`: Apenas gerencia tokens
- `JwtValidator`: Apenas valida JWTs
- `IteraApiClient`: Apenas comunica com a API Itera
- `DocumentProcessingService`: Apenas orquestra o fluxo de processamento

### Open/Closed Principle (OCP)
O código está aberto para extensão e fechado para modificação:
- Novas funcionalidades podem ser adicionadas criando novas implementações das interfaces
- O comportamento pode ser estendido sem modificar código existente

### Liskov Substitution Principle (LSP)
Todas as implementações podem ser substituídas por suas interfaces:
- `ITokenService` pode ser implementado de diferentes formas
- Repositórios podem usar diferentes bancos de dados

### Interface Segregation Principle (ISP)
Interfaces são específicas e focadas:
- `ITokenService`: Apenas métodos de token
- `IJwtValidator`: Apenas validação
- `IDocumentRepository`: Métodos específicos para documentos

### Dependency Inversion Principle (DIP)
Dependências são injetadas via construtor:
- Controllers dependem de interfaces, não de implementações
- Serviços recebem suas dependências via DI

---

## Requisitos

- .NET 8.0 SDK
- Visual Studio 2022 / VS Code / Rider

### Pacotes NuGet

| Pacote | Versão | Descrição |
|--------|--------|-----------|
| Microsoft.EntityFrameworkCore | 8.0.11 | ORM para acesso a dados |
| Microsoft.EntityFrameworkCore.InMemory | 8.0.11 | Banco em memória |
| Microsoft.Extensions.Caching.Memory | 10.0.1 | Cache de tokens |
| Microsoft.IdentityModel.Tokens | 8.15.0 | Validação de JWT |
| System.IdentityModel.Tokens.Jwt | 8.15.0 | Manipulação de JWT |
| Swashbuckle.AspNetCore | 6.6.2 | Documentação Swagger |

---

## Configuração

### appsettings.json

```json
{
  "IteraSettings": {
    "AuthConfig": {
      "Username": "seu_usuario",
      "Password": "sua_senha",
      "UrlAuth": "https://api.itera.com.br/auth"
    },
    "EndPoints": {
      "GetStatus": "https://api.itera.com.br/status",
      "GetExportJson": "https://api.itera.com.br/export",
      "GetDePara": "https://api.itera.com.br/depara",
      "UploadDoc": "https://api.itera.com.br/upload"
    }
  }
}
```

### Executando o Projeto

```bash
# Restaurar pacotes
dotnet restore

# Compilar
dotnet build

# Executar
dotnet run

# Acessar Swagger
# http://localhost:5000/swagger
```

---

## Endpoints

### Itera - Endpoints Diretos

Endpoints que comunicam diretamente com a API Itera.

#### GET /Itera/GetAccessToken

Obtém um token JWT válido para autenticação.

**Resposta:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Características:**
- Token é cacheado automaticamente
- Tokens expirados são renovados
- Validação antes do retorno

---

#### GET /Itera/GetStatus

Consulta o status de processamento de um documento.

**Parâmetros:**
| Nome | Tipo | Obrigatório | Descrição |
|------|------|-------------|-----------|
| documentId | Guid | Não | ID do documento na Itera |

**Exemplo:**
```
GET /Itera/GetStatus?documentId=490d3ec0-fe70-4e40-b286-09246f5e6d5d
```

**Status possíveis:**
- `Pendente`: Aguardando processamento
- `Processando`: Em processamento
- `Concluido`: Finalizado com sucesso
- `Erro`: Falha no processamento

---

#### GET /Itera/GetExport

Exporta dados extraídos de documentos processados.

**Parâmetros:**
| Nome | Tipo | Obrigatório | Descrição |
|------|------|-------------|-----------|
| cnpj | long | Não | CNPJ da empresa (14 dígitos) |

**Exemplo:**
```
GET /Itera/GetExport?cnpj=34413970000130
```

**Resposta:**
```json
[
  {
    "codigo": "1.01",
    "valor": "1000000.00",
    "empresa": "Empresa XYZ",
    "cnpj": "34413970000130",
    "data": "2024-12-31",
    "tipo_balanco": "Patrimonial"
  }
]
```

---

#### GET /Itera/GetDePara

Obtém o mapeamento De-Para de um documento.

**Parâmetros:**
| Nome | Tipo | Obrigatório | Descrição |
|------|------|-------------|-----------|
| documentId | Guid | Não | ID do documento na Itera |

**Exemplo:**
```
GET /Itera/GetDePara?documentId=490d3ec0-fe70-4e40-b286-09246f5e6d5d
```

---

#### POST /Itera/UploadDocument

Envia um documento para processamento.

**Content-Type:** `multipart/form-data`

**Parâmetros:**
| Nome | Tipo | Obrigatório | Descrição |
|------|------|-------------|-----------|
| File | file | Sim | Arquivo do documento |
| Cnpj | string | Sim | CNPJ (14 dígitos) |
| Source | string | Não | Origem do documento |
| Description | string | Não | Descrição |

**Exemplo com cURL:**
```bash
curl -X POST "http://localhost:5000/Itera/UploadDocument" \
  -F "File=@documento.pdf" \
  -F "Cnpj=34413970000130" \
  -F "Source=Sistema Interno" \
  -F "Description=Balanço 2024"
```

---

### Processamento em Lote

Endpoints para gerenciamento e processamento em lote de documentos.

#### POST /DocumentProcessing/Documents

Adiciona um documento ao banco de dados.

**Content-Type:** `application/json`

**Body:**
```json
{
  "fileName": "balanco-2024.pdf",
  "contentBase64": "JVBERi0xLjQK...",
  "contentType": "application/pdf",
  "cnpj": "34413970000130",
  "source": "Sistema Contábil",
  "description": "Balanço Patrimonial 2024"
}
```

**Campos:**
| Nome | Tipo | Obrigatório | Descrição |
|------|------|-------------|-----------|
| fileName | string | Sim | Nome do arquivo |
| contentBase64 | string | Sim | Conteúdo em Base64 |
| contentType | string | Não | Tipo MIME (padrão: application/pdf) |
| cnpj | string | Sim | CNPJ (14 dígitos) |
| source | string | Não | Origem do documento |
| description | string | Não | Descrição |

**Resposta (201 Created):**
```json
{
  "id": "9d2fa731-837f-460c-9357-9417b9ae280c",
  "fileName": "balanco-2024.pdf",
  "cnpj": "34413970000130",
  "source": "Sistema Contábil",
  "description": "Balanço Patrimonial 2024",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

#### POST /DocumentProcessing/Documents/Batch

Adiciona múltiplos documentos em uma requisição.

**Body:**
```json
[
  {
    "fileName": "balanco-q1.pdf",
    "contentBase64": "JVBERi0xLjQK...",
    "cnpj": "34413970000130"
  },
  {
    "fileName": "balanco-q2.pdf",
    "contentBase64": "JVBERi0xLjQK...",
    "cnpj": "34413970000130"
  }
]
```

**Resposta:**
```json
{
  "created": [
    { "id": "9d2fa731-...", "fileName": "balanco-q1.pdf", "cnpj": "34413970000130" },
    { "id": "11be1db9-...", "fileName": "balanco-q2.pdf", "cnpj": "34413970000130" }
  ],
  "errors": []
}
```

---

#### GET /DocumentProcessing/Documents

Lista todos os documentos no banco de dados.

**Resposta:**
```json
[
  {
    "id": "9d2fa731-837f-460c-9357-9417b9ae280c",
    "fileName": "balanco-2024.pdf",
    "cnpj": "34413970000130",
    "iteraDocumentId": null,
    "iteraStatus": null,
    "isProcessed": false,
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

---

#### GET /DocumentProcessing/Documents/{id}

Obtém detalhes de um documento específico.

**Exemplo:**
```
GET /DocumentProcessing/Documents/9d2fa731-837f-460c-9357-9417b9ae280c
```

---

#### POST /DocumentProcessing/ProcessBatch

**Endpoint principal** - Processa múltiplos documentos em lote.

**Body:**
```json
{
  "documentIds": [
    "9d2fa731-837f-460c-9357-9417b9ae280c",
    "11be1db9-1c28-4253-aece-b9727a5d3288"
  ],
  "waitForCompletion": true,
  "timeoutSeconds": 300,
  "pollingIntervalSeconds": 10
}
```

**Parâmetros:**
| Nome | Tipo | Padrão | Descrição |
|------|------|--------|-----------|
| documentIds | array | - | Lista de GUIDs dos documentos |
| waitForCompletion | bool | true | Aguardar conclusão |
| timeoutSeconds | int | 300 | Timeout em segundos |
| pollingIntervalSeconds | int | 10 | Intervalo de polling |

**Resposta:**
```json
{
  "totalDocuments": 2,
  "successCount": 2,
  "errorCount": 0,
  "processingCount": 0,
  "documentStatuses": [
    {
      "documentId": "9d2fa731-...",
      "iteraDocumentId": "abc123...",
      "fileName": "balanco-2024.pdf",
      "status": "Concluido",
      "isSuccess": true,
      "isProcessing": false,
      "exportedResultsCount": 150
    }
  ],
  "isSuccess": true,
  "message": "Processamento concluído: 2 sucesso, 0 erro"
}
```

---

#### GET /DocumentProcessing/Status/{documentId}

Verifica o status de um documento específico.

**Exemplo:**
```
GET /DocumentProcessing/Status/9d2fa731-837f-460c-9357-9417b9ae280c
```

**Status possíveis:**
| Status | Descrição |
|--------|-----------|
| NotFound | Documento não existe |
| NotUploaded | Não enviado para Itera |
| Uploaded | Enviado, aguardando |
| Processando | Em processamento |
| Concluido | Finalizado com sucesso |
| Erro | Falha no processamento |

---

#### GET /DocumentProcessing/Export/{documentId}

Obtém os resultados exportados salvos no banco local.

**Exemplo:**
```
GET /DocumentProcessing/Export/9d2fa731-837f-460c-9357-9417b9ae280c
```

**Resposta:**
```json
{
  "documentId": "9d2fa731-...",
  "cnpj": "34413970000130",
  "isSuccess": true,
  "recordsCount": 150,
  "data": [...]
}
```

---

## Fluxo de Processamento

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FLUXO DE PROCESSAMENTO EM LOTE                   │
└─────────────────────────────────────────────────────────────────────┘

1. CADASTRO DE DOCUMENTOS
   ┌──────────────────┐
   │ POST /Documents  │ ──► Armazena documento (Base64) no banco local
   └──────────────────┘

2. INÍCIO DO PROCESSAMENTO
   ┌──────────────────────┐
   │ POST /ProcessBatch   │ ──► Recebe array de IDs
   └──────────────────────┘
            │
            ▼
   ┌──────────────────────┐
   │ Busca documentos     │ ──► Recupera do banco local
   │ no banco de dados    │
   └──────────────────────┘
            │
            ▼
   ┌──────────────────────┐
   │ Upload para Itera    │ ──► Converte Base64 → File
   │ (para cada documento)│     Envia via API
   └──────────────────────┘
            │
            ▼
3. MONITORAMENTO (se waitForCompletion = true)
   ┌──────────────────────┐
   │ Polling de Status    │ ◄──┐
   │ GET /Itera/GetStatus │    │
   └──────────────────────┘    │
            │                  │
            ▼                  │
   ┌──────────────────────┐    │
   │ Status = Concluido?  │────┘ Não (aguarda pollingInterval)
   └──────────────────────┘
            │ Sim
            ▼
4. OBTENÇÃO DOS RESULTADOS
   ┌──────────────────────┐
   │ GET /Itera/GetExport │ ──► Obtém dados extraídos
   └──────────────────────┘
            │
            ▼
   ┌──────────────────────┐
   │ Salva no banco local │ ──► DocumentExportResults
   └──────────────────────┘
            │
            ▼
5. CONSULTA DOS RESULTADOS
   ┌──────────────────────┐
   │ GET /Export/{id}     │ ──► Retorna dados do banco local
   └──────────────────────┘
```

---

## Banco de Dados

### Entidades

#### Document
Armazena os documentos para processamento.

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único |
| FileName | string | Nome do arquivo |
| ContentBase64 | string | Conteúdo em Base64 |
| ContentType | string | Tipo MIME |
| Cnpj | string | CNPJ da empresa |
| Source | string | Origem do documento |
| Description | string | Descrição |
| IteraDocumentId | Guid? | ID na Itera |
| IteraStatus | string | Status na Itera |
| IsProcessed | bool | Processado com sucesso |
| CreatedAt | DateTime | Data de criação |
| UpdatedAt | DateTime? | Data de atualização |
| ErrorMessage | string | Mensagem de erro |

#### DocumentExportResult
Armazena os resultados exportados.

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único |
| DocumentId | Guid | Referência ao documento |
| Codigo | string | Código do item |
| Valor | string | Valor extraído |
| Empresa | string | Nome da empresa |
| Cnpj | string | CNPJ |
| ... | ... | Demais campos de exportação |

---

## Migração para PostgreSQL

O projeto está preparado para migração. Siga os passos:

### 1. Adicionar pacote NuGet

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
```

### 2. Atualizar appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=iteraclient;Username=postgres;Password=sua_senha"
  }
}
```

### 3. Modificar ServiceCollectionExtensions.cs

```csharp
public static IServiceCollection AddIteraDatabase(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    // Comentar ou remover:
    // services.AddDbContext<IteraDbContext>(options =>
    //     options.UseInMemoryDatabase("IteraClientDb"));
    
    // Descomentar:
    services.AddDbContext<IteraDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    
    services.AddScoped<IDocumentRepository, DocumentRepository>();
    services.AddScoped<IDocumentExportResultRepository, DocumentExportResultRepository>();
    
    return services;
}
```

### 4. Criar migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Exemplos de Uso

### Fluxo Completo com cURL

```bash
# 1. Adicionar documento
curl -X POST "http://localhost:5000/DocumentProcessing/Documents" \
  -H "Content-Type: application/json" \
  -d '{
    "fileName": "balanco-2024.pdf",
    "contentBase64": "'$(base64 -w 0 documento.pdf)'",
    "cnpj": "34413970000130",
    "source": "Sistema",
    "description": "Balanço 2024"
  }'

# Resposta: {"id":"9d2fa731-837f-460c-9357-9417b9ae280c",...}

# 2. Processar documento
curl -X POST "http://localhost:5000/DocumentProcessing/ProcessBatch" \
  -H "Content-Type: application/json" \
  -d '{
    "documentIds": ["9d2fa731-837f-460c-9357-9417b9ae280c"],
    "waitForCompletion": true
  }'

# 3. Verificar status (se não aguardou conclusão)
curl "http://localhost:5000/DocumentProcessing/Status/9d2fa731-837f-460c-9357-9417b9ae280c"

# 4. Obter resultados
curl "http://localhost:5000/DocumentProcessing/Export/9d2fa731-837f-460c-9357-9417b9ae280c"
```

### Exemplo em C#

```csharp
using var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5000");

// 1. Adicionar documento
var document = new
{
    FileName = "balanco-2024.pdf",
    ContentBase64 = Convert.ToBase64String(File.ReadAllBytes("documento.pdf")),
    Cnpj = "34413970000130",
    Source = "Sistema",
    Description = "Balanço 2024"
};

var response = await client.PostAsJsonAsync("/DocumentProcessing/Documents", document);
var created = await response.Content.ReadFromJsonAsync<dynamic>();
var documentId = created.id;

// 2. Processar
var processRequest = new
{
    DocumentIds = new[] { documentId },
    WaitForCompletion = true
};

var processResponse = await client.PostAsJsonAsync("/DocumentProcessing/ProcessBatch", processRequest);
var result = await processResponse.Content.ReadFromJsonAsync<BatchProcessingResult>();

Console.WriteLine($"Processados: {result.SuccessCount} sucesso, {result.ErrorCount} erro");

// 3. Obter resultados
var exportResponse = await client.GetFromJsonAsync<ExportResult>(
    $"/DocumentProcessing/Export/{documentId}");

Console.WriteLine($"Registros exportados: {exportResponse.RecordsCount}");
```

---

## Licença

Este projeto é proprietário e de uso interno.

---

## Suporte

Para dúvidas ou problemas, entre em contato com a equipe de desenvolvimento.
