using System.Globalization;
using CapturaDePolizas_2026_NET8.Business;
using CapturaDePolizas_2026_NET8.Models;
using Microsoft.VisualBasic;

namespace CapturaDePolizas_2026_NET8;

public partial class FormAjustarCheque : Form
{
    private ChequePrintSettings _settings;
    private bool _cargandoPlantilla;

    public FormAjustarCheque()
    {
        InitializeComponent();

        _settings = ChequePrintSettings.Cargar();

        ConfigurarGrid();
        EnlazarEventos();
        CargarComboPlantillas();
    }

    private void ConfigurarGrid()
    {
        dgvCoordenadas.Columns.Clear();

        var colConcepto = new DataGridViewTextBoxColumn
        {
            Name = "Concepto",
            HeaderText = "CONCEPTO",
            ReadOnly = true,
            FillWeight = 50,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

        var colY = new DataGridViewTextBoxColumn
        {
            Name = "PosY",
            HeaderText = "Vertical Y (mm)",
            FillWeight = 25,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colY.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        var colX = new DataGridViewTextBoxColumn
        {
            Name = "PosX",
            HeaderText = "Horizontal X (mm)",
            FillWeight = 25,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colX.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        dgvCoordenadas.Columns.AddRange(colConcepto, colY, colX);

        string[] conceptos =
        {
            "Fecha",
            "Beneficiario",
            "Importe en número",
            "Importe en letra",
            "Concepto de póliza",
            "Inicio aplicación (1er renglón)",
            "Interlineado renglones (mm)",
            "Sumas iguales",
            "Columna: Cuenta",
            "Columna: Subcuenta",
            "Columna: Nombre Cta-Subcta",
            "Columna: Parcial",
            "Columna: Debe",
            "Columna: Haber"
        };

        foreach (string concepto in conceptos)
        {
            dgvCoordenadas.Rows.Add(concepto, "", "");
        }

        // Bloquear celdas que solo aplican en un eje (igual que en AJUSTCH.frm)
        BloquearCelda(5, 2); // Inicio aplicación solo usa Y
        BloquearCelda(6, 2); // Interlineado solo usa Y
        BloquearCelda(7, 2); // Sumas iguales solo usa Y
        for (int r = 8; r <= 13; r++)
        {
            BloquearCelda(r, 1); // Columnas de tabla solo usan X
        }
    }

    private void BloquearCelda(int fila, int columna)
    {
        DataGridViewCell celda = dgvCoordenadas.Rows[fila].Cells[columna];
        celda.ReadOnly = true;
        celda.Style.BackColor = Color.FromArgb(235, 235, 235);
        celda.Value = "—";
    }

    private void EnlazarEventos()
    {
        cmbPlantillaBanco.SelectedIndexChanged += CmbPlantillaBanco_SelectedIndexChanged;
        btnNuevoBanco.Click += BtnNuevoBanco_Click;

        btnGuardar.Click += (_, _) => GuardarAjustes();
        mnuArchivar.Click += (_, _) => GuardarAjustes();

        btnVistaPrevia.Click += (_, _) => EjecutarVistaPrevia();
        mnuVistaPrevia.Click += (_, _) => EjecutarVistaPrevia();

        btnPruebaImpresion.Click += (_, _) => EjecutarPruebaImpresion();
        mnuPruebaImpresion.Click += (_, _) => EjecutarPruebaImpresion();

        mnuRestaurarDefecto.Click += (_, _) => RestaurarPorDefecto();

        btnCerrar.Click += (_, _) => Close();
        mnuSalida.Click += (_, _) => Close();
    }

    private void CargarComboPlantillas()
    {
        _cargandoPlantilla = true;

        cmbPlantillaBanco.Items.Clear();
        foreach (ChequeTemplateConfig p in _settings.Plantillas)
        {
            cmbPlantillaBanco.Items.Add(p.NombreBanco);
        }

        int index = cmbPlantillaBanco.Items.IndexOf(_settings.PlantillaActiva);
        cmbPlantillaBanco.SelectedIndex = index >= 0 ? index : 0;

        _cargandoPlantilla = false;

        MostrarPlantillaEnPantalla(_settings.ObtenerPlantillaActiva());
    }

    private void CmbPlantillaBanco_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cargandoPlantilla || cmbPlantillaBanco.SelectedItem is null)
            return;

        // Guardar en memoria los cambios hechos a la plantilla anterior antes de cambiar
        SincronizarPantallaAPlantilla(_settings.ObtenerPlantillaActiva());

        _settings.PlantillaActiva = cmbPlantillaBanco.SelectedItem.ToString() ?? "Banamex (Estándar)";
        MostrarPlantillaEnPantalla(_settings.ObtenerPlantillaActiva());
    }

    private void MostrarPlantillaEnPantalla(ChequeTemplateConfig p)
    {
        _cargandoPlantilla = true;

        numOffsetX.Value = Math.Clamp((decimal)p.OffsetX, numOffsetX.Minimum, numOffsetX.Maximum);
        numOffsetY.Value = Math.Clamp((decimal)p.OffsetY, numOffsetY.Minimum, numOffsetY.Maximum);
        numFontSize.Value = Math.Clamp((decimal)(p.FontSize <= 0 ? 10f : p.FontSize), numFontSize.Minimum, numFontSize.Maximum);

        AsignarFila(0, p.FechaY, p.FechaX);
        AsignarFila(1, p.BeneficiarioY, p.BeneficiarioX);
        AsignarFila(2, p.ImporteNumeroY, p.ImporteNumeroX);
        AsignarFila(3, p.ImporteLetraY, p.ImporteLetraX);
        AsignarFila(4, p.ConceptoY, p.ConceptoX);

        dgvCoordenadas.Rows[5].Cells[1].Value = FormatearMm(p.InicioAplicacionY);
        dgvCoordenadas.Rows[6].Cells[1].Value = FormatearMm(p.InterlineadoAplicacionMm);
        dgvCoordenadas.Rows[7].Cells[1].Value = FormatearMm(p.SumasIgualesY);

        dgvCoordenadas.Rows[8].Cells[2].Value = FormatearMm(p.CuentaX);
        dgvCoordenadas.Rows[9].Cells[2].Value = FormatearMm(p.SubcuentaX);
        dgvCoordenadas.Rows[10].Cells[2].Value = FormatearMm(p.NombreX);
        dgvCoordenadas.Rows[11].Cells[2].Value = FormatearMm(p.ParcialX);
        dgvCoordenadas.Rows[12].Cells[2].Value = FormatearMm(p.DebeX);
        dgvCoordenadas.Rows[13].Cells[2].Value = FormatearMm(p.HaberX);

        _cargandoPlantilla = false;
    }

    private void AsignarFila(int fila, float posY, float posX)
    {
        dgvCoordenadas.Rows[fila].Cells[1].Value = FormatearMm(posY);
        dgvCoordenadas.Rows[fila].Cells[2].Value = FormatearMm(posX);
    }

    private static string FormatearMm(float valor)
    {
        return valor.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private void SincronizarPantallaAPlantilla(ChequeTemplateConfig p)
    {
        dgvCoordenadas.EndEdit();

        p.OffsetX = (float)numOffsetX.Value;
        p.OffsetY = (float)numOffsetY.Value;
        p.FontSize = (float)numFontSize.Value;

        p.FechaY = LeerMm(0, 1, p.FechaY);
        p.FechaX = LeerMm(0, 2, p.FechaX);

        p.BeneficiarioY = LeerMm(1, 1, p.BeneficiarioY);
        p.BeneficiarioX = LeerMm(1, 2, p.BeneficiarioX);

        p.ImporteNumeroY = LeerMm(2, 1, p.ImporteNumeroY);
        p.ImporteNumeroX = LeerMm(2, 2, p.ImporteNumeroX);

        p.ImporteLetraY = LeerMm(3, 1, p.ImporteLetraY);
        p.ImporteLetraX = LeerMm(3, 2, p.ImporteLetraX);

        p.ConceptoY = LeerMm(4, 1, p.ConceptoY);
        p.ConceptoX = LeerMm(4, 2, p.ConceptoX);

        p.InicioAplicacionY = LeerMm(5, 1, p.InicioAplicacionY);
        p.InterlineadoAplicacionMm = LeerMm(6, 1, p.InterlineadoAplicacionMm);
        p.SumasIgualesY = LeerMm(7, 1, p.SumasIgualesY);

        p.CuentaX = LeerMm(8, 2, p.CuentaX);
        p.SubcuentaX = LeerMm(9, 2, p.SubcuentaX);
        p.NombreX = LeerMm(10, 2, p.NombreX);
        p.ParcialX = LeerMm(11, 2, p.ParcialX);
        p.DebeX = LeerMm(12, 2, p.DebeX);
        p.HaberX = LeerMm(13, 2, p.HaberX);
    }

    private float LeerMm(int fila, int col, float fallback)
    {
        string texto = dgvCoordenadas.Rows[fila].Cells[col].Value?.ToString()?.Trim() ?? string.Empty;

        if (float.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out float val))
            return val;

        if (float.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out val))
            return val;

        return fallback;
    }

    private void BtnNuevoBanco_Click(object? sender, EventArgs e)
    {
        string nombre = Interaction.InputBox(
            "Ingrese el nombre del banco o formato de cheque (ej. Scotiabank, HSBC, BanBajío):",
            "Nuevo Formato de Cheque",
            ""
        ).Trim();

        if (string.IsNullOrWhiteSpace(nombre))
            return;

        if (_settings.Plantillas.Any(p => p.NombreBanco.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(
                "Ya existe una plantilla con ese nombre.",
                "Aviso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        SincronizarPantallaAPlantilla(_settings.ObtenerPlantillaActiva());

        ChequeTemplateConfig nueva = _settings.ObtenerPlantillaActiva().Clonar();
        nueva.NombreBanco = nombre;

        _settings.Plantillas.Add(nueva);
        _settings.PlantillaActiva = nombre;
        CargarComboPlantillas();
    }

    private void GuardarAjustes()
    {
        try
        {
            SincronizarPantallaAPlantilla(_settings.ObtenerPlantillaActiva());
            _settings.Guardar();

            MessageBox.Show(
                $"Ajustes guardados correctamente para el formato '{_settings.PlantillaActiva}'.",
                "Ajustes de Cheque",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al guardar los ajustes: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void EjecutarVistaPrevia()
    {
        SincronizarPantallaAPlantilla(_settings.ObtenerPlantillaActiva());
        ChequePrintService.MostrarVistaPrevia(
            this,
            ChequePrintData.CrearDatosPrueba(),
            _settings.ObtenerPlantillaActiva()
        );
    }

    private void EjecutarPruebaImpresion()
    {
        try
        {
            SincronizarPantallaAPlantilla(_settings.ObtenerPlantillaActiva());
            ChequePrintService.ImprimirConDialogo(
                this,
                ChequePrintData.CrearDatosPrueba(),
                _settings.ObtenerPlantillaActiva()
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al intentar imprimir prueba: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void RestaurarPorDefecto()
    {
        DialogResult result = MessageBox.Show(
            "¿Desea restaurar todas las coordenadas por defecto (Banamex, BBVA, Banorte, Santander)?",
            "Confirmar Restauración",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
            return;

        _settings = ChequePrintSettings.CrearPorDefecto();
        _settings.Guardar();
        CargarComboPlantillas();
    }
}
