using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF
{
    public partial class VentanaLibros : Window
    {
        private readonly LibroNegocio _libroNegocio = new LibroNegocio();
        private readonly AutorNegocio _autorNegocio = new AutorNegocio();
        private int _libroIdSeleccionado = 0;

        public VentanaLibros()
        {
            InitializeComponent();
            Loaded += VentanaLibros_Loaded;
        }

        private async void VentanaLibros_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarAutoresAsync();
            await CargarLibrosAsync();
        }

        private async System.Threading.Tasks.Task CargarAutoresAsync()
        {
            try
            {
                var autores = await _autorNegocio.ListarActivosAsync();
                cboAutor.ItemsSource = autores;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar autores: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task CargarLibrosAsync(string filtro = "")
        {
            try
            {
                var libros = await _libroNegocio.ListarAsync(filtro);
                dgLibros.ItemsSource = libros;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar libros: " + ex.Message);
            }
        }

        private void DgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLibros.SelectedItem is Libro libro)
            {
                _libroIdSeleccionado = libro.Id;
                txtTitulo.Text = libro.Titulo;
                txtIsbn.Text = libro.ISBN;
                cboAutor.SelectedValue = libro.AutorId;
                txtEjemplares.Text = libro.Ejemplares.ToString();
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            await CargarLibrosAsync(txtBuscar.Text.Trim());
        }

        private async void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await CargarLibrosAsync(txtBuscar.Text.Trim());
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtEjemplares.Text, out int ejemplares))
                {
                    MostrarError("Los ejemplares deben ser un numero entero.");
                    return;
                }

                var libro = new Libro
                {
                    Id = _libroIdSeleccionado,
                    Titulo = txtTitulo.Text.Trim(),
                    ISBN = txtIsbn.Text.Trim(),
                    AutorId = (int)(cboAutor.SelectedValue ?? 0),
                    Ejemplares = ejemplares
                };

                if (_libroIdSeleccionado == 0)
                {
                    await _libroNegocio.InsertarAsync(libro);
                    MostrarExito("Libro registrado correctamente.");
                }
                else
                {
                    await _libroNegocio.ActualizarAsync(libro);
                    MostrarExito("Libro actualizado correctamente.");
                }

                Limpiar();
                await CargarLibrosAsync();
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
            if (_libroIdSeleccionado == 0)
            {
                MostrarError("Debe seleccionar un libro de la lista.");
                return;
            }

            var confirmacion = MessageBox.Show(
                $"Esta seguro que desea eliminar el libro '{txtTitulo.Text}'?",
                "Confirmar eliminacion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                await _libroNegocio.EliminarAsync(_libroIdSeleccionado);
                MostrarExito("Libro eliminado correctamente.");
                Limpiar();
                await CargarLibrosAsync();
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
            _libroIdSeleccionado = 0;
            txtTitulo.Clear();
            txtIsbn.Clear();
            txtEjemplares.Clear();
            cboAutor.SelectedIndex = -1;
            dgLibros.SelectedItem = null;
        }

        private void MostrarError(string mensaje)
            => MessageBox.Show(mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void MostrarExito(string mensaje)
            => MessageBox.Show(mensaje, "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}