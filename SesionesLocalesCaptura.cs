
using CapturaDePolizas_2026_NET8.Context;
using CapturaDePolizas_2026_NET8.Models;
using CapturaDePolizas_2026_NET8.Services;
using GaCostos.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CapturaDePolizas_2026_NET8;

public partial class FormCaptura
{
    private readonly GestorSesionesLocales _gestorSesiones = new();
    private List<SesionLocal> _sesionesGuardadas = new();
    private SesionLocal? _sesionActiva;
    private ToolStripMenuItem? _menuSesiones;
    private bool _restaurandoSesion;

    private void InicializarSesionesLocales()
    {
        _menuSesiones = new ToolStripMenuItem("Sesiones locales");
        _menuSesiones.DropDownOpening += (_, _) => ReconstruirMenuSesiones();

        menuStrip.Items.Insert(Math.Min(2, menuStrip.Items.Count), _menuSesiones);

        btnGuardar.Text = "Guardar en BD";
        btnGuardar.Width = 108;

        dgvMovimientos.AllowUserToDeleteRows = false;

        var menuFila = new ContextMenuStrip();
        menuFila.Items.Add("Eliminar movimiento del borrador", null, (_, _) => EliminarMovimientoBorrador());

        dgvMovimientos.ContextMenuStrip = menuFila;

        dgvMovimientos.KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                EliminarMovimientoBorrador();
            }
        };

        dgvMovimientos.CellEndEdit += (_, _) => ActualizarEstadoBotonGuardar();

        EventHandler cambio = (_, _) =>
        {
            if (!_restaurandoSesion)
                ActualizarEstadoBotonGuardar();
        };

        txtPolizaNo.TextChanged += cambio;
        txtConceptoPoliza.TextChanged += cambio;
        txtConceptoCheque.TextChanged += cambio;
        txtBeneficiario.TextChanged += cambio;
        txtMonto.TextChanged += cambio;
        txtNumeroCheque.TextChanged += cambio;
        txtRFC.TextChanged += cambio;
        txtFolios.TextChanged += cambio;
        chkVarios.CheckedChanged += cambio;

        Shown += async (_, _) =>
        {
            try
            {
                await RecargarListaSesionesAsync();
            }
            catch
            {
                MostrarErrorSesiones();
            }

            _ = Task.Run(() => _gestorSesiones.GenerarRespaldoSemanalSiCorrespondeAsync());
        };
    }

    private void ReconstruirMenuSesiones()
    {
        if (_menuSesiones is null)
            return;

        _menuSesiones.DropDownItems.Clear();

        bool enModoBd = !SesionTrabajoJson.UsarJson;

        void AgregarOpcion(string texto, EventHandler accion, bool habilitado = true)
        {
            var item = new ToolStripMenuItem(texto)
            {
                Enabled = enModoBd && habilitado
            };

            item.Click += accion;
            _menuSesiones.DropDownItems.Add(item);
        }

        AgregarOpcion("Guardar borrador local...", async (_, _) => await GuardarBorradorLocalAsync());
        AgregarOpcion("Abrir sesión guardada...", async (_, _) => await AbrirSesionLocalAsync());
        AgregarOpcion("Renombrar sesión...", async (_, _) => await RenombrarSesionLocalAsync());
        AgregarOpcion("Eliminar / limpiar sesión...", async (_, _) => await EliminarSesionLocalAsync());
        AgregarOpcion("Nuevo borrador", (_, _) => CrearNuevoBorrador());
        AgregarOpcion("Actualizar folio del borrador según SQL...", (_, _) => ActualizarFolioBorrador(), _sesionActiva?.Estado == GestorSesionesLocales.Borrador);

        if (_sesionesGuardadas.Any(
            s => s.Estado == GestorSesionesLocales.EnProceso))
        {
            AgregarOpcion("Revisar contabilización pendiente...", async (_, _) => await RevisarSesionPendienteAsync());
        }

        _menuSesiones.DropDownItems.Add(new ToolStripSeparator());

        for (int id = 1; id <= GestorSesionesLocales.MaximoSesiones; id++)
        {
            int seleccionado = id;
            SesionLocal? sesion = _sesionesGuardadas.FirstOrDefault(s => s.Id == id);

            var item = new ToolStripMenuItem(DescripcionSesion(id, sesion))
            {
                Enabled = enModoBd && sesion?.Estado == GestorSesionesLocales.Borrador,
                Checked = sesion is not null && _sesionActiva?.Id == id
            };

            item.Click += async (_, _) => await AbrirSesionPorIdAsync(seleccionado);
            _menuSesiones.DropDownItems.Add(item);
        }
    }

    private void ActualizarIndicadorSesion()
    {
        if (_menuSesiones is not null)
        {
            _menuSesiones.Text = _sesionActiva is null ? "Sesiones locales" : $"Sesión {_sesionActiva.Id}: {_sesionActiva.Nombre}";
        }

        if (btnGuardar is not null)
        {
            btnGuardar.Text = SesionTrabajoJson.UsarJson ? "Guardar JSON" : "Guardar en BD";
        }
    }

    private static string DescripcionSesion(int id, SesionLocal? s)
    {
        if (s is null)
            return $"{id}. (Vacía)";

        string estado = s.Estado switch
        {
            GestorSesionesLocales.Contabilizada => " [Contabilizada]",
            GestorSesionesLocales.EnProceso => " [Pendiente de revisión]",
            _ => string.Empty
        };

        string fecha = DateTime.TryParseExact(s.FechaActualizacion, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d)
            ? d.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : "Sin fecha";

        return $"{id}. {s.Nombre} — {fecha}{estado}";
    }

    private async Task RecargarListaSesionesAsync()
    {
        _sesionesGuardadas = await _gestorSesiones.ListarAsync();

        if (_sesionActiva is not null)
        {
            _sesionActiva = _sesionesGuardadas.FirstOrDefault(s => s.Id == _sesionActiva.Id);
        }

        ActualizarIndicadorSesion();
        ActualizarEstadoBotonGuardar();
    }

    private static void MostrarErrorSesiones()
    {
        MessageBox.Show("No fue posible acceder a las sesiones locales. Verifica los permisos del programa y el almacenamiento.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private CapturaSesionLocal CapturarEstadoSesion()
    {
        var captura = new CapturaSesionLocal
        {
            TipoCaptura = ObtenerTipoCapturaActual() == TipoCaptura.Cheque ? (byte)0 : (byte)1,

            Folio = txtPolizaNo.Text,
            ConceptoPoliza = txtConceptoPoliza.Text,
            ConceptoCheque = txtConceptoCheque.Text,
            Beneficiario = txtBeneficiario.Text,
            MontoCheque = txtMonto.Text,
            NumeroCheque = txtNumeroCheque.Text,
            Rfc = txtRFC.Text,
            FoliosFiscales = txtFolios.Text,
            Varios = chkVarios.Checked,
            ChequeImpreso = _chequeImpreso
        };

        foreach (DataGridViewRow row in dgvMovimientos.Rows)
        {
            var celdas = new Dictionary<string, string>();

            foreach (DataGridViewColumn col in dgvMovimientos.Columns)
            {
                celdas[col.Name] = row.Cells[col.Name].Value?.ToString() ?? string.Empty;
            }

            captura.Filas.Add(celdas);
        }

        return captura;
    }

    private bool CapturaIgualAlUltimoGuardado()
    {
        if (_restaurandoSesion || _sesionActiva?.Estado != GestorSesionesLocales.Borrador)
        {
            return false;
        }

        return string.Equals(_sesionActiva.Huella, GestorSesionesLocales.CalcularHuella(CapturarEstadoSesion()), StringComparison.Ordinal);
    }

    private bool PermiteGuardarSegunSesion()
    {
        return SesionTrabajoJson.UsarJson || CapturaIgualAlUltimoGuardado();
    }

    private bool ImportesConCentavosValidos()
    {
        foreach (DataGridViewRow fila in dgvMovimientos.Rows)
        {
            foreach (string columna in new[] { "Parcial", "Debe", "Haber" })
            {
                string texto = ObtenerTextoCelda(fila, columna).Trim();

                if (texto.Length == 0)
                    continue;

                if (!TryParseDecimal(texto, out decimal valor) || decimal.Round(valor, 2) != valor)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool HayCambiosPendientes()
    {
        if (_sesionActiva is not null)
            return !CapturaIgualAlUltimoGuardado();

        return FormularioTieneDatos() || !string.IsNullOrWhiteSpace(ObtenerConceptoActivo()) || !string.IsNullOrWhiteSpace(txtBeneficiario.Text) || !string.IsNullOrWhiteSpace(txtMonto.Text);
    }

    private bool ConfirmarDescartarCambios()
    {
        if (!HayCambiosPendientes())
            return true;

        return MessageBox.Show(this, "Hay cambios sin guardar localmente. ¿Deseas descartarlos y continuar?", "Sesiones locales", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    private static string? SolicitarNombreSesion(IWin32Window propietario, string titulo, string nombreActual)
    {
        using Form dlg = new()
        {
            Text = titulo,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            ClientSize = new Size(440, 118),
            ShowInTaskbar = false
        };

        Label etiqueta = new()
        {
            Text = "Nombre de la sesión:",
            Location = new Point(12, 12),
            AutoSize = true
        };

        TextBox entrada = new()
        {
            Location = new Point(12, 37),
            Width = 416,
            MaxLength = 100,
            Text = nombreActual
        };

        Button aceptar = new()
        {
            Text = "Aceptar",
            DialogResult = DialogResult.OK,
            Location = new Point(254, 78),
            Size = new Size(84, 28)
        };

        Button cancelar = new()
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(344, 78),
            Size = new Size(84, 28)
        };

        dlg.Controls.AddRange(new Control[]
        {
            etiqueta, entrada, aceptar, cancelar
        });

        dlg.AcceptButton = aceptar;
        dlg.CancelButton = cancelar;

        if (dlg.ShowDialog(propietario) != DialogResult.OK)
            return null;

        string nombre = entrada.Text.Trim();

        if (nombre.Length > 0)
            return nombre;

        MessageBox.Show(propietario, "Debes asignar un nombre a la sesión.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        return null;
    }

    private int? SeleccionarEspacioSesion(string titulo)
    {
        using Form dlg = new()
        {
            Text = titulo,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ShowInTaskbar = false,
            MinimizeBox = false,
            MaximizeBox = false,
            ClientSize = new Size(590, 240)
        };

        ListBox lista = new()
        {
            Location = new Point(12, 12),
            Size = new Size(566, 166),
            HorizontalScrollbar = true
        };

        for (int id = 1; id <= GestorSesionesLocales.MaximoSesiones; id++)
        {
            lista.Items.Add(DescripcionSesion(id, _sesionesGuardadas.FirstOrDefault(s => s.Id == id)));
        }

        lista.SelectedIndex = _sesionActiva is null ? 0 : _sesionActiva.Id - 1;

        Button aceptar = new()
        {
            Text = "Seleccionar",
            Location = new Point(382, 195),
            Size = new Size(90, 30)
        };

        Button cancelar = new()
        {
            Text = "Cancelar",
            DialogResult = DialogResult.Cancel,
            Location = new Point(482, 195),
            Size = new Size(96, 30)
        };

        aceptar.Click += (_, _) =>
        {
            if (lista.SelectedIndex >= 0)
                dlg.DialogResult = DialogResult.OK;
        };

        dlg.Controls.AddRange(new Control[]
        {
            lista, aceptar, cancelar
        });

        dlg.AcceptButton = aceptar;
        dlg.CancelButton = cancelar;

        return dlg.ShowDialog(this) == DialogResult.OK ? lista.SelectedIndex + 1 : null;
    }

    private bool TerminarEdicionGrid()
    {
        if (!dgvMovimientos.IsCurrentCellInEditMode)
            return true;

        if (dgvMovimientos.EndEdit())
            return true;

        MessageBox.Show(this, "Concluye la edición de la celda antes de continuar.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        return false;
    }

    private async Task GuardarBorradorLocalAsync()
    {
        if (SesionTrabajoJson.UsarJson)
            return;

        if (!TerminarEdicionGrid())
            return;

        try
        {
            await RecargarListaSesionesAsync();

            int? id = SeleccionarEspacioSesion("Guardar borrador en una sesión");

            if (!id.HasValue)
                return;

            SesionLocal? anterior = _sesionesGuardadas.FirstOrDefault(x => x.Id == id.Value);

            if (anterior is not null)
            {
                if (MessageBox.Show(this, "¿Deseas sobreescribir la sesión seleccionada?", "Confirmar sobreescritura", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            string? nombre = anterior is null ? SolicitarNombreSesion(this, "Nombre del borrador", string.Empty) : anterior.Nombre;

            if (nombre is null)
                return;

            if (!int.TryParse(txtPolizaNo.Text, out int folio))
            {
                MessageBox.Show(this, "El número de póliza debe ser válido.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            CalcularTotales(out decimal debe, out decimal haber, out _);
            PolizaModel modelo = CrearPolizaModel(ObtenerTipoCapturaActual(), folio, debe, haber);
            AgregarMovimientosAlModelo(modelo, ObtenerTipoCapturaActual());
            SesionLocal guardada = await _gestorSesiones.GuardarAsync(id.Value, nombre, CapturarEstadoSesion(), modelo, anterior is not null);

            _sesionActiva = guardada;

            await RecargarListaSesionesAsync();
            MessageBox.Show(this, "Borrador guardado localmente.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch
        {
            MostrarErrorSesiones();
        }
    }

    private async Task AbrirSesionLocalAsync()
    {
        if (SesionTrabajoJson.UsarJson)
            return;

        try
        {
            await RecargarListaSesionesAsync();

            int? id = SeleccionarEspacioSesion("Abrir una sesión guardada");

            if (id.HasValue)
                await AbrirSesionPorIdAsync(id.Value);
        }
        catch
        {
            MostrarErrorSesiones();
        }
    }

    private async Task AbrirSesionPorIdAsync(int id)
    {
        if (SesionTrabajoJson.UsarJson)
            return;

        try
        {
            SesionLocal? sesion = await _gestorSesiones.ObtenerAsync(id);

            if (sesion is null)
            {
                MessageBox.Show(this, "Este espacio no tiene un borrador guardado.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (sesion.Estado != GestorSesionesLocales.Borrador)
            {
                MessageBox.Show(this, "La sesión ya fue contabilizada o requiere revisión.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sesion.Huella != GestorSesionesLocales.CalcularHuella(sesion.Captura) || sesion.HuellaModelo != GestorSesionesLocales.CalcularHuella(sesion.Poliza))
            {
                throw new InvalidOperationException("La sesión está incompleta o modificada.");
            }

            if (!ConfirmarDescartarCambios())
                return;

            RestaurarSesionEnPantalla(sesion);
            await RecargarListaSesionesAsync();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch
        {
            MostrarErrorSesiones();
        }
    }

    private void RestaurarSesionEnPantalla(SesionLocal sesion)
    {
        _restaurandoSesion = true;
        dgvMovimientos.CellValueChanged -= DgvMovimientos_CellValueChanged;

        try
        {
            var datos = sesion.Captura;

            RefrescarCaptura();

            ConfigurarPantalla(datos.TipoCaptura == 0 ? TipoCaptura.Cheque : TipoCaptura.Poliza);

            txtPolizaNo.Text = datos.Folio;
            txtConceptoPoliza.Text = datos.ConceptoPoliza;
            txtConceptoCheque.Text = datos.ConceptoCheque;
            txtBeneficiario.Text = datos.Beneficiario;
            txtMonto.Text = datos.MontoCheque;
            txtNumeroCheque.Text = datos.NumeroCheque;
            txtRFC.Text = datos.Rfc;
            txtFolios.Text = datos.FoliosFiscales;
            chkVarios.Checked = datos.Varios;

            _activeCell = null;
            dgvMovimientos.Rows.Clear();

            foreach (Dictionary<string, string> fila in datos.Filas)
            {
                int indice = dgvMovimientos.Rows.Add();

                foreach (var par in fila)
                {
                    if (dgvMovimientos.Columns.Contains(par.Key))
                    {
                        dgvMovimientos[par.Key, indice].Value = par.Value;
                    }
                }
            }

            if (dgvMovimientos.Rows.Count == 0)
                CrearFilasIniciales();

            _chequeImpreso = datos.ChequeImpreso;

            _undoStack.Clear();
            _redoStack.Clear();

            _lastState = SnapshotGridState();

            ActualizarBotonesUndoRedo();

            _sesionActiva = sesion;
        }
        finally
        {
            dgvMovimientos.CellValueChanged += DgvMovimientos_CellValueChanged;
            _restaurandoSesion = false;
        }

        ActualizarIndicadorSesion();
        ActualizarEstadoBotonGuardar();
    }

    private async Task RenombrarSesionLocalAsync()
    {
        if (SesionTrabajoJson.UsarJson)
            return;

        try
        {
            await RecargarListaSesionesAsync();

            int? id = SeleccionarEspacioSesion("Renombrar sesión");

            if (!id.HasValue)
                return;

            SesionLocal? sesion = _sesionesGuardadas.FirstOrDefault(s => s.Id == id.Value);

            if (sesion is null)
            {
                MessageBox.Show(this, "Selecciona una sesión ocupada.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string? nombre = SolicitarNombreSesion(this, "Renombrar sesión", sesion.Nombre);

            if (nombre is null)
                return;

            await _gestorSesiones.RenombrarAsync(id.Value, nombre);
            await RecargarListaSesionesAsync();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch
        {
            MostrarErrorSesiones();
        }
    }

    private async Task EliminarSesionLocalAsync()
    {
        if (SesionTrabajoJson.UsarJson)
            return;

        try
        {
            await RecargarListaSesionesAsync();

            int? id = SeleccionarEspacioSesion("Eliminar / limpiar sesión");

            if (!id.HasValue)
                return;

            if (_sesionesGuardadas.All(s => s.Id != id.Value))
            {
                MessageBox.Show(this, "La sesión seleccionada ya está vacía.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "¿Deseas eliminar la sesión seleccionada y su borrador?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            await _gestorSesiones.EliminarAsync(id.Value);

            if (_sesionActiva?.Id == id.Value)
            {
                TipoCaptura tipo = ObtenerTipoCapturaActual();
                RefrescarCaptura();
                ConfigurarPantalla(tipo);
            }

            await RecargarListaSesionesAsync();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch
        {
            MostrarErrorSesiones();
        }
    }

    private void CrearNuevoBorrador()
    {
        if (SesionTrabajoJson.UsarJson || !ConfirmarDescartarCambios())
            return;

        TipoCaptura tipo = ObtenerTipoCapturaActual();

        RefrescarCaptura();
        ConfigurarPantalla(tipo);
        ActualizarIndicadorSesion();
    }

    private void ActualizarFolioBorrador()
    {
        if (SesionTrabajoJson.UsarJson || _sesionActiva?.Estado != GestorSesionesLocales.Borrador)
            return;

        if (MessageBox.Show(this, "Se asignará el siguiente folio disponible en SQL Server. Después deberás volver a guardar localmente el borrador. ¿Deseas continuar?",
            "Actualizar folio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            txtPolizaNo.Text = _catalogoRepository.ObtenerSiguienteFolio().ToString();

            if (ObtenerTipoCapturaActual() == TipoCaptura.Cheque)
            {
                txtNumeroCheque.Text = _catalogoRepository.ObtenerSiguienteNumeroCheque();
                _chequeImpreso = false;
            }

            ActualizarEstadoBotonGuardar();
        }
        catch
        {
            MessageBox.Show(this, "No se pudo consultar el siguiente folio en SQL Server.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LimpiarVinculoSesion()
    {
        if (_restaurandoSesion)
            return;

        _sesionActiva = null;
        ActualizarIndicadorSesion();
    }

    private bool ValidarSesionAntesDeContabilizar(PolizaModel modelo, out SesionLocal? sesion)
    {
        sesion = null;

        if (SesionTrabajoJson.UsarJson)
            return true;

        if (!TerminarEdicionGrid())
            return false;

        if (_sesionActiva is null || !CapturaIgualAlUltimoGuardado())
        {
            MessageBox.Show(this, "Primero guarda la captura actual en una sesión local. Si cambiaste algo, vuelve a guardar el borrador.", "Guardar en Base de Datos",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        try
        {
            modelo.Fecha = _sesionActiva.Poliza.Fecha;

            string huellaModelo = GestorSesionesLocales.CalcularHuella(modelo);
            string huellaCaptura = GestorSesionesLocales.CalcularHuella(CapturarEstadoSesion());

            if (huellaModelo != _sesionActiva.HuellaModelo)
            {
                throw new InvalidOperationException("El borrador ya no coincide con sus movimientos guardados. Vuelve a guardarlo localmente.");
            }

            sesion = _gestorSesiones.ValidarGuardadoAsync(_sesionActiva.Id, huellaCaptura, huellaModelo).GetAwaiter().GetResult();

            decimal sumaDebe = modelo.Movimientos.Sum(m => m.Debe);
            decimal sumaHaber = modelo.Movimientos.Sum(m => m.Haber);

            if (modelo.Movimientos.Count == 0 || modelo.TotalDebe != sumaDebe || modelo.TotalHaber != sumaHaber || modelo.TotalDebe <= 0 ||
                modelo.TotalDebe != modelo.TotalHaber || modelo.Movimientos.Any(m => m.CuentaId <= 0 || m.Debe < 0 || m.Haber < 0 || (m.Debe > 0 && m.Haber > 0) ||
                decimal.Round(m.Debe, 2) != m.Debe || decimal.Round(m.Haber, 2) != m.Haber || decimal.Round(m.Parcial, 2) != m.Parcial))
            {
                throw new InvalidOperationException("Los movimientos del borrador no son válidos o no cuadran al centavo.");
            }

            using (var db = new EmpresaDbContext())
            {
                if (db.Polizas.AsNoTracking().Any(p => p.Folio == modelo.Folio))
                {
                    throw new InvalidOperationException("Este folio ya existe en SQL Server. No se ha contabilizado el borrador.");
                }
            }

            if (!ImportesConCentavosValidos())
            {
                throw new InvalidOperationException("Hay importes inválidos o con más de dos decimales.");
            }

            return true;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Guardar en Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        catch
        {
            MostrarErrorSesiones();
            return false;
        }
    }

    private void MarcarSesionEnProceso(SesionLocal? sesion)
    {
        if (sesion is null)
            return;

        _gestorSesiones.MarcarEnProcesoAsync(sesion.Id, sesion.Huella).GetAwaiter().GetResult();

        if (_sesionActiva?.Id == sesion.Id)
        {
            _sesionActiva.Estado = GestorSesionesLocales.EnProceso;
            ActualizarEstadoBotonGuardar();
        }
    }

    private void MarcarSesionContabilizada(SesionLocal? sesion)
    {
        if (sesion is null)
            return;

        _gestorSesiones.MarcarContabilizadaAsync(sesion.Id, sesion.Huella).GetAwaiter().GetResult();
        _sesionActiva = null;

        ActualizarIndicadorSesion();
        _ = RecargarListaSesionesSinInterrumpirAsync();
    }

    private async Task RevisarSesionPendienteAsync()
    {
        try
        {
            await RecargarListaSesionesAsync();

            int? id = SeleccionarEspacioSesion("Revisar contabilización pendiente");

            if (!id.HasValue)
                return;

            SesionLocal? s = _sesionesGuardadas.FirstOrDefault(x => x.Id == id.Value);

            if (s?.Estado != GestorSesionesLocales.EnProceso)
            {
                MessageBox.Show(this, "Selecciona una sesión pendiente de revisión.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var db = new EmpresaDbContext();

            var existentes = await db.Polizas.AsNoTracking().Where(p => p.Folio == s.Poliza.Folio).Select(p => new
            {
                p.Id,
                p.Folio,
                p.Fecha,
                p.TipoCaptura,
                p.Concepto,
                p.TotalDebe,
                p.TotalHaber,
                p.NumeroCheque
            }).ToListAsync();

            bool coincidencia = false;

            foreach (var p in existentes)
            {
                if (p.Fecha.Date != s.Poliza.Fecha.Date || p.TipoCaptura != s.Poliza.TipoCaptura || p.Concepto != s.Poliza.Concepto || p.TotalDebe != s.Poliza.TotalDebe ||
                    p.TotalHaber != s.Poliza.TotalHaber || (p.NumeroCheque ?? string.Empty) != s.Poliza.NumeroCheque)
                {
                    continue;
                }

                var movimientosSql = await db.Movimientos.AsNoTracking().Where(m => m.PolizaId == p.Id).OrderBy(m => m.Id).ToListAsync();

                if (movimientosSql.Count != s.Poliza.Movimientos.Count)
                    continue;

                coincidencia = movimientosSql.Zip(s.Poliza.Movimientos).All(par => par.First.CuentaId == par.Second.CuentaId && par.First.SubcuentaId == par.Second.SubcuentaId &&
                par.First.FolioFiscal == par.Second.FolioFiscal && par.First.Parcial == par.Second.Parcial && par.First.Debe == par.Second.Debe && par.First.Haber == par.Second.Haber &&
                par.First.Redaccion == par.Second.Redaccion && par.First.Beneficiario == par.Second.Beneficiario && par.First.PerteneceConcepto == par.Second.PerteneceConcepto);

                if (coincidencia)
                    break;
            }

            if (coincidencia)
            {
                await _gestorSesiones.MarcarContabilizadaAsync(s.Id, s.Huella);

                MessageBox.Show(this, "La póliza ya aparece contabilizada en SQL Server.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (existentes.Count > 0)
            {
                MessageBox.Show(this, "El folio existe en SQL Server, pero no coincide con el borrador. Se mantiene pendiente para evitar duplicaciones.", "Sesiones locales",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (MessageBox.Show(this, "No se encontró este folio en SQL Server. ¿Deseas recuperar el borrador para corregirlo y volver a guardarlo?", "Sesiones locales",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                await _gestorSesiones.RecuperarBorradorRevisadoAsync(s.Id, s.Huella);
            }

            await RecargarListaSesionesAsync();
        }
        catch
        {
            MessageBox.Show(this, "No se pudo verificar el estado en SQL Server. La sesión permanece protegida.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task RecargarListaSesionesSinInterrumpirAsync()
    {
        try
        {
            await RecargarListaSesionesAsync();
        }
        catch
        {
        }
    }


    private void EliminarMovimientoBorrador()
    {
        if (SesionTrabajoJson.UsarJson || _restaurandoSesion)
            return;

        // Una sesión contabilizada o pendiente nunca permite borrar.
        if (_sesionActiva is not null && _sesionActiva.Estado != GestorSesionesLocales.Borrador)
        {
            MessageBox.Show(this, "Esta sesión no permite eliminar movimientos.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TerminarEdicionGrid() || dgvMovimientos.CurrentRow is null)
            return;

        int primero = dgvMovimientos.CurrentRow.Index;

        if (primero < 0 || primero >= dgvMovimientos.Rows.Count)
            return;

        DataGridViewRow fila = dgvMovimientos.Rows[primero];

        bool esCuenta = !string.IsNullOrWhiteSpace(ObtenerTextoCelda(fila, "Cuenta"));
        bool esSubcuenta = !string.IsNullOrWhiteSpace(ObtenerTextoCelda(fila, "SubCta"));

        // No permitir eliminar renglones vacíos.
        if (!esCuenta && !esSubcuenta)
            return;

        int padre = esSubcuenta && !esCuenta ? ObtenerFilaCuentaPadre(primero) : -1;

        // No permitir movimientos auxiliares huérfanos.
        if (esSubcuenta && !esCuenta && padre < 0)
        {
            MessageBox.Show(this, "El movimiento no tiene una cuenta padre válida.", "Sesiones locales", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int ultimo = primero;

        // Si se elimina una cuenta, incluir todas sus subcuentas, sin afectar la siguiente cuenta del borrador.
        if (esCuenta)
        {
            for (int i = primero + 1; i < dgvMovimientos.Rows.Count; i++)
            {
                DataGridViewRow siguiente = dgvMovimientos.Rows[i];

                string cuentaSiguiente = ObtenerTextoCelda(siguiente, "Cuenta");

                if (!string.IsNullOrWhiteSpace(cuentaSiguiente))
                    break;

                string subcuentaSiguiente = ObtenerTextoCelda(siguiente, "SubCta");

                if (!string.IsNullOrWhiteSpace(subcuentaSiguiente))
                    ultimo = i;
            }
        }

        string pregunta = esCuenta ? "¿Deseas eliminar esta cuenta y todos sus movimientos del borrador?" : "¿Deseas eliminar el movimiento seleccionado del borrador?";

        if (MessageBox.Show(this, pregunta, "Eliminar movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        GuardarEstadoParaUndo();
        _isUndoing = true;

        try
        {
            _activeCell = null;

            for (int i = ultimo; i >= primero; i--) dgvMovimientos.Rows.RemoveAt(i);

            if (dgvMovimientos.Rows.Count == 0)
                CrearFilasIniciales();

            // Recalcular la cuenta padre cuando se elimina únicamente uno de sus movimientos auxiliares.
            if (padre >= 0 && padre < dgvMovimientos.Rows.Count)
            {
                bool quedanSubcuentas = false;

                for (int i = padre + 1; i < dgvMovimientos.Rows.Count; i++)
                {
                    DataGridViewRow actual = dgvMovimientos.Rows[i];

                    if (!string.IsNullOrWhiteSpace(ObtenerTextoCelda(actual, "Cuenta")))
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(ObtenerTextoCelda(actual, "SubCta")))
                    {
                        quedanSubcuentas = true;
                    }
                }

                if (quedanSubcuentas)
                {
                    RecalcularDebeHaberCuenta(padre, 0m, -1);
                }
                else
                {
                    dgvMovimientos["Debe", padre].Value = string.Empty;
                    dgvMovimientos["Haber", padre].Value = string.Empty;
                }
            }

            _lastState = SnapshotGridState();
        }
        finally
        {
            _isUndoing = false;
        }

        ActualizarBotonesUndoRedo();
        ActualizarEstadoBotonGuardar();
    }
}