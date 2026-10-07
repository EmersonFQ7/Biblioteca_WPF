using System.Windows;

namespace Biblioteca.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLibros_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaLibros();
            ventana.ShowDialog();
        }

        private void BtnSocios_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaSocios();
            ventana.ShowDialog();
        }

        private void BtnPrestamos_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaPrestamo();
            ventana.ShowDialog();
        }

        private void BtnDevoluciones_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaDevolucion();
            ventana.ShowDialog();
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaReporte();
            ventana.ShowDialog();
        }
    }
}