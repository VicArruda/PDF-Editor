using System.IO;
using PdfEditor.Models;
using UglyToad.PdfPig;

namespace PdfEditor.Services;

public sealed class PdfScanner
{
    public IReadOnlyList<PdfOccurrence> ScanFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException(
                $"Pasta não encontrada: {folderPath}"
            );
        }

        var pdfFiles = Directory.GetFiles(
            folderPath,
            "*.pdf",
            SearchOption.TopDirectoryOnly
        );

        var occurrences = new List<PdfOccurrence>();

        foreach (string filePath in pdfFiles.OrderBy(
            Path.GetFileName,
            StringComparer.OrdinalIgnoreCase))
        {
            occurrences.AddRange(ScanFile(filePath));
        }

        return occurrences;
    }

    public IReadOnlyList<PdfOccurrence> ScanFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "O documento PDF não foi encontrado.",
                filePath
            );
        }

        if (!string.Equals(
            Path.GetExtension(filePath),
            ".pdf",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "O arquivo selecionado não possui a extensão .pdf.",
                nameof(filePath)
            );
        }

        string fileName = Path.GetFileName(filePath);
        var occurrences = new List<PdfOccurrence>();

        using PdfDocument document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word.Text))
                {
                    continue;
                }

                var bounds = word.BoundingBox;

                occurrences.Add(new PdfOccurrence
                {
                    FileName = fileName,
                    FilePath = filePath,

                    PageNumber = page.Number,

                    Text = word.Text,

                    Left = (double)bounds.Left,
                    Bottom = (double)bounds.Bottom,

                    Width = (double)bounds.Width,
                    Height = (double)bounds.Height,

                    PageWidth = (double)page.Width,
                    PageHeight = (double)page.Height
                });
            }
        }

        return occurrences;
    }
}
