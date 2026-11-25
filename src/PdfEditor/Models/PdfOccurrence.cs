
namespace PdfEditor.Models;

public sealed class PdfOccurrence
{
    public string FileName { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;

    public int PageNumber { get; init; }

    public string Text { get; init; } = string.Empty;

    // Coordenadas em pontos PDF.
    // Origem: canto inferior esquerdo da página.

    public double Left { get; init; }

    public double Bottom { get; init; }

    public double Width { get; init; }

    public double Height { get; init; }

    // Dimensões totais da página.

    public double PageWidth { get; init; }

    public double PageHeight { get; init; }
}
