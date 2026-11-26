using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using PdfEditor.Models;
using PdfEditor.Services;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PdfEditor;

public partial class MainWindow : Window
{
    private readonly PdfScanner _scanner = new();
    private readonly OccurrenceFinder _occurrenceFinder = new();

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
            FieldOccurrencesList.ItemsSource = null;
            FieldCountText.Text = "Localizando referências...";

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

            PdfViewer.Visibility = Visibility.Visible;
            EmptyState.Visibility = Visibility.Collapsed;
            PdfViewer.CoreWebView2.Navigate(new Uri(filePath).AbsoluteUri);

            _currentFilePath = filePath;

            IReadOnlyList<PdfOccurrence> words =
                await Task.Run(() => _scanner.ScanFile(filePath));

            IReadOnlyList<PdfFieldOccurrence> fields =
                await Task.Run(() => _occurrenceFinder.FindReferenceOccurrences(words));

            int pageCount = words
                .Select(item => item.PageNumber)
                .Distinct()
                .Count();

            WordCountText.Text = words.Count.ToString("N0");
            PageCountText.Text = $"Páginas com texto analisado: {pageCount}";
            ScannerStatusText.Text = "Leitura concluída";

            FieldOccurrencesList.ItemsSource = fields;
            FieldCountText.Text = fields.Count == 1
                ? "1 ocorrência localizada"
                : $"{fields.Count} ocorrências localizadas";

            StatusText.Text = fields.Count == 0
                ? $"Documento aberto: {Path.GetFileName(filePath)} — nenhuma referência identificada."
                : $"Documento aberto: {Path.GetFileName(filePath)} — {fields.Count} referência(s) identificada(s).";
        }
        catch (Exception ex)
        {
            ScannerStatusText.Text = "Falha na análise";
            FieldCountText.Text = "Não foi possível localizar os campos.";
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