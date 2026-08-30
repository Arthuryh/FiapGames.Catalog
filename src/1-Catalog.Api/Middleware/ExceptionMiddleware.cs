using System.Net;
using System.Text;
using System.Text.Json;
namespace Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
                if (!context.Request.Path.ToString().Contains("swagger"))
                {
                    if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized ||
                            context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                    {
                        throw new UnauthorizedAccessException();
                    }
                    string requestBody = await ReadRequestBody(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var objErro = ex switch
            {
                UnauthorizedAccessException => new ErroResponseDto("Acesso não autorizado", HttpStatusCode.Unauthorized),

                ArgumentException => new ErroResponseDto($"Requisição inválida: {ex.Message}", HttpStatusCode.BadRequest),

                _ => new ErroResponseDto("Erro interno do servidor", HttpStatusCode.InternalServerError)
            };

            context.Response.StatusCode = (int)objErro.Status;

            try
            {
                string requestBody = await ReadRequestBody(context);

                Console.WriteLine($"objeto:{requestBody}\nerro:{objErro.Erro}\nurl:{context.Request.Path}\nstatus:{context.Response.StatusCode}");
                
            }
            catch { /* Silencioso para garantir a resposta ao usuário */ }

            await context.Response.WriteAsync(JsonSerializer.Serialize(objErro));
        }

        private async Task<string> ReadRequestBody(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            return body;
        }

        private string TratarDadosSensiveis(string path, string body)
        {
            if (string.IsNullOrEmpty(body)) return body;
            if (path.Contains("/login", StringComparison.OrdinalIgnoreCase)) return "[PROTEGIDO]";
            return body.Length > 2000 ? body.Substring(0, 2000) : body;
        }
        public record ErroResponseDto(string Erro, HttpStatusCode Status);
    }
}
