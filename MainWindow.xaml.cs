using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace cukraszda_wpf
{
    public class Suti
    {
        public string Nev { get; set; }
        public int Ar { get; set; }
    }

    public partial class MainWindow : Window
    {
        private List<Suti> sutik = new List<Suti>();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                kep.Source = new BitmapImage(new Uri("pack://application:,,,/Images/sutemeny.jpg"));
            }
            catch { }
        }

        private void Betoltes_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Adatfájl betöltése",
                Filter = "Szöveges fájlok (*.txt;*.csv)|*.txt;*.csv|Minden fájl (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true) return;

            List<Suti> beolvasott;
            try
            {
                beolvasott = Beolvas(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a fájl beolvasásakor: " + ex.Message, "Hiba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (beolvasott.Count == 0)
            {
                MessageBox.Show("A fájlban nem található süteményadat!", "Hiba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            sutik = beolvasott;
            sutemenyekMenu.IsEnabled = true;
            statusText.Text = sutik.Count + " sütemény betöltve.";

            ArlistaMegnyitas(); 
        }

        private List<Suti> Beolvas(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            byte[] bytes = File.ReadAllBytes(path);
            string szoveg;
            try
            {
                szoveg = new UTF8Encoding(false, true).GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                szoveg = Encoding.GetEncoding(1250).GetString(bytes);
            }
            szoveg = szoveg.TrimStart('\uFEFF');

            var lista = new List<Suti>();
            foreach (string sor in szoveg.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] p = sor.Split(new[] { ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 2)
                {

                    int i = sor.LastIndexOf(' ');
                    if (i < 0) continue;
                    p = new[] { sor.Substring(0, i), sor.Substring(i + 1) };
                }

                string szamok = new string(p[1].Where(char.IsDigit).ToArray());
                if (!int.TryParse(szamok, out int ar)) continue;

                lista.Add(new Suti { Nev = p[0].Trim(), Ar = ar });
            }
            return lista;
        }

        private void Sutemenyek_Click(object sender, RoutedEventArgs e)
        {
            ArlistaMegnyitas();
        }

        private void ArlistaMegnyitas()
        {
            var ablak = new ArlistaWindow(sutik) { Owner = this };
            ablak.ShowDialog();
        }

        private void Nevjegy_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Olasz Cukrászda\n\nKészítők: Kis Bence, Barbalics Zoltán\n4-es Csapat\n\nVerzió: 1.0",
                "Névjegy", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}