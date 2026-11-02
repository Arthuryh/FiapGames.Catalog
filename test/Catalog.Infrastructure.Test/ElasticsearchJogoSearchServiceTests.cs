using System.Net;
using System.Text;
using Entities;
using Microsoft.Extensions.Configuration;
using Services;

namespace Catalog.Infrastructure.Test;

public sealed class ElasticsearchJogoSearchServiceTests
{
    [Fact]
    public async Task BuscarAsync_UsaFuzzySearchERetornaDocumentosIndexados()
    {
        var handler = new RecordingHandler((request, body) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/fiapgames-jogos/_search", request.RequestUri!.AbsolutePath);
            Assert.Contains("\"fuzziness\":\"AUTO\"", body);
            Assert.Contains("\"nome^3\"", body);

            return JsonResponse("""
                {"hits":{"hits":[{"_source":{"id":7,"nome":"Halo","preco":20,"precoAtual":20,"descricao":"Aventura","dataLancamento":"2026-09-26T00:00:00Z","promocao":null}}]}}
                """);
        });
        var service = CreateService(handler);

        var results = await service.BuscarAsync("Halo", 10);

        var game = Assert.Single(results);
        Assert.Equal(7, game.Id);
        Assert.Equal("Halo", game.Nome);
    }

    [Fact]
    public async Task IndexarAsync_EnviaDocumentoPorIdComRefresh()
    {
        var handler = new RecordingHandler((request, body) =>
        {
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal("/fiapgames-jogos/_doc/12", request.RequestUri!.AbsolutePath);
            Assert.Equal("wait_for", request.RequestUri.Query.Split('=')[1]);
            Assert.Contains("\"nome\":\"Jogo de teste\"", body);
            return JsonResponse("{\"result\":\"created\"}");
        });
        var service = CreateService(handler);

        await service.IndexarAsync(new Jogo("Jogo de teste", 25m, "Descricao", 12));
    }

    private static ElasticsearchJogoSearchService CreateService(RecordingHandler handler)
    {
        var configuration = new ConfigurationManager
        {
            ["Elasticsearch:Index"] = "fiapgames-jogos"
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://search.example.test/")
        };
        return new ElasticsearchJogoSearchService(client, configuration);
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, string, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responder(request, body);
        }
    }
}