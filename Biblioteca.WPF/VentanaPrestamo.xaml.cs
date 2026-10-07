using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF
{
    public partial class VentanaPrestamo : Window
    {
        private readonly SocioNegocio _socioNegocio = new SocioNegocio();
        private readonly LibroNegocio _libroNegocio = new LibroNegocio();
        private readonly PrestamoNegocio _prestamoNegocio = new PrestamoNegocio();

        private readonly ObservableCollection<Libro> _carrito = new ObservableCollection<Libro>();

        public VentanaPrestamo()
        {
            InitializeComponent();
            dgCarrito.ItemsSource = _carrito;

            dpFechaPrestamo.SelectedDate = DateTime.Today;
            dpFechaLimite.SelectedDate = DateTime.Today.AddDays(7);

            Loaded += VentanaPrestamo_Loaded;
        }

        private async void VentanaPrestamo_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarSociosAsync();
            await CargarLibrosDisponiblesAsync();
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

        private async Task CargarLibrosDisponiblesAsync(string filtro = "")
        {
            try
            {
                var libros = await _libroNegocio.ListarAsync(filtro);
                // Solo mostramos los que tienen stock y no estan en el carrito
                var disponibles = libros
                    .Where(l => l.Ejemplares > 0 && !_carrito.Any(c => c.Id == l.Id))
                    .ToList();
                dgDisponibles.ItemsSource = disponibles;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar libros: " + ex.Message);
            }
        }

        private async void BtnBuscarLibro_Click(object sender, RoutedEventArgs e)
        {
            await CargarLibrosDisponiblesAsync(txtBuscarLibro.Text.Trim());
        }

        private async void TxtBuscarLibro_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await CargarLibrosDisponiblesAsync(txtBuscarLibro.Text.Trim());
        }

        private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (dgDisponibles.SelectedItem is not Libro libro)
            {
                MostrarError("Selecciona un libro de la lista.");
                return;
            }

            if (_carrito.Count >= 3)
            {
                MostrarError("No puedes agregar mas de 3 libros a un prestamo.");
                return;
            }

            _carrito.Add(libro);
            ActualizarContador();
            await CargarLibrosDisponiblesAsync(txtBuscarLibro.Text.Trim());
        }

        private async void BtnQuitar_Click(object sender, RoutedEventArgs e)
        {
            if (dgCarrito.SelectedItem is not Libro libro)
            {
                MostrarError("Selecciona un libro del carrito para quitarlo.");
                return;
            }

            _carrito.Remove(libro);
            ActualizarContador();
            await CargarLibrosDisponiblesAsync(txtBuscarLibro.Text.Trim());
        }

        private void ActualizarContador()
        {
            lblContador.Text = $"{_carrito.Count} libro(s) seleccionado(s)";
        }

        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboSocio.SelectedValue == null)
                {
                    MostrarError("Debes seleccionar un socio.");
                    return;
                }

                if (!dpFechaPrestamo.SelectedDate.HasValue || !dpFechaLimite.SelectedDate.HasValue)
                {
                    MostrarError("Debes indicar las fechas.");
                    return;
                }

                if (_carrito.Count == 0)
                {
                    MostrarError("Agrega al menos un libro al prestamo.");
                    return;
                }

                var prestamo = new Prestamo
                {
                    SocioId = (int)cboSocio.SelectedValue,
                    FechaPrestamo = dpFechaPrestamo.SelectedDate.Value,
                    FechaLimite = dpFechaLimite.SelectedDate.Value,
                    Estado = "Pendiente",
                    Detalles = _carrito.Select(l => new DetallePrestamo
                    {
                        LibroId = l.Id
                    }).ToList()
                };

                int prestamoId = await _prestamoNegocio.RegistrarAsync(prestamo);

                MostrarExito($"Prestamo #{prestamoId} registrado correctamente.");
                this.Close();
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

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MostrarError(string mensaje)
            => MessageBox.Show(mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void MostrarExito(string mensaje)
            => MessageBox.Show(mensaje, "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}