using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using PdfEditor.Services;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PdfEditor;

public partial class MainWindow : Window
{
    private readonly PdfScanner _scanner = new();
    private bool _webViewInitialized;
    private string? _currentFilePath;

    public MainWindow()
    {
        InitializeComponent();
        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializePdfViewerAsync();
    }

    private async Task InitializePdfViewerAsync()
    {
        try
        {
            await PdfViewer.EnsureCoreWebView2Async();

            PdfViewer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            PdfViewer.CoreWebView2.Settings.AreDevToolsEnabled = false;
            PdfViewer.CoreWebView2.Settings.IsStatusBarEnabled = false;

            _webViewInitialized = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = "Não foi possível inicializar o visualizador: " + ex.Message;
        }
    }

    private async void OpenPdf_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Abrir documento PDF",
            Filter = "Documentos PDF (*.pdf)|*.pdf",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await OpenPdfAsync(dialog.FileName);
    }

    private async Task OpenPdfAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show(
                "O arquivo selecionado não foi encontrado.",
                "PDF Editor",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            return;
        }

        try
        {
            StatusText.Text = "Abrindo documento...";
            ScannerStatusText.Text = "Analisando documento...";
            DocumentNameText.Text = Path.GetFileName(filePath);

            if (!_webViewInitialized)
            {
                await InitializePdfViewerAsync();
            }

            if (!_webViewInitialized)
            {
                throw new InvalidOperationException(
                    "O componente de visualização não foi inicializado."
                );
            }

            // O visualizador integrado do WebView2 apresenta o PDF
            // com rolagem contínua, sem converter as páginas em imagens.
            PdfViewer.Visibility = Visibility.Visible;
            EmptyState.Visibility = Visibility.Collapsed;
            PdfViewer.CoreWebView2.Navigate(new Uri(filePath).AbsoluteUri);

            _currentFilePath = filePath;

            IReadOnlyList<PdfEditor.Models.PdfOccurrence> occurrences =
                await Task.Run(() => _scanner.ScanFile(filePath));

            int pageCount = occurrences
                .Select(item => item.PageNumber)
                .Distinct()
                .Count();

            WordCountText.Text = occurrences.Count.ToString("N0");
            PageCountText.Text = $"Páginas com texto analisado: {pageCount}";
            ScannerStatusText.Text = "Leitura concluída";
            StatusText.Text = $"Documento aberto: {Path.GetFileName(filePath)}";

            // O scanner atual extrai palavras e coordenadas.
            // A classificação de campos editáveis será implementada
            // separadamente, para evitar tratar todo texto como editável.
        }
        catch (Exception ex)
        {
            ScannerStatusText.Text = "Falha na análise";
            StatusText.Text = "Não foi possível abrir ou analisar o documento.";

            MessageBox.Show(
                ex.Message,
                "Erro ao abrir PDF",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
        {
            OpenPdf_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
