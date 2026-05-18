using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCRDesktop.Infrastructure;
using OCRDesktop.Infrastructure.Models;
using OCRDesktop.Services;
using OCRDesktop.Services.Interfaces;

namespace OCRDesktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IOcrApiService _ocrApi;
    private readonly IErpApiService _erpApi;

    [ObservableProperty]
    private string _debugPath = "Escaneando...";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Listo";

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private OrderReview? _currentOrderReview;

    [ObservableProperty]
    private OcrProcessResult? _currentResult;

    [ObservableProperty]
    private bool _isValidationPopupOpen;

    [ObservableProperty]
    private ErpOrdenResumen? _ordenOdooSeleccionada;

    public ObservableCollection<OcrFile> Files { get; } = new();
    public ObservableCollection<OcrProcessResult> Results { get; } = new();
    public ObservableCollection<ErpOrdenResumen> OrdenesOdoo { get; } = new();

    public MainViewModel()
    {
        _ocrApi = new OcrApiService();
        _erpApi = new ErpApiService();
        RefreshCommand.Execute(null);
        _ = CargarOrdenesOdoo();
    }

    [RelayCommand]
    private void ToggleFile(OcrFile? file)
    {
        if (file != null)
        {
            file.IsSelected = !file.IsSelected;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsBusy = true;
        StatusMessage = "Sincronizando archivos...";

        var (fileNames, path) = await _ocrApi.GetFilesToProcessAsync();
        DebugPath = path;

        Files.Clear();
        foreach (var name in fileNames)
        {
            if (!Results.Any(r => r.FileName == name))
            {
                Files.Add(new OcrFile { Name = name });
            }
        }

        StatusMessage = $"Encontrados {Files.Count} archivos pendientes";
        IsBusy = false;
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var file in Files)
        {
            file.IsSelected = true;
        }
    }

    [RelayCommand]
    private async Task Process()
    {
        var selected = Files.Where(f => f.IsSelected).ToList();
        if (!selected.Any()) return;

        IsBusy = true;
        StatusMessage = $"Procesando {selected.Count} documentos...";

        foreach (var f in selected) f.IsProcessing = true;

        var processResults = await _ocrApi.ProcessFilesAsync(selected.Select(f => f.Name).ToList());

        foreach (var res in processResults)
        {
            Results.Insert(0, res);

            var pendingFile = Files.FirstOrDefault(f => f.Name == res.FileName);
            if (pendingFile != null)
            {
                Files.Remove(pendingFile);
            }
        }

        foreach (var f in selected)
        {
            f.IsProcessing = false;
        }

        StatusMessage = !processResults.Any()
            ? "El proceso no devolvió resultados. Comprueba el backend."
            : "Proceso completado";

        IsBusy = false;
    }

    [RelayCommand]
    private async Task SaveToDb(OcrProcessResult? result)
    {
        if (result?.ExtractedData == null) return;

        IsBusy = true;
        StatusMessage = $"Guardando {result.FileName} en BBDD...";

        var (saved, message, movedToErronies) = await _ocrApi.SaveToDbAsync(result.ExtractedData, result.FileName);
        if (saved)
        {
            var (finalized, finalMessage) = await _ocrApi.FinalizeProcessAsync(result.FileName);
            if (finalized)
            {
                Results.Remove(result);
                await CargarOrdenesOdoo();
                await Refresh();
                MessageBox.Show($"Archivo {result.FileName} guardado y movido correctamente a 'Processed'.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Guardado en BBDD pero hubo un error al mover el archivo:\n\n{finalMessage}", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            if (movedToErronies)
            {
                Results.Remove(result);
                await Refresh();
            }

            MessageBox.Show(
                movedToErronies
                    ? $"{message}\n\nEl PDF y el JSON (si existía) se han movido a Storage/Erronies."
                    : $"Error al guardar en la base de datos:\n\n{message}",
                "Error de Guardado",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task DiscardResult(OcrProcessResult? result)
    {
        if (result == null) return;

        var confirm = MessageBox.Show($"¿Estás seguro de que quieres descartar los resultados de {result.FileName}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm == MessageBoxResult.Yes)
        {
            Results.Remove(result);
            await Refresh();
            StatusMessage = $"Resultados de {result.FileName} descartados";
        }
    }

    [RelayCommand]
    private void ReviewResult(OcrProcessResult? result)
    {
        if (result?.ExtractedData == null) return;

        try
        {
            var dto = OcrOrderMapper.ParseExtractedData(result.ExtractedData);
            if (dto == null)
            {
                MessageBox.Show("No se pudieron leer los datos extraídos del JSON.");
                return;
            }

            var review = OcrOrderMapper.ToOrderReview(dto);
            CurrentOrderReview = review;
            CurrentResult = result;
            IsValidationPopupOpen = true;
            StatusMessage = $"Revisando: {result.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar datos para revisión: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveReviewedToDb()
    {
        if (CurrentOrderReview == null || CurrentResult == null) return;

        IsBusy = true;
        StatusMessage = $"Guardando {CurrentResult.FileName} en BBDD...";

        var (saved, message, movedToErronies) = await _ocrApi.SaveToDbAsync(CurrentOrderReview, CurrentResult.FileName);
        if (saved)
        {
            var (finalized, finalMessage) = await _ocrApi.FinalizeProcessAsync(CurrentResult.FileName);
            if (finalized)
            {
                Results.Remove(CurrentResult);
                IsValidationPopupOpen = false;
                CurrentOrderReview = null;
                CurrentResult = null;
                await CargarOrdenesOdoo();
                await Refresh();
                MessageBox.Show("Venta guardada y procesada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Venta guardada pero hubo un error al mover el archivo:\n\n{finalMessage}", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            if (movedToErronies)
            {
                Results.Remove(CurrentResult);
                IsValidationPopupOpen = false;
                CurrentOrderReview = null;
                CurrentResult = null;
                await Refresh();
            }

            MessageBox.Show(
                movedToErronies
                    ? $"{message}\n\nEl PDF y el JSON (si existía) se han movido a Storage/Erronies."
                    : $"Error al guardar: {message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private void CancelReview()
    {
        CurrentOrderReview = null;
        CurrentResult = null;
        IsValidationPopupOpen = false;
        StatusMessage = "Listo";
    }

    [RelayCommand]
    private async Task ClearHistory()
    {
        var result = MessageBox.Show(
            "¿Estás seguro de que deseas borrar permanentemente el historial de archivos procesados?\n\nEsta operación no se puede deshacer.",
            "Confirmar Borrado de Historial",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;
        StatusMessage = "Borrando historial...";

        var success = await _ocrApi.ClearHistoryAsync();

        if (success)
        {
            StatusMessage = "Historial borrado correctamente";
            MessageBox.Show("El historial ha sido borrado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            StatusMessage = "Error al borrar el historial";
            MessageBox.Show("Hubo un error al intentar borrar el historial. Por favor, comprueba el backend.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task CargarOrdenesOdoo()
    {
        IsBusy = true;
        StatusMessage = "Cargando órdenes disponibles para Odoo...";

        var selectedNumero = OrdenOdooSeleccionada?.Numero;
        var (ok, msg, ordenes) = await _erpApi.GetOrdenesAsync();

        if (ok)
        {
            OrdenesOdoo.Clear();
            foreach (var orden in ordenes)
            {
                OrdenesOdoo.Add(orden);
            }

            OrdenOdooSeleccionada = !string.IsNullOrWhiteSpace(selectedNumero)
                ? OrdenesOdoo.FirstOrDefault(o => o.Numero == selectedNumero)
                : OrdenesOdoo.FirstOrDefault();

            StatusMessage = OrdenesOdoo.Count == 0
                ? "No hay órdenes guardadas para enviar a Odoo"
                : $"Órdenes disponibles para Odoo: {OrdenesOdoo.Count}";
        }
        else
        {
            StatusMessage = $"No se pudieron cargar las órdenes para Odoo: {msg}";
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task CrearPresupuestoEnOdoo()
    {
        if (OrdenOdooSeleccionada == null)
        {
            MessageBox.Show("Selecciona una orden antes de crear el presupuesto en Odoo.", "Odoo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsBusy = true;
        StatusMessage = $"Enviando orden {OrdenOdooSeleccionada.Numero} a Odoo...";

        var (ok, msg, data) = await _erpApi.CrearPresupuestoAsync(OrdenOdooSeleccionada.Numero);
        if (ok && data != null)
        {
            MessageBox.Show(
                $"{msg}\n\nPedido Odoo: {data.OdooSaleOrderName}\nId interno: {data.OdooSaleOrderId}",
                "Presupuesto Odoo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            StatusMessage = "Presupuesto creado en Odoo";
            await CargarOrdenesOdoo();
        }
        else
        {
            MessageBox.Show(msg, "Error al crear presupuesto", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error al crear presupuesto en Odoo";
        }

        IsBusy = false;
    }
}
