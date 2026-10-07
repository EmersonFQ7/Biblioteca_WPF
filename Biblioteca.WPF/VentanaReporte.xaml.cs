using System;
using System.Linq;
using System.Windows;
using Biblioteca.Negocio;

namespace Biblioteca.WPF
{
    public partial class VentanaReporte : Window
    {
        private readonly PrestamoNegocio _prestamoNegocio = new PrestamoNegocio();

        public VentanaReporte()
        {
            InitializeComponent();
            // Rango por defecto: ultimo mes
            dpDesde.SelectedDate = DateTime.Today.AddDays(-30);
            dpHasta.SelectedDate = DateTime.Today;
        }

        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dpDesde.SelectedDate.HasValue || !dpHasta.SelectedDate.HasValue)
                {
                    MostrarError("Debes indicar ambas fechas.");
                    return;
                }

                var desde = dpDesde.SelectedDate.Value;
                var hasta = dpHasta.SelectedDate.Value;

                var prestamos = await _prestamoNegocio.ReportePorFechasAsync(desde, hasta);

                // Proyectamos a un objeto anonimo con los titulos concatenados
                var filas = prestamos.Select(p => new
                {
                    p.Id,
                    p.DniSocio,
                    p.NombreSocio,
                    p.FechaPrestamo,
                    p.FechaLimite,
                    p.Estado,
                    TituloLibros = string.Join(", ", p.Detalles.Select(d => d.TituloLibro))
                }).ToList();

                dgReporte.ItemsSource = filas;

                // Resumen
                lblTotalPrestamos.Text = prestamos.Count.ToString();
                lblTotalLibros.Text = prestamos.Sum(p => p.Detalles.Count).ToString();
                lblPendientes.Text = prestamos.Count(p => p.Estado == "Pendiente").ToString();

                if (prestamos.Count == 0)
                    MostrarInfo("No se encontraron prestamos en el intervalo seleccionado.");
            }
            catch (ReglaNegocioException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                MostrarError("Error al generar el reporte: " + ex.Message);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MostrarError(string mensaje)
            => MessageBox.Show(mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void MostrarInfo(string mensaje)
            => MessageBox.Show(mensaje, "Reporte", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}