using System.Globalization;
using System.Text;
using PdfEditor.Models;

namespace PdfEditor.Services;

public sealed class OccurrenceFinder
{
    // A busca inicial fica restrita a Referência para validar o comportamento
    // com documentos reais antes de ampliar a classificação para outros campos.
    private const string ReferenceLabel = "referencia";

    public IReadOnlyList<PdfFieldOccurrence> FindReferenceOccurrences(
        IReadOnlyList<PdfOccurrence> words)
    {
        var results = new List<PdfFieldOccurrence>();

        foreach (var pageGroup in words.GroupBy(word => new
        {
            word.FilePath,
            word.PageNumber
        }))
        {
            List<PdfOccurrence> pageWords = pageGroup
                .OrderByDescending(word => word.Bottom)
                .ThenBy(word => word.Left)
                .ToList();

            foreach (PdfOccurrence label in pageWords.Where(
                word => Normalize(word.Text) == ReferenceLabel))
            {
                PdfOccurrence? value = FindValueBesideLabel(label, pageWords)
                    ?? FindValueBelowLabel(label, pageWords);

                if (value is null)
                {
                    continue;
                }

                results.Add(new PdfFieldOccurrence
                {
                    FieldName = "Referência",
                    OriginalValue = value.Text,
                    FileName = value.FileName,
                    FilePath = value.FilePath,
                    PageNumber = value.PageNumber,
                    Left = value.Left,
                    Bottom = value.Bottom,
                    Width = value.Width,
                    Height = value.Height,
                    PageWidth = value.PageWidth,
                    PageHeight = value.PageHeight
                });
            }
        }

        return results
            .OrderBy(item => item.PageNumber)
            .ThenBy(item => item.Bottom)
            .ThenBy(item => item.Left)
            .ToList();
    }

    private static PdfOccurrence? FindValueBesideLabel(
        PdfOccurrence label,
        IReadOnlyList<PdfOccurrence> pageWords)
    {
        return pageWords
            .Where(word =>
                word != label &&
                Normalize(word.Text).Length > 0 &&
                word.Left >= label.Left + label.Width - 0.5 &&
                HasSameLine(label, word))
            .Select(word => new
            {
                Word = word,
                Gap = word.Left - (label.Left + label.Width)
            })
            .Where(item => item.Gap <= Math.Max(label.Height * 8, 30))
            .OrderBy(item => item.Gap)
            .Select(item => item.Word)
            .FirstOrDefault();
    }

    private static PdfOccurrence? FindValueBelowLabel(
        PdfOccurrence label,
        IReadOnlyList<PdfOccurrence> pageWords)
    {
        return pageWords
            .Where(word =>
                word != label &&
                Normalize(word.Text).Length > 0 &&
                word.Bottom < label.Bottom &&
                HorizontalOverlap(label, word) >= 0.25)
            .Select(word => new
            {
                Word = word,
                VerticalGap = label.Bottom - (word.Bottom + word.Height)
            })
            .Where(item => item.VerticalGap <= Math.Max(label.Height * 2.5, 12))
            .OrderBy(item => item.VerticalGap)
            .Select(item => item.Word)
            .FirstOrDefault();
    }

    private static bool HasSameLine(
        PdfOccurrence first,
        PdfOccurrence second)
    {
        double firstCenter = first.Bottom + first.Height / 2;
        double secondCenter = second.Bottom + second.Height / 2;
        double tolerance = Math.Max(
            Math.Min(first.Height, second.Height) * 0.65,
            2.0
        );

        return Math.Abs(firstCenter - secondCenter) <= tolerance;
    }

    private static double HorizontalOverlap(
        PdfOccurrence first,
        PdfOccurrence second)
    {
        double overlap = Math.Max(
            0,
            Math.Min(first.Left + first.Width, second.Left + second.Width) -
            Math.Max(first.Left, second.Left)
        );

        double smallerWidth = Math.Min(first.Width, second.Width);

        return smallerWidth <= 0 ? 0 : overlap / smallerWidth;
    }

    private static string Normalize(string value)
    {
        string decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (char character in decomposed)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }
}