using CapturaDePolizas_2026_NET8;
using CapturaDePolizas_2026_NET8.Business;
using CapturaDePolizas_2026_NET8.Entities;
using CapturaDePolizas_2026_NET8.Models;
using CapturaDePolizas_2026_NET8.Repositories;
using GaCostos.Models;
using GaCostos.Services;
using System.ComponentModel;
using System.Text;

namespace GaCostos
{
    public partial class FormAuxiliar : Form
    {
        private readonly ConfigService _configService;
        private readonly ContabilidadService _contabilidadService;
        private readonly BindingSource _bindingSource = new();

        private string? _rutaDatos;

        private NivelNavegacion _nivelActual = NivelNavegacion.CatalogoMayor;
        private OrigenDatosAuxiliar _origenDatosActual = OrigenDatosAuxiliar.Viejos;

        private readonly ICatalogoRepository _catalogoRepository;

        private int? _cuentaNuevaActual;
        private int? _subcuentaNuevaActual;

        private bool _auxiliarNuevoEsCuentaDirecta;
        private int _guiaAuxiliarActual;

        private bool _inicializandoSelectorOrigen;

        private int _filaCatalogoMayor;
        private int _filaCatalogoAuxiliar;

        private int _rangoInferiorActual;
        private int _rangoSuperiorActual;

        private bool _cierreConfirmado;

        private enum OrigenDatosAuxiliar
        {
            Viejos = 0,
            Nuevos = 1
        }

        public FormAuxiliar()
        {
            _configService = new ConfigService();
            _contabilidadService = new ContabilidadService();
            _catalogoRepository = new CatalogoRepository();

            InitializeComponent();
            InicializarFormulario();
            CargarRutaInicial();
        }

        private void InicializarFormulario()
        {
            dgvPrincipal.AutoGenerateColumns = false;
            dgvPrincipal.DataSource = _bindingSource;
            dgvPrincipal.EnableHeadersVisualStyles = false;
            dgvPrincipal.MultiSelect = true;

            dgvPrincipal.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            dgvPrincipal.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            dgvPrincipal.ColumnHeadersDefaultCellStyle.Font = new Font(dgvPrincipal.Font, FontStyle.Bold);

            dgvPrincipal.CellDoubleClick += DgvPrincipal_CellDoubleClick;
            btnRegresar.Click += BtnRegresar_Click;

            menuActualizar.Click += MenuActualizar_Click;
            menuCambiarSubdirectorio.Click += MenuCambiarSubdirectorio_Click;
            menuVerificarArchivos.Click += MenuVerificarArchivos_Click;

            dgvPrincipal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvPrincipal.BackgroundColor = Color.White;

            InicializarSelectorOrigenDatos(); ;
        }

        private void FormPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cierreConfirmado)
                return;

            // No preguntar cuando Windows o la aplicación completa
            // estén cerrando la aplicación.
            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                return;
            }

            DialogResult result = MessageBox.Show("Se regresará al menú de CAPTURA, todo cambio aplicado se perderá si no se ha guardado.\n\n¿Desea continuar?", "Confirmación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void dgvPrincipal_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            AplicarModoSeleccionGrid();
        }

        private void dgvPrincipal_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewColumn columna = dgvPrincipal.Columns[e.ColumnIndex];

            if (columna.Name == "colSaldo")
            {
                e.CellStyle.ForeColor = Color.Blue;
                e.CellStyle.Font = new Font(dgvPrincipal.Font, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                e.CellStyle.SelectionForeColor = Color.White;
            }

            if (columna.Name is "colDebe" or "colHaber" or "colRangoInferior" or "colRangoSuperior" or "colPoliza")
            {
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void btnVolverACaptura_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Se regresará al menú de CAPTURA, todo cambio aplicado se perderá si no se ha guardado.\n\n¿Desea continuar?", "Confirmación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Evita que FormClosing vuelva a preguntar.
            _cierreConfirmado = true;

            Close();
        }

        private void CargarRutaInicial()
        {
            // La ruta de archivos se sigue leyendo porque será utilizada
            // como respaldo si la base de datos no está disponible.
            _rutaDatos = _configService.LeerRutaDatos();

            if (_origenDatosActual == OrigenDatosAuxiliar.Nuevos)
            {
                // La BD es la fuente principal.
                // No necesitamos una ruta de archivos válida para continuar.
                dgvPrincipal.Visible = true;

                ActualizarInterfazSegunOrigen();
                CargarCatalogoMayor();

                return;
            }

            // Si estamos trabajando con archivos se conserva
            // exactamente la validación anterior.
            if (string.IsNullOrWhiteSpace(_rutaDatos) || !Directory.Exists(_rutaDatos))
            {
                lblRuta.Text = "Ruta de datos no configurada.";
                _bindingSource.DataSource = null;
                dgvPrincipal.Visible = false;

                return;
            }

            lblRuta.Text = _rutaDatos;
            dgvPrincipal.Visible = true;

            CargarCatalogoMayor();
        }

        private void CargarCatalogoMayor()
        {
            if (!ValidarOrigenActual(mostrarMensaje: false))
                return;

            try
            {
                IReadOnlyList<CuentaMayor> cuentas = ObtenerCatalogoMayorSegunOrigen();

                _nivelActual = NivelNavegacion.CatalogoMayor;

                ConfigurarColumnasCatalogoMayor();

                _bindingSource.DataSource = new BindingList<CuentaMayor>(cuentas.ToList());

                dgvPrincipal.Visible = true;
                ActualizarInterfazSegunOrigen();
            }
            catch (Exception ex)
            {
                MostrarError("No fue posible cargar el catálogo mayor.", ex);
            }
        }

        private void CargarCatalogoAuxiliar(int rangoInferior, int rangoSuperior)
        {
            if (!ValidarOrigenActual())
                return;

            try
            {
                IReadOnlyList<CuentaAuxiliar> cuentas = ObtenerCatalogoAuxiliarSegunOrigen(rangoInferior, rangoSuperior);

                if (_origenDatosActual == OrigenDatosAuxiliar.Nuevos &&
                    cuentas.Count == 0)
                {
                    _subcuentaNuevaActual = null;
                    _auxiliarNuevoEsCuentaDirecta = true;

                    CargarMovimientosAuxiliar(0);
                    return;
                }

                _auxiliarNuevoEsCuentaDirecta = false;
                _nivelActual = NivelNavegacion.CatalogoAuxiliar;
                _rangoInferiorActual = rangoInferior;
                _rangoSuperiorActual = rangoSuperior;

                ConfigurarColumnasCatalogoAuxiliar();

                _bindingSource.DataSource = new BindingList<CuentaAuxiliar>(cuentas.ToList());
                dgvPrincipal.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("No fue posible cargar el catálogo auxiliar.", ex);
            }
        }

        private void CargarMovimientosAuxiliar(int guia)
        {
            if (!ValidarOrigenActual())
                return;

            try
            {
                _guiaAuxiliarActual = guia;

                AuxiliarResultado resultado = ObtenerMovimientosAuxiliarSegunOrigen(guia, mesProceso: 0);
                List<MovimientoAuxiliarGridRow> movimientos = resultado.Movimientos.Select(MovimientoAuxiliarGridRow.FromMovimiento).ToList();

                _nivelActual = NivelNavegacion.Auxiliar;

                ConfigurarColumnasMovimientosAuxiliar();

                _bindingSource.DataSource = new BindingList<MovimientoAuxiliarGridRow>(movimientos);
                dgvPrincipal.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("No fue posible cargar los movimientos del auxiliar.", ex);
            }
        }

        private void EjecutarSeleccionActual()
        {
            if (dgvPrincipal.CurrentRow?.DataBoundItem is null)
                return;

            switch (_nivelActual)
            {
                case NivelNavegacion.CatalogoMayor:
                    EntrarACatalogoAuxiliar();
                    break;

                case NivelNavegacion.CatalogoAuxiliar:
                    EntrarAAuxiliar();
                    break;

                case NivelNavegacion.Auxiliar:
                    AbrirPolizaDiario();
                    break;
            }
        }

        private void EntrarACatalogoAuxiliar()
        {
            if (dgvPrincipal.CurrentRow?.DataBoundItem
                is not CuentaMayor cuenta)
            {
                return;
            }

            _filaCatalogoMayor = dgvPrincipal.CurrentRow.Index;

            if (_origenDatosActual == OrigenDatosAuxiliar.Nuevos)
            {
                if (!int.TryParse(cuenta.Cuenta, out int cuentaId))
                {
                    return;
                }

                _cuentaNuevaActual = cuentaId;
                _subcuentaNuevaActual = null;

                CargarCatalogoAuxiliar(rangoInferior: cuentaId, rangoSuperior: cuenta.RangoSuperior);

                return;
            }

            if (cuenta.RangoInferior <= 0 || cuenta.RangoSuperior <= 0)
            {
                return;
            }

            CargarCatalogoAuxiliar(cuenta.RangoInferior, cuenta.RangoSuperior);
        }

        private void EntrarAAuxiliar()
        {
            if (dgvPrincipal.CurrentRow?.DataBoundItem is not CuentaAuxiliar cuenta)
            {
                return;
            }

            if (cuenta.Guia <= 0)
                return;

            _filaCatalogoAuxiliar = dgvPrincipal.CurrentRow.Index;

            if (_origenDatosActual == OrigenDatosAuxiliar.Nuevos)
            {
                _cuentaNuevaActual = cuenta.RangoInferior;
                _subcuentaNuevaActual = cuenta.Guia;
                _auxiliarNuevoEsCuentaDirecta = false;
            }

            CargarMovimientosAuxiliar(cuenta.Guia);
        }

        private void AbrirPolizaDiario()
        {
            if (dgvPrincipal.CurrentRow?.DataBoundItem
                is not MovimientoAuxiliarGridRow movimiento)
            {
                return;
            }

            if (movimiento.Poliza <= 0)
                return;

            try
            {
                using FormPolizaDiario form = new(_contabilidadService);
                bool cargada;

                if (_origenDatosActual == OrigenDatosAuxiliar.Nuevos)
                {
                    if (!movimiento.PolizaId.HasValue)
                    {
                        MessageBox.Show(this, "El movimiento no contiene el identificador interno de la póliza.", "Póliza no disponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    PolizaAuxiliarDbDto? poliza = _catalogoRepository.ObtenerPolizaParaAuxiliar(movimiento.PolizaId.Value);

                    if (poliza is null)
                    {
                        MessageBox.Show(this, "No se encontró la póliza en la base de datos.", "Sin resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    cargada = form.CargarPolizaNueva(poliza);
                }
                
                else
                {
                    if (!ValidarRutaDatos())
                        return;

                    cargada = form.CargarPoliza(
                        rutaDatos: _rutaDatos!,
                        fechaMovimiento: movimiento.Fecha,
                        numeroPoliza: movimiento.Poliza);
                }

                if (!cargada)
                    return;

                form.ShowDialog(this);
            }
            
            catch (Exception ex)
            {
                MostrarError("No fue posible abrir la póliza.", ex);
            }
        }

        //Se podrá sustituir por AbrirPolizaSegunOrigen(movimiento);

        private void Regresar()
        {
            switch (_nivelActual)
            {
                case NivelNavegacion.Auxiliar:

                    if (_origenDatosActual == OrigenDatosAuxiliar.Nuevos && _auxiliarNuevoEsCuentaDirecta)
                    {
                        CargarCatalogoMayor();
                        RestaurarFila(_filaCatalogoMayor);
                        return;
                    }

                    CargarCatalogoAuxiliar(_rangoInferiorActual, _rangoSuperiorActual);
                    RestaurarFila(_filaCatalogoAuxiliar);
                    break;

                case NivelNavegacion.CatalogoAuxiliar:
                    CargarCatalogoMayor();
                    RestaurarFila(_filaCatalogoMayor);
                    break;

                case NivelNavegacion.CatalogoMayor:
                    dgvPrincipal.Visible = false;
                    break;
            }
        }

        private void RecargarVistaActual()
        {
            switch (_nivelActual)
            {
                case NivelNavegacion.CatalogoMayor:
                    CargarCatalogoMayor();
                    break;

                case NivelNavegacion.CatalogoAuxiliar:
                    CargarCatalogoAuxiliar(_rangoInferiorActual, _rangoSuperiorActual);
                    break;

                case NivelNavegacion.Auxiliar:
                    CargarMovimientosAuxiliar(_guiaAuxiliarActual);
                    break;
            }
        }

        private void CambiarSubdirectorio()
        {
            if (_origenDatosActual != OrigenDatosAuxiliar.Viejos)
            {
                return;
            }

            using FolderBrowserDialog dialog = new()
            {
                Description = "Seleccione la carpeta que contiene catmay, cataux, DATOS y AUXILIAR.",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                _rutaDatos = dialog.SelectedPath;

                _configService.GuardarRutaDatos(_rutaDatos);

                _filaCatalogoMayor = 0;
                _filaCatalogoAuxiliar = 0;
                _rangoInferiorActual = 0;
                _rangoSuperiorActual = 0;

                lblRuta.Text = _rutaDatos;
                dgvPrincipal.Visible = true;

                CargarCatalogoMayor();
            }
            catch (Exception ex)
            {
                MostrarError("No fue posible guardar la ruta seleccionada.", ex);
            }
        }

        private void VerificarArchivos()
        {
            if (_origenDatosActual != OrigenDatosAuxiliar.Viejos)
            {
                return;
            }

            if (!ValidarRutaDatos())
                return;

            List<string> faltantes = [];

            string catmay = Path.Combine(_rutaDatos!, "catmay");
            string cataux = Path.Combine(_rutaDatos!, "cataux");
            string datos = Path.Combine(_rutaDatos!, "DATOS");
            string auxiliar = Path.Combine(_rutaDatos!, "AUXILIAR");

            if (!File.Exists(catmay))
                faltantes.Add("catmay");

            if (!File.Exists(cataux))
                faltantes.Add("cataux");

            if (!File.Exists(datos))
                faltantes.Add("DATOS");

            if (!Directory.Exists(auxiliar))
                faltantes.Add("AUXILIAR");

            if (faltantes.Count == 0)
            {
                MessageBox.Show(this, "Los archivos principales existen en la ruta seleccionada.", "Verificación correcta", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            MessageBox.Show(this, "No se encontraron los siguientes elementos:" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, faltantes),
                "Archivos faltantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void CopiarSeleccion()
        {
            string texto = ObtenerTextoSeleccionado();

            if (string.IsNullOrWhiteSpace(texto))
                return;

            Clipboard.SetText(texto);
        }

        private void RestaurarFila(int indice)
        {
            if (indice < 0 || indice >= dgvPrincipal.Rows.Count)
                return;

            dgvPrincipal.ClearSelection();

            DataGridViewRow fila = dgvPrincipal.Rows[indice];

            fila.Selected = true;

            if (fila.Cells.Count > 0)
                dgvPrincipal.CurrentCell = fila.Cells[0];

            try
            {
                dgvPrincipal.FirstDisplayedScrollingRowIndex = indice;
            }
            catch (InvalidOperationException)
            {
                // Puede ocurrir si el grid todavía no terminó de repintarse.
            }
        }

        private bool ValidarRutaDatos(bool mostrarMensaje = true)
        {
            if (!string.IsNullOrWhiteSpace(_rutaDatos) && Directory.Exists(_rutaDatos))
                return true;

            if (mostrarMensaje)
            {
                MessageBox.Show(this, "Seleccione primero la carpeta que contiene los archivos de datos.", "Ruta no configurada", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            return false;
        }

        private void ConfigurarColumnasCatalogoMayor()
        {
            OcultarTodasLasColumnas();

            MostrarColumna("colCuenta", 0, 100);
            MostrarColumna("colNombre", 1, 330);
            MostrarColumna("colSaldo", 2, 130);
            MostrarColumna("colRangoInferior", 3, 90);
            MostrarColumna("colRangoSuperior", 4, 90);

            AplicarEstilosVisualesGrid();
        }

        private void ConfigurarColumnasCatalogoAuxiliar()
        {
            OcultarTodasLasColumnas();

            MostrarColumna("colCuenta", 0, 100);
            MostrarColumna("colNombre", 1, 330);
            MostrarColumna("colSaldo", 2, 130);
            MostrarColumna("colRangoInferior", 3, 90);
            MostrarColumna("colRangoSuperior", 4, 90);

            MostrarColumna("colGuia", 5, 80, visible: false);

            AplicarEstilosVisualesGrid();
        }

        private void ConfigurarColumnasMovimientosAuxiliar()
        {
            OcultarTodasLasColumnas();

            MostrarColumna("colFecha", 0, 100);
            MostrarColumna("colPoliza", 1, 80);
            MostrarColumna("colConcepto", 2, 350);
            MostrarColumna("colDebe", 3, 130);
            MostrarColumna("colHaber", 4, 130);
            MostrarColumna("colSaldo", 5, 130);

            AplicarEstilosVisualesGrid();
        }

        private void AlinearColumnaDerecha(string nombreColumna)
        {
            if (!dgvPrincipal.Columns.Contains(nombreColumna))
                return;

            dgvPrincipal.Columns[nombreColumna]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void MostrarError(string mensaje, Exception ex)
        {
            MessageBox.Show(this, $"{mensaje}{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DgvPrincipal_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                EjecutarSeleccionActual();
        }

        private void BtnRegresar_Click(object? sender, EventArgs e)
        {
            Regresar();
        }

        private void MenuActualizar_Click(object? sender, EventArgs e)
        {
            RecargarVistaActual();
        }

        private void MenuCambiarSubdirectorio_Click(object? sender, EventArgs e)
        {
            CambiarSubdirectorio();
        }

        private void MenuVerificarArchivos_Click(object? sender, EventArgs e)
        {
            VerificarArchivos();
        }

        private void MenuCopiar_Click(object? sender, EventArgs e)
        {
            CopiarSeleccion();
        }

        private void MenuVersion_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(this, "GaCostos 2026 - Migración a .NET 8", "Versión", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private enum NivelNavegacion
        {
            CatalogoMayor = 1,
            CatalogoAuxiliar = 2,
            Auxiliar = 3
        }

        private sealed class MovimientoAuxiliarGridRow
        {
            public string Fecha { get; init; } = string.Empty;
            public int Poliza { get; init; }
            public int? PolizaId { get; init; }
            public string Concepto { get; init; } = string.Empty;
            public string Debe { get; init; } = string.Empty;
            public string Haber { get; init; } = string.Empty;
            public string Saldo { get; init; } = string.Empty;

            public static MovimientoAuxiliarGridRow FromMovimiento(MovimientoAuxiliar movimiento)
            {
                return new MovimientoAuxiliarGridRow
                {
                    Fecha = movimiento.Fecha,
                    Poliza = movimiento.Poliza,
                    PolizaId = movimiento.PolizaId,
                    Concepto = movimiento.Concepto,

                    Debe = movimiento.Importe > 0 ? FormatHelper.FormatearImporte(movimiento.Importe) : string.Empty,
                    Haber = movimiento.Importe < 0 ? FormatHelper.FormatearImporte(Math.Abs(movimiento.Importe)) : string.Empty,
                    Saldo = FormatHelper.FormatearImporte(movimiento.SaldoAcumulado)
                };
            }
        }

        private void OcultarTodasLasColumnas()
        {
            foreach (DataGridViewColumn columna in dgvPrincipal.Columns)
            {
                columna.Visible = false;
            }
        }

        private void MostrarColumna(string nombreColumna, int displayIndex, int width, bool visible = true)
        {
            if (!dgvPrincipal.Columns.Contains(nombreColumna))
                return;

            DataGridViewColumn columna = dgvPrincipal.Columns[nombreColumna]!;

            columna.Visible = visible;
            columna.DisplayIndex = displayIndex;
            columna.Width = width;
            columna.ReadOnly = true;
        }

        private void AplicarModoSeleccionGrid()
        {
            if (menuSeleccionFilaCompleta.Checked)
            {
                dgvPrincipal.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                menuSeleccionFilaCompleta.Text = "Selección de fila completa: Habilitado";
            }
            else
            {
                dgvPrincipal.SelectionMode = DataGridViewSelectionMode.CellSelect;
                menuSeleccionFilaCompleta.Text = "Selección de fila completa: Deshabilitado";
            }

            dgvPrincipal.MultiSelect = true;
            dgvPrincipal.ClearSelection();
        }

        private void CopiarTodoElGrid(bool incluirEncabezados)
        {
            List<DataGridViewColumn> columnasVisibles = ObtenerColumnasVisiblesOrdenadas();

            if (columnasVisibles.Count == 0)
                return;

            List<DataGridViewRow> filasVisibles = dgvPrincipal.Rows.Cast<DataGridViewRow>().Where(fila => !fila.IsNewRow && fila.Visible).ToList();

            if (filasVisibles.Count == 0)
                return;

            StringBuilder texto = new();

            if (incluirEncabezados)
            {
                string encabezados = string.Join("\t", columnasVisibles.Select(columna => LimpiarTextoClipboard(columna.HeaderText)));

                texto.AppendLine(encabezados);
            }

            foreach (DataGridViewRow fila in filasVisibles)
            {
                IEnumerable<string> valores = columnasVisibles
                    .Select(columna =>
                    {
                        object? valor = fila.Cells[columna.Index].FormattedValue;

                        return LimpiarTextoClipboard(valor?.ToString() ?? string.Empty);
                    });

                texto.AppendLine(string.Join("\t", valores));
            }

            Clipboard.SetText(texto.ToString());
        }

        private List<DataGridViewColumn> ObtenerColumnasVisiblesOrdenadas()
        {
            return dgvPrincipal.Columns.Cast<DataGridViewColumn>().Where(columna => columna.Visible).OrderBy(columna => columna.DisplayIndex).ToList();
        }

        private static string LimpiarTextoClipboard(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return texto.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private string ObtenerTextoSeleccionado()
        {
            List<DataGridViewCell> celdasSeleccionadas = dgvPrincipal.SelectedCells.Cast<DataGridViewCell>()
                .Where(celda =>
                    celda.Visible &&
                    celda.OwningRow.Visible &&
                    celda.OwningColumn.Visible &&
                    !celda.OwningRow.IsNewRow)
                .ToList();

            if (celdasSeleccionadas.Count == 0)
                return string.Empty;

            int filaMin = celdasSeleccionadas.Min(celda => celda.RowIndex);
            int filaMax = celdasSeleccionadas.Max(celda => celda.RowIndex);

            int displayIndexMin = celdasSeleccionadas.Min(celda => celda.OwningColumn.DisplayIndex);
            int displayIndexMax = celdasSeleccionadas.Max(celda => celda.OwningColumn.DisplayIndex);

            List<DataGridViewColumn> columnasEnRango = dgvPrincipal.Columns.Cast<DataGridViewColumn>()
                .Where(columna =>
                    columna.Visible &&
                    columna.DisplayIndex >= displayIndexMin &&
                    columna.DisplayIndex <= displayIndexMax)
                .OrderBy(columna => columna.DisplayIndex).ToList();

            HashSet<(int RowIndex, int ColumnIndex)> celdasMarcadas = celdasSeleccionadas.Select(celda => (celda.RowIndex, celda.ColumnIndex)).ToHashSet();

            StringBuilder texto = new();

            for (int rowIndex = filaMin; rowIndex <= filaMax; rowIndex++)
            {
                DataGridViewRow fila = dgvPrincipal.Rows[rowIndex];

                if (!fila.Visible || fila.IsNewRow)
                    continue;

                List<string> valoresFila = [];

                foreach (DataGridViewColumn columna in columnasEnRango)
                {
                    bool celdaSeleccionada = celdasMarcadas.Contains(
                        (rowIndex, columna.Index));

                    if (!celdaSeleccionada)
                    {
                        valoresFila.Add(string.Empty);
                        continue;
                    }

                    object? valor = fila.Cells[columna.Index].FormattedValue;

                    valoresFila.Add(
                        LimpiarTextoClipboard(valor?.ToString() ?? string.Empty));
                }

                texto.AppendLine(string.Join("\t", valoresFila));
            }

            return texto.ToString();
        }

        private void menuDGVConEncabezados_Click(object sender, EventArgs e)
        {
            CopiarTodoElGrid(incluirEncabezados: true);
        }

        private void menuDGVSinEncabezados_Click(object sender, EventArgs e)
        {
            CopiarTodoElGrid(incluirEncabezados: false);
        }

        private void menuSeleccionFilaCompleta_Click(object sender, EventArgs e)
        {
            AplicarModoSeleccionGrid();
        }

        private void dgvPrincipal_KeyDown_1(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.C)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                CopiarTodoElGrid(incluirEncabezados: true);
                return;
            }

            if (e.Control && e.Shift && e.KeyCode == Keys.X)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                CopiarTodoElGrid(incluirEncabezados: false);
                return;
            }

            if (e.Control && e.KeyCode == Keys.C)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                CopiarSeleccion();
                return;
            }

            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            EjecutarSeleccionActual();
        }

        private void AplicarEstilosVisualesGrid()
        {
            dgvPrincipal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            AplicarEstiloBaseColumna("colCuenta", 100);
            AplicarEstiloBaseColumna("colNombre", 330);
            AplicarEstiloBaseColumna("colSaldo", 130);
            AplicarEstiloBaseColumna("colRangoInferior", 90);
            AplicarEstiloBaseColumna("colRangoSuperior", 90);
            AplicarEstiloBaseColumna("colGuia", 80);
            AplicarEstiloBaseColumna("colFecha", 100);
            AplicarEstiloBaseColumna("colPoliza", 80);
            AplicarEstiloBaseColumna("colConcepto", 350);
            AplicarEstiloBaseColumna("colDebe", 130);
            AplicarEstiloBaseColumna("colHaber", 130);

            AplicarEstiloSaldo();

            AlinearColumnaDerecha("colSaldo");
            AlinearColumnaDerecha("colRangoInferior");
            AlinearColumnaDerecha("colRangoSuperior");
            AlinearColumnaDerecha("colPoliza");
            AlinearColumnaDerecha("colDebe");
            AlinearColumnaDerecha("colHaber");

            dgvPrincipal.Invalidate();
        }

        private void AplicarEstiloBaseColumna(string nombreColumna, int ancho)
        {
            if (!dgvPrincipal.Columns.Contains(nombreColumna))
                return;

            DataGridViewColumn columna = dgvPrincipal.Columns[nombreColumna]!;

            columna.Width = ancho;
            columna.MinimumWidth = 30;
            columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            columna.ReadOnly = true;
        }

        private void AplicarEstiloSaldo()
        {
            if (!dgvPrincipal.Columns.Contains("colSaldo"))
                return;

            DataGridViewColumn columna = dgvPrincipal.Columns["colSaldo"]!;

            columna.DefaultCellStyle.ForeColor = Color.Blue;
            columna.DefaultCellStyle.Font = new Font(dgvPrincipal.Font, FontStyle.Bold);
            columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Mientras una fila está seleccionada, el color de selección puede ocultar el azul. Se deja blanco para que sea legible sobre selección azul.
            columna.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        private sealed record OpcionOrigenDatos(OrigenDatosAuxiliar Valor, string Texto)
        {
            public override string ToString()
            {
                return Texto;
            }
        }

        private void InicializarSelectorOrigenDatos()
        {
            _inicializandoSelectorOrigen = true;

            try
            {
                cmbOrigenDatos.Items.Clear();

                cmbOrigenDatos.Items.Add(new OpcionOrigenDatos(OrigenDatosAuxiliar.Nuevos,"Base de datos"));
                cmbOrigenDatos.Items.Add(new OpcionOrigenDatos(OrigenDatosAuxiliar.Viejos, "Archivos"));

                bool baseDatosDisponible = _catalogoRepository.PuedeConectarBaseDatos();

                if (baseDatosDisponible)
                {
                    _origenDatosActual = OrigenDatosAuxiliar.Nuevos;
                    cmbOrigenDatos.SelectedIndex = 0;
                }
                else
                {
                    _origenDatosActual = OrigenDatosAuxiliar.Viejos;
                    cmbOrigenDatos.SelectedIndex = 1;
                }
            }
            finally
            {
                _inicializandoSelectorOrigen = false;
            }

            ActualizarInterfazSegunOrigen();
        }

        private void cmbOrigenDatos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_inicializandoSelectorOrigen)
                return;

            if (cmbOrigenDatos.SelectedItem
                is not OpcionOrigenDatos opcion)
            {
                return;
            }

            if (_origenDatosActual == opcion.Valor)
                return;

            _origenDatosActual = opcion.Valor;

            ReiniciarNavegacionPorCambioDeOrigen();
            ActualizarInterfazSegunOrigen();

            dgvPrincipal.Visible = false;

            CargarCatalogoMayor();
        }

        private void ReiniciarNavegacionPorCambioDeOrigen()
        {
            _nivelActual = NivelNavegacion.CatalogoMayor;

            _filaCatalogoMayor = 0;
            _filaCatalogoAuxiliar = 0;
            _rangoInferiorActual = 0;
            _rangoSuperiorActual = 0;
            _bindingSource.DataSource = null;

            dgvPrincipal.ClearSelection();
        }

        private void ActualizarInterfazSegunOrigen()
        {
            bool usaArchivosViejos = _origenDatosActual == OrigenDatosAuxiliar.Viejos;

            menuCambiarSubdirectorio.Enabled = usaArchivosViejos;
            menuVerificarArchivos.Enabled = usaArchivosViejos;

            if (usaArchivosViejos)
            {
                label1.Text = "Directorio:";
                lblRuta.Text = string.IsNullOrWhiteSpace(_rutaDatos) ? "Ruta de datos no configurada." : _rutaDatos;
            }
            else
            {
                label1.Text = "Origen:";
                lblRuta.Text = "Base de datos SQL Server";
            }
        }

        private bool ValidarOrigenActual(bool mostrarMensaje = true)
        {
            if (_origenDatosActual == OrigenDatosAuxiliar.Nuevos)
            {
                return true;
            }

            return ValidarRutaDatos(mostrarMensaje);
        }

        private IReadOnlyList<CuentaMayor>ObtenerCatalogoMayorSegunOrigen()
        {
            return _origenDatosActual switch
            {
                OrigenDatosAuxiliar.Viejos => _contabilidadService.ObtenerCatalogoMayor(_rutaDatos!),
                OrigenDatosAuxiliar.Nuevos => ObtenerCatalogoMayorNuevo(),
                _ => []
            };
        }

        private IReadOnlyList<CuentaAuxiliar>ObtenerCatalogoAuxiliarSegunOrigen(int rangoInferior, int rangoSuperior)
        {
            return _origenDatosActual switch
            {
                OrigenDatosAuxiliar.Viejos => _contabilidadService.ObtenerCatalogoAuxiliar(_rutaDatos!, rangoInferior, rangoSuperior),
                OrigenDatosAuxiliar.Nuevos => ObtenerCatalogoAuxiliarNuevo(rangoInferior, rangoSuperior),
                _ => []
            };
        }

        private AuxiliarResultado ObtenerMovimientosAuxiliarSegunOrigen(int guia, int mesProceso)
        {
            return _origenDatosActual switch
            {
                OrigenDatosAuxiliar.Viejos => _contabilidadService.ObtenerMovimientosAuxiliar(_rutaDatos!, guia, mesProceso),
                OrigenDatosAuxiliar.Nuevos => ObtenerMovimientosAuxiliarNuevo(guia, mesProceso),
                _ => new AuxiliarResultado(0m, [])
            };
        }

        private IReadOnlyList<CuentaMayor>ObtenerCatalogoMayorNuevo()
        {
            IReadOnlyList<CuentaAuxiliarDbDto> cuentas = _catalogoRepository.ObtenerCuentasParaAuxiliares();

            return cuentas.Select(cuenta => new CuentaMayor(
                Cuenta: cuenta.Id.ToString(),
                Nombre: cuenta.Nombre,
                Saldo: FormatHelper.FormatearImporte(
                    cuenta.Saldo),

                    // Para Nuevos, RangoInferior guarda CuentaId.
                    RangoInferior: cuenta.Id,

                    // Solo se usa como indicador informativo.
                    RangoSuperior: cuenta.NumeroSubcuentas)).ToList();
        }

        private IReadOnlyList<CuentaAuxiliar>ObtenerCatalogoAuxiliarNuevo(int rangoInferior, int rangoSuperior)
        {
            int cuentaId = rangoInferior;

            IReadOnlyList<SubcuentaAuxiliarDbDto> subcuentas = _catalogoRepository.ObtenerSubcuentasParaAuxiliares(cuentaId);

            return subcuentas.Select(subcuenta => new CuentaAuxiliar(
                Cuenta: subcuenta.Id.ToString(),
                Nombre: subcuenta.Nombre,
                Saldo: FormatHelper.FormatearImporte(
                    subcuenta.Saldo),

                    // Conservamos el padre para consultar movimientos.
                    RangoInferior: subcuenta.CuentaId,
                    RangoSuperior: 0,

                    // En Nuevos, Guia representa SubcuentaId.
                    Guia: subcuenta.Id)).ToList();
        }

        private AuxiliarResultado ObtenerMovimientosAuxiliarNuevo(int guia, int mesProceso)
        {
            if (!_cuentaNuevaActual.HasValue)
                return new AuxiliarResultado(0m, []);

            ResultadoAuxiliarDbDto resultado = _catalogoRepository.ObtenerMovimientosParaAuxiliar(
                cuentaId: _cuentaNuevaActual.Value,
                subcuentaId: _subcuentaNuevaActual,
                mesProceso: mesProceso);

            List<MovimientoAuxiliar> movimientos = resultado.Movimientos.Select(movimiento => new MovimientoAuxiliar(
                Fecha: movimiento.Fecha.ToString("dd/MM/yy"),
                Poliza: movimiento.Folio,
                Concepto: movimiento.Concepto,
                Importe: movimiento.Importe,
                SaldoAcumulado: movimiento.SaldoAcumulado,
                PolizaId: movimiento.PolizaId)).ToList();

            return new AuxiliarResultado(SaldoInicial: resultado.SaldoInicial, Movimientos: movimientos);
        }

        // Sustituir por repositorio real
        //private IReadOnlyList<CuentaMayor>ObtenerCatalogoMayorNuevo()
        //{
        //    return _servicioNuevo.ObtenerCatalogoMayor();
        //}


    }
}
