using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF
{
    public partial class VentanaDevolucion : Window
    {
        private readonly SocioNegocio _socioNegocio = new SocioNegocio();
        private readonly PrestamoNegocio _prestamoNegocio = new PrestamoNegocio();

        private List<Prestamo> _prestamos = new List<Prestamo>();

        public VentanaDevolucion()
        {
            InitializeComponent();
            dpFechaDevolucion.SelectedDate = DateTime.Today;
            Loaded += VentanaDevolucion_Loaded;
        }

        private async void VentanaDevolucion_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarSociosAsync();
        }

        private async Task CargarSociosAsync()
        {
            try
            {
                var socios = await _socioNegocio.ListarAsync();
                cboSocio.ItemsSource = socios;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar socios: " + ex.Message);
            }
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            if (cboSocio.SelectedValue == null)
            {
                MostrarError("Debes seleccionar un socio.");
                return;
            }

            try
            {
                int socioId = (int)cboSocio.SelectedValue;
                _prestamos = await _prestamoNegocio.ListarPendientesPorSocioAsync(socioId);
                dgPrestamos.ItemsSource = _prestamos;
                dgDetalles.ItemsSource = null;
                LimpiarMulta();

                if (_prestamos.Count == 0)
                    MostrarExito("Este socio no tiene prestamos pendientes.");
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar prestamos: " + ex.Message);
            }
        }

        private void DgPrestamos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPrestamos.SelectedItem is Prestamo prestamo)
            {
                dgDetalles.ItemsSource = prestamo.Detalles;
            }
            LimpiarMulta();
        }

        private void DgDetalles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalcularMulta();
        }

        private void DpFechaDevolucion_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalcularMulta();
        }

        private void CalcularMulta()
        {
            if (dgPrestamos.SelectedItem is not Prestamo prestamo ||
                dgDetalles.SelectedItem is not DetallePrestamo detalle ||
                !dpFechaDevolucion.SelectedDate.HasValue)
            {
                LimpiarMulta();
                return;
            }

            DateTime fechaDev = dpFechaDevolucion.SelectedDate.Value;
            DateTime fechaLimite = prestamo.FechaLimite;

            decimal multa = _prestamoNegocio.CalcularMulta(fechaLimite, fechaDev);
            int diasAtraso = (fechaDev.Date - fechaLimite.Date).Days;

            if (diasAtraso <= 0)
            {
                lblInfoRetraso.Text = $"Libro: '{detalle.TituloLibro}' - devolucion a tiempo.";
            }
            else
            {
                lblInfoRetraso.Text = $"Libro: '{detalle.TituloLibro}' - {diasAtraso} dia(s) de retraso " +
                                      $"(S/ 1.50 por dia).";
            }

            lblMulta.Text = $"Multa: S/ {multa:F2}";
        }

        private void LimpiarMulta()
        {
            lblInfoRetraso.Text = "Selecciona un prestamo y un libro para calcular la multa";
            lblMulta.Text = "Multa: S/ 0.00";
        }

        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPrestamos.SelectedItem is not Prestamo prestamo)
                {
                    MostrarError("Selecciona un prestamo.");
                    return;
                }

                if (dgDetalles.SelectedItem is not DetallePrestamo detalle)
                {
                    MostrarError("Selecciona el libro que se esta devolviendo.");
                    return;
                }

                if (!dpFechaDevolucion.SelectedDate.HasValue)
                {
                    MostrarError("Debes indicar la fecha de devolucion.");
                    return;
                }

                DateTime fechaDev = dpFechaDevolucion.SelectedDate.Value;

                decimal multa = await _prestamoNegocio.RegistrarDevolucionAsync(
                    prestamo.Id, detalle.LibroId, fechaDev, prestamo.FechaLimite);

                string msj = multa > 0
                    ? $"Devolucion registrada.\nMulta por retraso: S/ {multa:F2}"
                    : "Devolucion registrada correctamente (sin multa).";

                MostrarExito(msj);

                // Recargar los prestamos del socio
                int socioId = prestamo.SocioId;
                _prestamos = await _prestamoNegocio.ListarPendientesPorSocioAsync(socioId);
                dgPrestamos.ItemsSource = _prestamos;
                dgDetalles.ItemsSource = null;
                LimpiarMulta();
            }
            catch (ReglaNegocioException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                MostrarError("Error inesperado: " + ex.Message);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MostrarError(string mensaje)
            => MessageBox.Show(mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void MostrarExito(string mensaje)
            => MessageBox.Show(mensaje, "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}