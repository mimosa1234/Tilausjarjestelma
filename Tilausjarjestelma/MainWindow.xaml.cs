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


    }
}