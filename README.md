# FiapGames.Catalog

Microservico responsavel pelo catalogo de jogos, promocoes, compras e biblioteca do usuario na plataforma FiapGames.

O Catalog e a porta de entrada do fluxo de compra: a API recebe os jogos escolhidos, extrai o usuario e o e-mail do Bearer Token, registra a compra como pendente e publica a solicitacao no RabbitMQ para o microservico de Payment.

## Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- RabbitMQ
- JWT Bearer Authentication
- Swagger / OpenAPI
- Docker / Docker Compose

## Arquitetura

A solucao segue uma separacao inspirada em Clean Architecture:

```txt
src/
  1-Catalog.Api             API HTTP, DI, auth, Swagger e pipeline
  2-Catalog.Application     Casos de uso, DTOs, interfaces e eventos
  3-Catalog.Infrastructure  EF Core, repositories, RabbitMQ e workers
  4-Catalog.Domain          Entidades e regras de dominio
```

## Fluxo de compra

1. Cliente chama `POST /api/Compra` com os IDs dos jogos.
2. Catalog extrai `UsuarioId` e `EmailUsuario` do Bearer Token.
3. Catalog valida:
   - jogo existe;
   - nao ha jogos duplicados na requisicao;
   - usuario ainda nao possui o jogo na biblioteca.
4. Catalog salva a compra com status `Pendente`.
5. Catalog publica `CompraSolicitadaIntegrationEvent` no RabbitMQ.
6. Payment consome a compra, processa pagamento e publica o resultado.
7. Catalog consome `PagamentoProcessadoIntegrationEvent`.
8. Se aprovado, Catalog libera os jogos na biblioteca e marca a compra como `Aprovado`.
9. Se recusado, Catalog marca a compra como `Reprovado` e salva o motivo da recusa.

## RabbitMQ

### Catalog -> Payment

- Exchange: `catalogo.exchange`
- Queue: `pagamento.compra.solicitada`
- Routing key: `catalogo.compra.solicitada`

Contrato publicado:

```json
{
  "CompraId": 2,
  "UsuarioId": 2,
  "JogosIds": [3],
  "ValorTotal": 80.0,
  "SolicitadaEm": "2026-07-05T01:40:01.042226Z",
  "EmailUsuario": "TESTE@TESTE.COM",
  "RastreioId": "b9d6b483-2f1f-4962-a6fa-37535d212355"
}
```

### Payment -> Catalog

- Exchange: `pagamento.exchange`
- Queue: `catalogo.pagamento.processado`
- Routing keys:
  - `pagamento.aprovado`
  - `pagamento.recusado`

Contrato consumido:

```json
{
  "CompraId": 2,
  "UsuarioId": 2,
  "Aprovado": true,
  "ValorTotal": 80.0,
  "Status": "Aprovado",
  "ProcessadoEm": "2026-07-05T01:41:00Z",
  "RastreioId": "b9d6b483-2f1f-4962-a6fa-37535d212355",
  "MotivoRecusa": null
}
```

## Endpoints principais

Endpoints que dependem do usuario autenticado usam o ID e o e-mail do Bearer Token.

### Compras

- `POST /api/Compra`
  - Cria uma compra pendente e publica a solicitacao para o Payment.
  - Body:

```json
{
  "jogosIds": [1, 2]
}
```

- `GET /api/Compra`
  - Lista as compras do usuario autenticado.
  - Retorna status `Pendente`, `Aprovado` ou `Reprovado`.

### Biblioteca

- `GET /api/Biblioteca`
  - Lista os jogos liberados na biblioteca do usuario autenticado.

- `DELETE /api/Biblioteca/jogos/{jogoId}`
  - Remove um jogo da biblioteca do usuario autenticado.

### Jogos

- `GET /api/Jogo`
- `GET /api/Jogo/{id}`
- `POST /api/Jogo`
- `POST /api/Jogo/promocao`

### Promocoes

- `GET /api/Promocao`
- `GET /api/Promocao/{id}`
- `POST /api/Promocao`
- `PUT /api/Promocao`
- `DELETE /api/Promocao/{id}`

## Configuracao

Exemplo de `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "FIAPGamesConnection": "Server=(localdb)\\mssqllocaldb;Database=fiapgames_catalog;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "ChaveSuperSecretaFiapGamesEAD12MuitoLongaParaGarantirSeguranca123!",
    "Issuer": "FiapGamesApi",
    "Audience": "FiapGamesClients"
  },
  "RabbitMq": {
    "HostName": "127.0.0.1",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

## Executando com Docker

Na raiz do projeto:

```bash
docker-compose up --build
```

Servicos expostos:

- Catalog API: `http://localhost:8082`
- RabbitMQ: `localhost:5672`
- RabbitMQ Management: `http://localhost:15672`
- SQL Server Catalog: `localhost:1437`

O Docker Compose usa a rede compartilhada `fiapgames-network`, a mesma esperada pelo microservico Payment.

## Executando localmente

Restaurar dependencias:

```bash
dotnet restore FiapGames.Catalog.slnx
```

Executar a API:

```bash
dotnet run --project src/1-Catalog.Api/1-Catalog.Api.csproj
```

## Migrations

Criar migration:

```bash
dotnet ef migrations add NomeDaMigration --project src/3-Catalog.Infrastructure/3-Catalog.Infrastructure.csproj --startup-project src/1-Catalog.Api/1-Catalog.Api.csproj
```

Aplicar migrations:

```bash
dotnet ef database update --project src/3-Catalog.Infrastructure/3-Catalog.Infrastructure.csproj --startup-project src/1-Catalog.Api/1-Catalog.Api.csproj
```

As migrations tambem sao aplicadas na inicializacao da API via `DbInitializer`.

## Validacao

Compilar:

```bash
dotnet build FiapGames.Catalog.slnx
```

Testar:

```bash
dotnet test
```

## Observacoes de arquitetura

- O Catalog nao deve confiar em `UsuarioId` vindo no body da requisicao.
- O usuario e o e-mail sao derivados do Bearer Token.
- O Payment nao tem acesso ao token HTTP original, por isso o Catalog envia `EmailUsuario` no evento.

## Kubernetes (autonomia por serviço)

Manifests próprios do serviço estão em `k8s/`:

- `catalog-api-configmap.yaml`
- `catalog-api-secret.yaml`
- `catalog-api-service.yaml`
- `catalog-api-deployment.yaml`

Separacao de configuracao:

- ConfigMap: ambiente, urls, issuer/audience JWT, host e porta do RabbitMQ.
- Secret: connection string completa, chave JWT e credenciais do RabbitMQ.
