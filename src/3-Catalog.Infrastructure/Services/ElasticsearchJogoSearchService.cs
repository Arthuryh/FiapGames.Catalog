using System.Net.Http.Json;
using System.Text.Json;
using DTOs;
using Entities;
using Interfaces;
using Microsoft.Extensions.Configuration;

namespace Services;

public sealed class ElasticsearchJogoSearchService(HttpClient httpClient, IConfiguration configuration)
    : IJogoSearchService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _indexName = configuration["Elasticsearch:Index"] ?? "fiapgames-jogos";

    public async Task IndexarAsync(Jogo jogo, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var document = new JogoSearchDocument(
            jogo.Id,
            jogo.Nome,
            jogo.Descricao,
            jogo.Preco,
            jogo.ObterPrecoAtual(),
            jogo.DataLancamento,
            jogo.Promocao is null
                ? null
                : new PromocaoResponseDto(
                    jogo.Promocao.Id,
                    jogo.Promocao.Nome,
                    jogo.Promocao.TaxaDesconto,
                    jogo.Promocao.DataInicio,
                    jogo.Promocao.DataFim,
                    jogo.Promocao.Ativo));

        using var response = await httpClient.PutAsJsonAsync(
            $"{_indexName}/_doc/{jogo.Id}?refresh=wait_for",
            document,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<JogoResponseDto>> BuscarAsync(
        string termo,
        int limite,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var request = new
        {
            size = Math.Clamp(limite, 1, 100),
            query = new
            {
                multi_match = new
                {
                    query = termo,
                    fields = new[] { "nome^3", "descricao" },
                    fuzziness = "AUTO"
                }
            }
        };

        using var response = await httpClient.PostAsJsonAsync(
            $"{_indexName}/_search",
            request,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var payload = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        var hits = payload.RootElement.GetProperty("hits").GetProperty("hits");
        var results = new List<JogoResponseDto>(hits.GetArrayLength());

        foreach (var hit in hits.EnumerateArray())
        {
            var jogo = hit.GetProperty("_source").Deserialize<JogoResponseDto>(JsonOptions);
            if (jogo is not null)
                results.Add(jogo);
        }

        return results;
    }

    private void EnsureConfigured()
    {
        if (httpClient.BaseAddress is null)
            throw new InvalidOperationException("Elasticsearch:Endpoint must be configured.");
    }

    private sealed record JogoSearchDocument(
        int Id,
        string Nome,
        string Descricao,
        decimal Preco,
        decimal PrecoAtual,
        DateTime DataLancamento,
        PromocaoResponseDto? Promocao);
}