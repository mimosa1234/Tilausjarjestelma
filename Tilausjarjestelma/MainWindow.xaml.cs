using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;


namespace Tilausjarjestelma
{

    public partial class MainWindow : Window
    {
        string polku = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hp\Documents\Tilausjarjestelma\Tilausjarjestelma\Tilausjarjestelma.mdf;Integrated Security=True;Connect Timeout=30";

        public MainWindow()
        {
            InitializeComponent();

            PaivitaAsiakasLista();
            PaivitaAsiakasCombo();

            PaivitaKategoriatLista();
            PaivitaKategoriaCombo();

            PaivitaTuoteLista();
            PaivitaTuoteCombo();
            PaivitaTuoteKategoriaCombo();

        }
        private void PaivitaDataGrid(DataGrid grid, string sql)
        {
            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                grid.ItemsSource = dt.DefaultView;
            }
        }

        private void PaivitaComboBox(ComboBox box, string sql, string display, string value)
        {
            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                box.ItemsSource = dt.DefaultView;
                box.DisplayMemberPath = display;
                box.SelectedValuePath = value;
            }
        }

        private void Lisaa(string sql, Dictionary<string, object> param)
        {
            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    foreach (var p in param)
                        cmd.Parameters.AddWithValue("@" + p.Key, p.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void Poista(string sql, int id)
        {
            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void PaivitaAsiakasLista()
        {
            PaivitaDataGrid(Asiakkaat_lista, "SELECT * FROM Customers");
        }

        private void LisaaAsiakas_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"
                INSERT INTO Customers (FirstName, LastName, Email, Phone, Address)
                VALUES (@F, @L, @E, @P, @A)";

            Lisaa(sql, new Dictionary<string, object>()
            {
                { "F", Asiakas_etunimi.Text },
                { "L", Asiakas_sukunimi.Text },
                { "E", Asiakas_sahkoposti.Text },
                { "P", Asiakas_puhelin.Text },
                { "A", Asiakas_osoite.Text }
            });

            PaivitaAsiakasLista();

            Asiakas_etunimi.Clear();
            Asiakas_sukunimi.Clear();
            Asiakas_sahkoposti.Clear();
            Asiakas_puhelin.Clear();
            Asiakas_osoite.Clear();
        }

        private void PaivitaAsiakasCombo()
        {
            PaivitaComboBox(
                Asiakas_poisto,
                "SELECT Id, FirstName + ' ' + LastName AS Nimi FROM Customers",
                "Nimi",
                "Id"
            );
        }

        private void Poista_asiakas_Click(object sender, RoutedEventArgs e)
        {
            if (Asiakas_poisto.SelectedValue == null)
            {
                MessageBox.Show("Valitse poistettava asiakas.");
                return;
            }

            int id = (int)Asiakas_poisto.SelectedValue;

            Poista("DELETE FROM Customers WHERE Id = @Id", id);

            PaivitaAsiakasLista();
            PaivitaAsiakasCombo();
        }

        private void PaivitaKategoriatLista()
        {
            PaivitaDataGrid(Kategoriat_lista, "SELECT * FROM Categories");
        }

        private void Lisaa_kategoria_Click(object sender, RoutedEventArgs e)
        {
            string sql = "INSERT INTO Categories (Name) VALUES (@Name)";

            var param = new Dictionary<string, object>()
            {
                { "Name", Kategoria_nimi.Text }
            };

            Lisaa(sql, param);

            PaivitaKategoriatLista();
            PaivitaTuoteKategoriaCombo();

            Kategoria_nimi.Text = "";
        }

        private void PaivitaKategoriaCombo()
        {
            PaivitaComboBox(
                Kategoria_poisto,
                "SELECT Id, Name FROM Categories",
                "Name",
                "Id"
            );
        }

        private void Poista_kategoria_Click(object sender, RoutedEventArgs e)
        {
            if (Kategoria_poisto.SelectedValue == null)
            {
                MessageBox.Show("Valitse poistettava kategoria.");
                return;
            }

            int id = (int)Kategoria_poisto.SelectedValue;

            Poista("DELETE FROM Categories WHERE Id = @Id", id);

            PaivitaKategoriatLista();
            PaivitaKategoriaCombo();
        }

        private void PaivitaTuoteLista()
        {
            PaivitaDataGrid(Tuotteet_lista, "SELECT * FROM Products");
        }
        private void Lisaa_tuote_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Tuotteet_nimi.Text) ||
                string.IsNullOrWhiteSpace(Tuotteet_hinta.Text) ||
                string.IsNullOrWhiteSpace(Tuotteet_kuvaus.Text) ||
                Tuotteet_kategoria.SelectedValue == null ||
                string.IsNullOrWhiteSpace(Tuotteet_varastosaldo.Text))
            {
                MessageBox.Show("Täytä kaikki kentät!");
                return;
            }

            if (!decimal.TryParse(Tuotteet_hinta.Text.Replace(",", "."), out decimal hinta))
            {
                MessageBox.Show("Virheellinen hinta!");
                return;
            }

            if (!int.TryParse(Tuotteet_varastosaldo.Text, out int saldo))
            {
                MessageBox.Show("Varastosaldo ei ole numero!");
                return;
            }

            string sql = @"INSERT INTO Products (Name, Price, Description, CategoryId, Stock)
                   VALUES (@N, @P, @D, @C, @S)";

            Lisaa(sql, new Dictionary<string, object>()
            {
                { "N", Tuotteet_nimi.Text },
                { "P", hinta },
                { "D", Tuotteet_kuvaus.Text },
                { "C", (int)Tuotteet_kategoria.SelectedValue },
                { "S", saldo }
            });

            MessageBox.Show("Tuote lisätty!");

            Tuotteet_nimi.Clear();
            Tuotteet_hinta.Clear();
            Tuotteet_kuvaus.Clear();
            Tuotteet_varastosaldo.Clear();
            Tuotteet_kategoria.SelectedIndex = -1;

            PaivitaTuoteLista();
            PaivitaTuoteCombo();

            PaivitaTuoteKategoriaCombo();
        }
        private void PaivitaTuoteCombo()
        {
            PaivitaComboBox(
                Tuotteet_poista,
                "SELECT Id, Name FROM Products",
                "Name",
                "Id"
            );
        }

        private void PaivitaTuoteKategoriaCombo()
        {
            PaivitaComboBox(
                Tuotteet_kategoria,
                "SELECT Id, Name FROM Categories",
                "Name",
                "Id"
            );
        }


        private void Poista_tuote_Click(object sender, RoutedEventArgs e)
        {
            if (Tuotteet_poista.SelectedValue == null)
            {
                MessageBox.Show("Valitse poistettava tuote.");
                return;
            }

            int id = (int)Tuotteet_poista.SelectedValue;

            string sql = "DELETE FROM Products WHERE Id = @Id";

            try
            {
                Poista(sql, id);
                MessageBox.Show("Tuote poistettu!");

                PaivitaTuoteLista();
                PaivitaTuoteCombo();
                PaivitaTuoteKategoriaCombo();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Virhe poistettaessa tuotetta: " + ex.Message);
            }
        }


    }
}