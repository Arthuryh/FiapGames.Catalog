# FiapGames.Catalog

Microserviço responsável pelo catálogo de jogos, promoção de títulos e gestão de biblioteca da plataforma FiapGames.

Este serviço centraliza as operações relacionadas à consulta e organização dos jogos disponíveis, controle de promoções e acesso à biblioteca do usuário.

> Objetivo: oferecer uma base organizada e extensível para o catálogo, seguindo princípios de Clean Architecture, separação de responsabilidades e autenticação via JWT.

---

## Arquitetura do Projeto

A solução é estruturada em camadas inspiradas em Clean Architecture e DDD, com foco em manutenibilidade, testes e evolução incremental.

### Executar via Docker

Na raiz do projeto, execute:

```bash
docker compose up --build
```

### Estrutura da solução

```txt
FiapGames.Catalog.sln

src/
├── 1-Catalog.Api
├── 2-Catalog.Application
├── 3-Catalog.Infrastructure
└── 4-Catalog.Domain
```

### Responsabilidades das camadas

#### 1-Catalog.Api

Camada de exposição da API HTTP.

Responsável por:
- Endpoints REST para catálogo, jogos, compras e promoções
- Configuração de autenticação e autorização
- Swagger/OpenAPI
- Middleware e pipeline HTTP
- Injeção de dependências

#### 2-Catalog.Application

Camada de aplicação.

Responsável por:
- Regras de negócio do catálogo
- Serviços de aplicação
- Casos de uso
- DTOs
- Interfaces de contratos

#### 4-Catalog.Domain

Camada de domínio.

Responsável por:
- Entidades como jogo, biblioteca, compra e promoção
- Regras de domínio
- Objetos de valor
- Contratos principais
- Lógica independente de framework

#### 3-Catalog.Infrastructure

Camada de infraestrutura.

Responsável por:
- Persistência de dados com Entity Framework Core
- Repositórios
- Contexto do banco
- Implementações técnicas

---

## Principais Funcionalidades

Este microserviço é responsável por:
- Cadastro e consulta de jogos
- Gestão de catálogo
- Promoções e ofertas
- Biblioteca do usuário
- Registro de compras relacionadas ao catálogo
- Integração com autenticação JWT

---

## Stack Tecnológica

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger / OpenAPI
- xUnit (testes)

---

## Padrões Utilizados

- Clean Architecture
- SOLID
- Dependency Injection
- Repository Pattern
- Separation of Concerns
- Domain-Oriented Design

---

## Configuração do Ambiente

### Pré-requisitos

Antes de executar este projeto, certifique-se de ter instalado:
- .NET SDK 9+
- SQL Server
- Docker Desktop
- Visual Studio 2022+ ou Rider
- EF Core CLI

Instalar o CLI do Entity Framework:

```bash
dotnet tool install --global dotnet-ef
```

ou atualizar:

```bash
dotnet tool update --global dotnet-ef
```

### Exemplo de configuração

```json
{
  "ConnectionStrings": {
    "FIAPGamesConnection": "Server=localhost,1437;Database=fiapgames_catalog;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False"
  },
  "Jwt": {
    "Key": "sua-chave-super-secreta",
    "Issuer": "FiapGames",
    "Audience": "FiapGamesUsers"
  }
}
```

---

## Executando o Projeto

Restaurar dependências:

```bash
dotnet restore
```

Executar a aplicação:

```bash
dotnet run --project src/1-Catalog.Api
```

---

## Migrations

Criar migration:

```bash
dotnet ef migrations add InitialCreate \
--project src/3-Catalog.Infrastructure \
--startup-project src/1-Catalog.Api
```

Aplicar migrations:

```bash
dotnet ef database update \
--project src/3-Catalog.Infrastructure \
--startup-project src/1-Catalog.Api
```

---

## Testes

Executar testes:

```bash
dotnet test
```

---

## Licença

Projeto desenvolvido para fins acadêmicos e evolução arquitetural da plataforma FiapGames.
