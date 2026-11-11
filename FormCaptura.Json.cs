using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GaCostos.Models;
using GaCostos.Services;

namespace CapturaDePolizas_2026_NET8;

public partial class FormCaptura
{
    private readonly CarpetaJsonService _carpetaJsonService = new();
    private ToolStripMenuItem? _mnuOrigenBd;
    private ToolStripMenuItem? _mnuOrigenJsonActual;

    private void InicializarMenuOrigenDatos()
    {
        try
        {
            ToolStripMenuItem mnuDatos = new("Origen de Datos");

            _mnuOrigenBd = new ToolStripMenuItem("Usar Base de Datos")
            {
                Checked = !SesionTrabajoJson.UsarJson
            };
            _mnuOrigenBd.Click += MnuUsarBaseDatos_Click;

            _mnuOrigenJsonActual = new ToolStripMenuItem("Usar JSON cargado")
            {
                Checked = SesionTrabajoJson.UsarJson,
                Enabled = SesionTrabajoJson.Actual is not null
            };
            _mnuOrigenJsonActual.Click += MnuUsarJsonActual_Click;

            ToolStripMenuItem mnuImportarCarpeta = new("Importar carpeta a JSON y usarla...");
            mnuImportarCarpeta.Click += MnuImportarCarpetaJson_Click;

            ToolStripMenuItem mnuAbrirJson = new("Abrir archivo JSON y usarlo...");
            mnuAbrirJson.Click += MnuAbrirArchivoJson_Click;

            ToolStripMenuItem mnuGuardarJson = new("Guardar cambios en archivo JSON");
            mnuGuardarJson.Click += MnuGuardarArchivoJson_Click;

            ToolStripMenuItem mnuVolcarBd = new("Guardar JSON actual en Base de Datos...");
            mnuVolcarBd.Click += MnuGuardarJsonEnBd_Click;

            mnuDatos.DropDownItems.AddRange(new ToolStripItem[]
            {
                _mnuOrigenBd,
                _mnuOrigenJsonActual,
                new ToolStripSeparator(),
                mnuImportarCarpeta,
                mnuAbrirJson,
                mnuGuardarJson,
                new ToolStripSeparator(),
                mnuVolcarBd
            });

            menuStrip.Items.Add(mnuDatos);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error en InicializarMenuOrigenDatos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActualizarIndicadorOrigenCaptura()
    {
        try
        {
            if (_mnuOrigenBd is not null)
                _mnuOrigenBd.Checked = !SesionTrabajoJson.UsarJson;

            if (_mnuOrigenJsonActual is not null)
            {
                _mnuOrigenJsonActual.Enabled = SesionTrabajoJson.Actual is not null;
                _mnuOrigenJsonActual.Checked = SesionTrabajoJson.UsarJson;
                _mnuOrigenJsonActual.Text = SesionTrabajoJson.RutaArchivo is not null
                    ? $"Usar JSON ({Path.GetFileName(SesionTrabajoJson.RutaArchivo)})"
                    : "Usar JSON cargado";
            }

            TipoCaptura tipo = ObtenerTipoCapturaActual();
            string baseTitulo = tipo == TipoCaptura.Poliza
                ? "SACMAG DE MEXICO SA CV - Captura de Pólizas"
                : "SACMAG DE MEXICO SA CV - CAPTURA DE CHEQUES";

            Text = SesionTrabajoJson.UsarJson
                ? $"{baseTitulo} [MODO JSON: {Path.GetFileName(SesionTrabajoJson.RutaArchivo ?? "Memoria")}]"
                : baseTitulo;

            CargarSiguienteFolio();
            if (tipo == TipoCaptura.Cheque)
            {
                CargarSiguienteNumeroCheque();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error en ActualizarIndicadorOrigenCaptura: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MnuUsarBaseDatos_Click(object? sender, EventArgs e)
    {
        try
        {
            SesionTrabajoJson.Desactivar();
            ActualizarIndicadorOrigenCaptura();
        }
        catch (Exception ex)
        {
            MostrarError("Error en MnuUsarBaseDatos_Click", ex);
        }
    }

    private void MnuUsarJsonActual_Click(object? sender, EventArgs e)
    {
        try
        {
            if (SesionTrabajoJson.Actual is null)
            {
                MessageBox.Show(this, "Primero importe una carpeta o abra un archivo JSON.", "Origen JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SesionTrabajoJson.UsarJson = true;
            ActualizarIndicadorOrigenCaptura();
        }
        catch (Exception ex)
        {
            MostrarError("Error en MnuUsarJsonActual_Click", ex);
        }
    }

    private void MnuImportarCarpetaJson_Click(object? sender, EventArgs e)
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
            TrabajoJson trabajo;

            try
            {
                trabajo = _carpetaJsonService.ImportarCarpeta(carpeta.SelectedPath);
                _carpetaJsonService.GuardarJson(trabajo, guardar.FileName);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            SesionTrabajoJson.Activar(trabajo, guardar.FileName);
            ActualizarIndicadorOrigenCaptura();

            MessageBox.Show(this, $"JSON cargado correctamente ({trabajo.Cuentas.Count} cuentas, {trabajo.Polizas.Count} pólizas).\nEl modo JSON de trabajo está activo.",
                "Modo JSON activo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MostrarError("Error en MnuImportarCarpetaJson_Click", ex);
        }
    }

    private void MnuAbrirArchivoJson_Click(object? sender, EventArgs e)
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

            TrabajoJson trabajo = _carpetaJsonService.CargarJson(abrir.FileName);
            SesionTrabajoJson.Activar(trabajo, abrir.FileName);
            ActualizarIndicadorOrigenCaptura();

            MessageBox.Show(this,
                $"JSON cargado ({trabajo.Cuentas.Count} cuentas, {trabajo.Polizas.Count} pólizas).\nAhora todo el programa trabaja sobre:\n{abrir.FileName}",
                "Modo JSON activo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MostrarError("Error en MnuAbrirArchivoJson_Click", ex);
        }
    }

    private void MnuGuardarArchivoJson_Click(object? sender, EventArgs e)
    {
        try
        {
            if (SesionTrabajoJson.Actual is null)
            {
                MessageBox.Show(this, "No hay ningún JSON cargado.", "Guardar JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string? ruta = SesionTrabajoJson.RutaArchivo;
            if (string.IsNullOrWhiteSpace(ruta))
            {
                using SaveFileDialog guardar = new()
                {
                    Title = "Guardar JSON de trabajo",
                    Filter = "JSON (*.json)|*.json",
                    FileName = "trabajo.json"
                };

                if (guardar.ShowDialog(this) != DialogResult.OK)
                    return;

                ruta = guardar.FileName;
                SesionTrabajoJson.RutaArchivo = ruta;
            }

            _carpetaJsonService.GuardarJson(SesionTrabajoJson.Actual, ruta);
            SesionTrabajoJson.Modificado = false;

            MessageBox.Show(this, "JSON guardado correctamente.", "Guardar JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MostrarError("Error en MnuGuardarArchivoJson_Click", ex);
        }
    }

    private void MnuGuardarJsonEnBd_Click(object? sender, EventArgs e)
    {
        try
        {
            if (SesionTrabajoJson.Actual is null)
            {
                MessageBox.Show(this, "No hay ningún JSON cargado para guardar en la base de datos.", "Guardar en Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TrabajoJson json = SesionTrabajoJson.Actual;
            int subcuentas = json.Cuentas.Sum(c => c.Subcuentas.Count);

            DialogResult confirmar = MessageBox.Show(this,
                $"Se enviarán a la Base de Datos:\n" +
                $"• {json.Cuentas.Count} cuentas\n" +
                $"• {subcuentas} subcuentas\n" +
                $"• {json.Polizas.Count} pólizas capturadas en el JSON\n\n" +
                "¿Desea continuar?",
                "Guardar JSON en Base de Datos", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes)
                return;

            Cursor = Cursors.WaitCursor;
            ResultadoGuardadoBd resultado;

            try
            {
                resultado = _carpetaJsonService.GuardarEnBd(json, SesionTrabajoJson.RutaArchivo);
            }

            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show(this,
                $"Cuentas nuevas: {resultado.CuentasNuevas}\n" +
                $"Cuentas actualizadas: {resultado.CuentasActualizadas}\n" +
                $"Subcuentas nuevas: {resultado.SubcuentasNuevas}\n" +
                $"Subcuentas actualizadas: {resultado.SubcuentasActualizadas}\n" +
                $"Pólizas nuevas: {resultado.PolizasNuevas}\n" +
                $"Omitidas (código no numérico): {resultado.Omitidas}",
                "Guardado en Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex);

            MessageBox.Show( this, "No se pudo completar el guardado en SQL Server. Verifica que el JSON esté guardado localmente y que todas sus pólizas estén cuadradas. No se ha confirmado la importación.",
                "Guardar en Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

