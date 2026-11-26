namespace PdfEditor.Models;

public sealed class PdfFieldOccurrence
{
    public string FieldName { get; init; } = string.Empty;

    public string OriginalValue { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;

    public int PageNumber { get; init; }

    // Coordenadas em pontos PDF; origem no canto inferior esquerdo.
    // O retângulo cobre o valor, não o rótulo.
    public double Left { get; init; }

    public double Bottom { get; init; }

    public double Width { get; init; }

    public double Height { get; init; }

    public double PageWidth { get; init; }

    public double PageHeight { get; init; }

    public bool IsSelected { get; set; }
}