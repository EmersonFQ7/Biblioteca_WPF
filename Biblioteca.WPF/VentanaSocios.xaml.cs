using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF
{
    public partial class VentanaSocios : Window
    {
        private readonly SocioNegocio _socioNegocio = new SocioNegocio();
        private int _socioIdSeleccionado = 0;

        public VentanaSocios()
        {
            InitializeComponent();
            Loaded += VentanaSocios_Loaded;
        }

        private async void VentanaSocios_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarSociosAsync();
        }

        private async Task CargarSociosAsync(string filtro = "")
        {
            try
            {
                var socios = await _socioNegocio.ListarAsync(filtro);
                dgSocios.ItemsSource = socios;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar socios: " + ex.Message);
            }
        }

        private void DgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSocios.SelectedItem is Socio socio)
            {
                _socioIdSeleccionado = socio.Id;
                txtDni.Text = socio.DNI;
                txtNombre.Text = socio.Nombre;
                txtEmail.Text = socio.Email;
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await CargarSociosAsync(txtBuscar.Text.Trim());
        }

        private async void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await CargarSociosAsync(txtBuscar.Text.Trim());
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var socio = new Socio
                {
                    Id = _socioIdSeleccionado,
                    DNI = txtDni.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Email = txtEmail.Text.Trim()
                };

                if (_socioIdSeleccionado == 0)
                {
                    await _socioNegocio.InsertarAsync(socio);
                    MostrarExito("Socio registrado correctamente.");
                }
                else
                {
                    await _socioNegocio.ActualizarAsync(socio);
                    MostrarExito("Socio actualizado correctamente.");
                }

                Limpiar();
                await CargarSociosAsync();
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

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_socioIdSeleccionado == 0)
            {
                MostrarError("Debe seleccionar un socio de la lista.");
                return;
            }

            var confirmacion = MessageBox.Show(
                $"Esta seguro que desea eliminar al socio '{txtNombre.Text}'?",
                "Confirmar eliminacion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                await _socioNegocio.EliminarAsync(_socioIdSeleccionado);
                MostrarExito("Socio eliminado correctamente.");
                Limpiar();
                await CargarSociosAsync();
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

        private void BtnNuevo_Click(object sender, RoutedEventArgs e) => Limpiar();

        private void Limpiar()
        {
            _socioIdSeleccionado = 0;
            txtDni.Clear();
            txtNombre.Clear();
            txtEmail.Clear();
            dgSocios.SelectedItem = null;
        }

        private void MostrarError(string mensaje)
            => MessageBox.Show(mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void MostrarExito(string mensaje)
            => MessageBox.Show(mensaje, "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}