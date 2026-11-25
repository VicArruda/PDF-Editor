# PDF Editor

Aplicativo desktop em C# para identificar e futuramente editar informações em documentos PDF.

## Objetivo

Localizar textos específicos em documentos PDF, identificar todas as suas ocorrências e permitir a substituição individual dos valores selecionados.

## Tecnologias

- C#
- .NET 10 for Windows
- WPF
- WebView2 (visualização contínua de PDF)
- PdfPig (extração de texto e coordenadas)
- GitHub Actions

## Estrutura

- Models: modelos de dados.
- Services: serviços de processamento.
- App.xaml: inicialização e recursos da interface.
- MainWindow.xaml: interface principal.
- MainWindow.xaml.cs: abertura e gerenciamento do documento.

## Etapa 0.2 — Interface desktop inicial

- Migração da aplicação Console para WPF.
- Janela principal com menu Arquivo.
- Abertura de documentos PDF pelo seletor de arquivos.
- Visualização contínua do documento via WebView2.
- Integração do scanner existente na abertura do arquivo.
- Exibição de estatísticas básicas do texto extraído.
- Botão Salvar reservado para a etapa de edição.

**Limitação atual:** o scanner extrai palavras e coordenadas, mas ainda não classifica campos editáveis. Os destaques amarelos e a edição direta serão implementados em etapas posteriores.

## Compilação

O executável Windows é gerado pelo GitHub Actions.

O Microsoft Edge WebView2 Runtime precisa estar disponível no Windows para a visualização integrada.

## Próximas etapas

- Identificação de campos editáveis.
- Destaque visual das ocorrências no documento.
- Seleção individual de ocorrências.
- Edição e substituição do conteúdo.
- Preservação da formatação.
- Salvamento do documento editado.
