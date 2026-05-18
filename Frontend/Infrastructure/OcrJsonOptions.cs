using System.Text.Json;
using System.Text.Json.Serialization;

namespace OCRDesktop.Infrastructure;

public static class OcrJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonSerializerOptions Indented { get; } = new(Default)
    {
        WriteIndented = true,
    };
}
