using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace cukraszda_wpf
{
    public partial class ArlistaWindow : Window
    {
        private class Sor
        {
            public Suti Suti;
            public CheckBox Check;
            public TextBox Db;
        }

        private readonly List<Sor> sorok = new List<Sor>();

        public ArlistaWindow(List<Suti> sutik)
        {
            InitializeComponent();

            foreach (Suti s in sutik)
            {
                var panel = new DockPanel { Margin = new Thickness(0, 5, 0, 5) };

                var adag = new TextBlock
                {
                    Text = "adag",
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var db = new TextBox { Width = 45, VerticalContentAlignment = VerticalAlignment.Center };
                var cb = new CheckBox
                {
                    Content = s.Nev + " (" + s.Ar + " Ft)",
                    VerticalAlignment = VerticalAlignment.Center
                };

                // pipálásnál automatikusan 1 adag, kipipálás törlésénél üres
                cb.Checked += (a, b) => { if (string.IsNullOrWhiteSpace(db.Text)) db.Text = "1"; };
                cb.Unchecked += (a, b) => db.Text = "";

                DockPanel.SetDock(adag, Dock.Right);
                DockPanel.SetDock(db, Dock.Right);
                panel.Children.Add(adag);
                panel.Children.Add(db);
                panel.Children.Add(cb);

                listaPanel.Children.Add(panel);
                sorok.Add(new Sor { Suti = s, Check = cb, Db = db });
            }
        }

        // Kijelölt tételek összegyűjtése; hibánál null
        private List<(Suti Suti, int Db)> Rendeles()
        {
            var lista = new List<(Suti Suti, int Db)>();

            foreach (Sor s in sorok)
            {
                if (s.Check.IsChecked != true) continue;

                if (!int.TryParse(s.Db.Text, out int db) || db <= 0)
                {
                    MessageBox.Show("Adj meg érvényes mennyiséget ehhez: " + s.Suti.Nev + "!",
                        "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                lista.Add((s.Suti, db));
            }

            if (lista.Count == 0)
            {
                MessageBox.Show("Nincs kiválasztva semmi!", "Hiba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return lista;
        }

        // Bal egérgomb: rendelés leadása
        private void Rendel_Click(object sender, RoutedEventArgs e)
        {
            var rendeles = Rendeles();
            if (rendeles == null) return;

            int osszeg = rendeles.Sum(x => x.Suti.Ar * x.Db);
            MessageBox.Show("A rendelést leadtuk!\nÖsszesen: " + osszeg + " Ft",
                "Rendelés", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Jobb egérgomb → context menü → Számlát kér
        private void SzamlatKer_Click(object sender, RoutedEventArgs e)
        {
            var rendeles = Rendeles();
            if (rendeles == null) return;

            var sorokSzoveg = new List<string>
            {
                "OLASZ CUKRÁSZDA",
                "SZÁMLA",
                DateTime.Now.ToString("yyyy.MM.dd. HH:mm"),
                new string('-', 44)
            };

            int osszeg = 0;
            foreach (var t in rendeles)
            {
                int ertek = t.Suti.Ar * t.Db;
                osszeg += ertek;
                sorokSzoveg.Add(string.Format("{0,-20} {1,3} x {2,5} Ft = {3,6} Ft",
                    t.Suti.Nev, t.Db, t.Suti.Ar, ertek));
            }

            sorokSzoveg.Add(new string('-', 44));
            sorokSzoveg.Add("Végösszeg: " + osszeg + " Ft");

            var bekezdes = new Paragraph { Margin = new Thickness(0) };
            for (int i = 0; i < sorokSzoveg.Count; i++)
            {
                if (i > 0) bekezdes.Inlines.Add(new LineBreak());
                bekezdes.Inlines.Add(new Run(sorokSzoveg[i]));
            }

            var rtb = new RichTextBox
            {
                IsReadOnly = true,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14,
                Padding = new Thickness(10),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Document = new FlowDocument(bekezdes)
            };

            var ablak = new Window
            {
                Title = "Számla",
                Width = 520,
                Height = 400,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = rtb
            };
            ablak.ShowDialog();
        }
    }
}