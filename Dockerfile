# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore (keeps layer caching like Payment service)

# Preserve repository `src/` layout when copying to keep paths consistent
COPY ["src/1-Catalog.Api/1-Catalog.Api.csproj", "src/1-Catalog.Api/"]
COPY ["src/2-Catalog.Application/2-Catalog.Application.csproj", "src/2-Catalog.Application/"]
COPY ["src/3-Catalog.Infrastructure/3-Catalog.Infrastructure.csproj", "src/3-Catalog.Infrastructure/"]
COPY ["src/4-Catalog.Domain/4-Catalog.Domain.csproj", "src/4-Catalog.Domain/"]

RUN dotnet restore "src/1-Catalog.Api/1-Catalog.Api.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/src/1-Catalog.Api"

RUN dotnet publish "1-Catalog.Api.csproj" \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

EXPOSE 8082

ENV ASPNETCORE_URLS=http://+:8082
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "1-Catalog.Api.dll"]