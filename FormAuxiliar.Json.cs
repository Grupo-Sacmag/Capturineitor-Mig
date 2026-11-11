using System.Globalization;
using CapturaDePolizas_2026_NET8.Business;
using GaCostos.Models;
using GaCostos.Services;

namespace GaCostos
{
    // Origen "JSON de trabajo": importar carpeta -> editar (F2) -> guardar JSON -> guardar en base de datos.
    public partial class FormAuxiliar
    {
        private readonly CarpetaJsonService _carpetaJsonService = new();
        private TrabajoJson? _jsonActual;
        private string? _rutaJson;
        private bool _jsonModificado;
        private int _guiaJsonActual;

        private void InicializarMenusJson()
        {
            ToolStripMenuItem importar = new("Importar carpeta a JSON...");
            importar.Click += MenuImportarCarpetaJson_Click;

            ToolStripMenuItem abrir = new("Abrir JSON...");
            abrir.Click += MenuAbrirJson_Click;

            ToolStripMenuItem guardarJson = new("Guardar JSON");
            guardarJson.Click += MenuGuardarJson_Click;

            ToolStripMenuItem guardarBd = new("Guardar JSON en base de datos");
            guardarBd.Click += MenuGuardarJsonEnBd_Click;

            menuArchivo.DropDownItems.Add(new ToolStripSeparator());
            menuArchivo.DropDownItems.AddRange(new ToolStripItem[] { importar, abrir, guardarJson, guardarBd });
        }

        private void ActivarOrigenJson()
        {
            if (_jsonActual is not null && !string.IsNullOrWhiteSpace(_rutaJson))
            {
                SesionTrabajoJson.Activar(_jsonActual, _rutaJson);
            }

            _origen = OrigenDatos.Json;

            if (_menuOrigenBd is not null)
                _menuOrigenBd.Checked = false;

            _filaCatalogoMayor = 0;
            _filaCatalogoAuxiliar = 0;
            _rangoInferiorActual = 0;
            _rangoSuperiorActual = 0;

            dgvPrincipal.Visible = true;
            CargarCatalogoMayor();
        }

        private void MenuImportarCarpetaJson_Click(object? sender, EventArgs e)
        {
            try
            {
                using FolderBrowserDialog carpeta = new()
                {
                    Description = "Seleccione la carpeta heredada (catmay, cataux, DATOS y AUXILIAR) que se convertirá a JSON.",
                    UseDescriptionForTitle = true
                };

                if (carpeta.ShowDialog(this) != DialogResult.OK)
                    return;

                using SaveFileDialog guardar = new()
                {
                    Title = "Guardar JSON de trabajo",
                    Filter = "JSON (*.json)|*.json",
                    FileName = "trabajo.json"
                };

                if (guardar.ShowDialog(this) != DialogResult.OK)
                    return;

                Cursor = Cursors.WaitCursor;

                try
                {
                    _jsonActual = _carpetaJsonService.ImportarCarpeta(carpeta.SelectedPath);
                    _carpetaJsonService.GuardarJson(_jsonActual, guardar.FileName);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }

                _rutaJson = guardar.FileName;
                _jsonModificado = false;

                ActivarOrigenJson();
            }
            catch (Exception ex)
            {
                MostrarError("Error en MenuImportarCarpetaJson_Click.", ex);
            }
        }

        private void MenuAbrirJson_Click(object? sender, EventArgs e)
        {
            try
            {
                using OpenFileDialog abrir = new()
                {
                    Title = "Abrir JSON de trabajo",
                    Filter = "JSON (*.json)|*.json"
                };

                if (abrir.ShowDialog(this) != DialogResult.OK)
                    return;

                _jsonActual = _carpetaJsonService.CargarJson(abrir.FileName);
                _rutaJson = abrir.FileName;
                _jsonModificado = false;

                ActivarOrigenJson();
            }
            catch (Exception ex)
            {
                MostrarError("Error en MenuAbrirJson_Click.", ex);
            }
        }

        private void MenuGuardarJson_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_jsonActual is null)
                {
                    MessageBox.Show(this, "No hay un JSON de trabajo cargado.", "Guardar JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_rutaJson))
                {
                    using SaveFileDialog guardar = new() { Title = "Guardar JSON de trabajo", Filter = "JSON (*.json)|*.json", FileName = "trabajo.json" };

                    if (guardar.ShowDialog(this) != DialogResult.OK)
                        return;

                    _rutaJson = guardar.FileName;
                }

                _carpetaJsonService.GuardarJson(_jsonActual, _rutaJson);
                _jsonModificado = false;

                MessageBox.Show(this, "JSON guardado correctamente.", "Guardar JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarError("Error en MenuGuardarJson_Click.", ex);
            }
        }

        private void MenuGuardarJsonEnBd_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_jsonActual is null)
                {
                    MessageBox.Show(this, "No hay un JSON de trabajo cargado.", "Guardar en base de datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int subcuentas = _jsonActual.Cuentas.Sum(c => c.Subcuentas.Count);

                DialogResult confirmar = MessageBox.Show(this,
                    $"Se agregarán o actualizarán {_jsonActual.Cuentas.Count} cuentas y {subcuentas} subcuentas en la base de datos." + Environment.NewLine +
                    "No se borrará ningún registro existente." + Environment.NewLine + Environment.NewLine + "¿Desea continuar?",
                    "Guardar en base de datos", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmar != DialogResult.Yes)
                    return;

                Cursor = Cursors.WaitCursor;
                ResultadoGuardadoBd resultado;

                try
                {
                    resultado = _carpetaJsonService.GuardarEnBd(_jsonActual, _rutaJson);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }

                MessageBox.Show(this,
                    $"Cuentas nuevas: {resultado.CuentasNuevas}" + Environment.NewLine +
                    $"Cuentas actualizadas: {resultado.CuentasActualizadas}" + Environment.NewLine +
                    $"Subcuentas nuevas: {resultado.SubcuentasNuevas}" + Environment.NewLine +
                    $"Subcuentas actualizadas: {resultado.SubcuentasActualizadas}" + Environment.NewLine +
                    $"Pólizas nuevas: {resultado.PolizasNuevas}" + Environment.NewLine +
                    $"Omitidas (código no numérico): {resultado.Omitidas}",
                    "Guardado en base de datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);

                MessageBox.Show(this, "No se pudo completar la importación a SQL Server. Comprueba que el JSON esté guardado localmente y que todas sus pólizas estén correctamente cuadradas. No se ha confirmado la importación.",
                    "Guardar en Base de Datos", MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        /// <summary>F2: edita la fila actual dentro del JSON de trabajo (nunca los archivos originales).</summary>
        private void EditarFilaJson()
        {
            try
            {
                if (_jsonActual is null || dgvPrincipal.CurrentRow?.DataBoundItem is null)
                    return;

                int filaActual = dgvPrincipal.CurrentRow.Index;

                switch (_nivelActual)
                {
                    case NivelNavegacion.CatalogoMayor:
                        {
                            if (dgvPrincipal.CurrentRow.DataBoundItem is not CuentaMayor mayor)
                                return;

                            CuentaJson cuenta = _jsonActual.Cuentas[mayor.RangoInferior - 1];

                            if (!PedirValores("Editar cuenta", ["Nombre", "Saldo"], [cuenta.Nombre, cuenta.Saldo.ToString("0.00", CultureInfo.InvariantCulture)], out string[] valores))
                                return;

                            if (!TryParseImporte(valores[1], out decimal saldo))
                                return;

                            cuenta.Nombre = valores[0].Trim();
                            cuenta.Saldo = saldo;
                            break;
                        }

                    case NivelNavegacion.CatalogoAuxiliar:
                        {
                            if (dgvPrincipal.CurrentRow.DataBoundItem is not CuentaAuxiliar aux)
                                return;

                            SubcuentaJson sub = _jsonActual.Cuentas[_rangoInferiorActual - 1].Subcuentas[aux.Guia - 1];

                            if (!PedirValores("Editar subcuenta", ["Nombre", "Saldo"], [sub.Nombre, sub.Saldo.ToString("0.00", CultureInfo.InvariantCulture)], out string[] valores))
                                return;

                            if (!TryParseImporte(valores[1], out decimal saldo))
                                return;

                            sub.Nombre = valores[0].Trim();
                            sub.Saldo = saldo;
                            break;
                        }

                    case NivelNavegacion.Auxiliar:
                        {
                            MovimientoJson mov = _jsonActual.Cuentas[_rangoInferiorActual - 1].Subcuentas[_guiaJsonActual - 1].Movimientos[filaActual];

                            if (!PedirValores("Editar movimiento", ["Concepto", "Importe (+ debe / - haber)"], [mov.Concepto, mov.Importe.ToString("0.00", CultureInfo.InvariantCulture)], out string[] valores))
                                return;

                            if (!TryParseImporte(valores[1], out decimal importe))
                                return;

                            mov.Concepto = valores[0].Trim();
                            mov.Importe = importe;
                            break;
                        }
                }

                _jsonModificado = true;
                RecargarVistaJson();
                RestaurarFila(filaActual);
            }
            catch (Exception ex)
            {
                MostrarError("Error en EditarFilaJson.", ex);
            }
        }

        private void RecargarVistaJson()
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
                    CargarMovimientosAuxiliar(_guiaJsonActual);
                    break;
            }
        }

        private bool TryParseImporte(string texto, out decimal valor)
        {
            if (decimal.TryParse(texto.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
                return true;

            MessageBox.Show(this, "El importe capturado no es un número válido.", "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private bool PedirValores(string titulo, string[] etiquetas, string[] valoresIniciales, out string[] resultado)
        {
            resultado = [];

            using Form dialogo = new()
            {
                Text = titulo,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(460, 20 + (etiquetas.Length * 34) + 60)
            };

            TextBox[] cajas = new TextBox[etiquetas.Length];

            for (int i = 0; i < etiquetas.Length; i++)
            {
                int y = 16 + (i * 34);

                dialogo.Controls.Add(new Label { Text = etiquetas[i], Left = 12, Top = y + 3, Width = 140 });

                cajas[i] = new TextBox { Left = 156, Top = y, Width = 290, Text = valoresIniciales[i] };
                dialogo.Controls.Add(cajas[i]);
            }

            int yBotones = 16 + (etiquetas.Length * 34) + 6;

            Button aceptar = new() { Text = "Aceptar", Left = 290, Top = yBotones, Width = 75, DialogResult = DialogResult.OK };
            Button cancelar = new() { Text = "Cancelar", Left = 371, Top = yBotones, Width = 75, DialogResult = DialogResult.Cancel };

            dialogo.Controls.Add(aceptar);
            dialogo.Controls.Add(cancelar);
            dialogo.AcceptButton = aceptar;
            dialogo.CancelButton = cancelar;

            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return false;

            resultado = cajas.Select(c => c.Text).ToArray();
            return true;
        }
    }
}

