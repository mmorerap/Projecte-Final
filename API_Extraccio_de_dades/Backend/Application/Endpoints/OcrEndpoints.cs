using Backend.ServiceOCR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Application
{
    public static class OcrEndpoints
    {
        public static void MapOcrEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/OCRservice/files", async (IOcrService ocrService) =>
            {
                var files = await ocrService.ListFilesToProcessAsync();
                return Results.Ok(new { files, path = Path.GetFullPath("Storage/ToProcess") });
            });

            app.MapPost("/OCRservice/process", async (IOcrService ocrService, [FromBody] ProcessRequest request) =>
            {
                if (request.FileNames == null || !request.FileNames.Any())
                {
                    return Results.BadRequest("No se proporcionaron nombres de archivo.");
                }

                if (request.FileNames.Count() == 1)
                {
                    var result = await ocrService.ProcessDocumentAsync(request.FileNames.First());
                    return Results.Ok(new[] { result });
                }
                else
                {
                    var results = await ocrService.ProcessDocumentsInParallelAsync(request.FileNames);
                    return Results.Ok(results);
                }
            });

            app.MapPost("/OCRservice/finalize", async (IOcrService ocrService, [FromBody] FinalizeRequest request) =>
            {
                if (string.IsNullOrEmpty(request.FileName))
                {
                    return Results.BadRequest("Nombre de archivo no proporcionado.");
                }

                var (success, message) = await ocrService.FinalizeProcessAsync(request.FileName);
                if (success)
                {
                    return Results.Ok(new { message });
                }
                else
                {
                    return Results.Problem(detail: message, statusCode: 500, title: "Error en la finalización");
                }
            });

            app.MapGet("/OCRservice/preview/{fileName}", async (IOcrService ocrService, string fileName) =>
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return Results.BadRequest("Nombre de archivo no proporcionado.");
                }

                var preview = await ocrService.GetPreviewAsync(fileName);
                if (!preview.Success)
                {
                    return Results.BadRequest(new { error = preview.ErrorMessage });
                }

                return Results.Ok(preview);
            });
 
            app.MapDelete("/OCRservice/history", async (IOcrService ocrService) =>
            {
                var success = await ocrService.ClearHistoryAsync();
                if (success)
                {
                    return Results.Ok(new { message = "Historial borrado correctamente." });
                }
                else
                {
                    return Results.Problem(detail: "Hubo un error al intentar borrar el historial.", statusCode: 500, title: "Error de borrado");
                }
            });
        }
    }

    public class ProcessRequest
    {
        public IEnumerable<string>? FileNames { get; set; }
    }

    public class FinalizeRequest
    {
        public string? FileName { get; set; }
    }
}
