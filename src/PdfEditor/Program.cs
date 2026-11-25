
using PdfEditor.Services;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// ============================================================
// 01. CONFIGURAÇÕES
// ============================================================

string folderPath = AppContext.BaseDirectory;

// ============================================================
// 02. INICIALIZAÇÃO
// ============================================================

Console.WriteLine("================================================");
Console.WriteLine("                  PDF EDITOR");
Console.WriteLine("                  SCANNER 0.1");
Console.WriteLine("================================================");
Console.WriteLine();

Console.WriteLine($"Pasta de trabalho: {folderPath}");
Console.WriteLine();

// ============================================================
// 03. INICIALIZAR SCANNER
// ============================================================

try
{
    var scanner = new PdfScanner();

    Console.WriteLine("Procurando documentos PDF...");
    Console.WriteLine();

    var occurrences = scanner.ScanFolder(folderPath);

    if (occurrences.Count == 0)
    {
        Console.WriteLine("Nenhum texto foi encontrado.");
        Console.WriteLine();
        Console.WriteLine(
            "Verifique se existem PDFs na pasta do executável."
        );
    }
    else
    {
        // ====================================================
        // 04. RESULTADOS
        // ====================================================

        int totalFiles = occurrences
            .Select(item => item.FilePath)
            .Distinct()
            .Count();

        Console.WriteLine($"Documentos analisados: {totalFiles}");
        Console.WriteLine($"Palavras identificadas: {occurrences.Count}");
        Console.WriteLine();

        Console.WriteLine("================================================");
        Console.WriteLine("              RESULTADO DO SCANNER");
        Console.WriteLine("================================================");

        foreach (var fileGroup in occurrences.GroupBy(
            item => item.FileName))
        {
            Console.WriteLine();
            Console.WriteLine($"DOCUMENTO: {fileGroup.Key}");

            foreach (var pageGroup in fileGroup.GroupBy(
                item => item.PageNumber))
            {
                Console.WriteLine();
                Console.WriteLine($"--- PÁGINA {pageGroup.Key} ---");

                foreach (var item in pageGroup)
                {
                    Console.WriteLine(
                        $"X: {item.Left,8:F2} | " +
                        $"Y: {item.Bottom,8:F2} | " +
                        $"W: {item.Width,7:F2} | " +
                        $"H: {item.Height,7:F2} | " +
                        $"{item.Text}"
                    );
                }
            }
        }

        // ====================================================
        // 05. RESUMO
        // ====================================================

        Console.WriteLine();
        Console.WriteLine("================================================");
        Console.WriteLine("                SCAN CONCLUÍDO");
        Console.WriteLine("================================================");

        Console.WriteLine();
        Console.WriteLine($"Documentos: {totalFiles}");
        Console.WriteLine($"Palavras: {occurrences.Count}");
    }
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("================================================");
    Console.WriteLine("                 ERRO NO SCANNER");
    Console.WriteLine("================================================");
    Console.WriteLine();

    Console.WriteLine(ex.Message);
}

// ============================================================
// 06. ENCERRAMENTO
// ============================================================

Console.WriteLine();
Console.WriteLine("Pressione qualquer tecla para sair.");

Console.ReadKey();
