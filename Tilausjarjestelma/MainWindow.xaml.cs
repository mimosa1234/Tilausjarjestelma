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

            PaivitaTilausAsiakasCombo();
            PaivitaTilausTuoteCombo();

            PaivitaTilaustenPoistoCombo();

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
            PaivitaAsiakasCombo();
            PaivitaTilausAsiakasCombo();


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
            PaivitaKategoriaCombo();
            PaivitaTilausTuoteCombo();

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
            PaivitaTilausTuoteCombo();

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

        private void PaivitaTilausAsiakasCombo()
        {
            PaivitaComboBox(
                LisaaTilaus_asiakas,
                "SELECT Id, FirstName + ' ' + LastName AS Nimi FROM Customers",
                "Nimi",
                "Id"
            );
        }

        private void PaivitaTilausTuoteCombo()
        {
            PaivitaComboBox(
                LisaaTuote_tuote,
                "SELECT Id, Name FROM Products",
                "Name",
                "Id"
            );
        }
        private class TilausRivi
        {
            public int TuoteId { get; set; }
            public string Tuote { get; set; } = "";
            public int Maara { get; set; }
            public decimal Yksikkohinta { get; set; }
            public decimal RivinSumma => Yksikkohinta * Maara;
        }

        private List<TilausRivi> tilausRivit = new List<TilausRivi>();

        private void Lisaa_rivi_Click(object sender, RoutedEventArgs e)
        {
            if (LisaaTilaus_asiakas.SelectedValue == null ||
                LisaaTuote_tuote.SelectedValue == null)
            {
                MessageBox.Show("Valitse asiakas ja tuote.");
                return;
            }

            if (!int.TryParse(LisaaTuote_maara.Text, out int maara) || maara <= 0)
            {
                MessageBox.Show("Määrän pitää olla positiivinen numero.");
                return;
            }

            int tuoteId = (int)LisaaTuote_tuote.SelectedValue;

            decimal hinta;
            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Price FROM Products WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", tuoteId);
                    hinta = (decimal)cmd.ExecuteScalar();
                }
            }

            var row = (DataRowView)LisaaTuote_tuote.SelectedItem;
            string tuoteNimi = row["Name"].ToString() ?? "";

            tilausRivit.Add(new TilausRivi
            {
                TuoteId = tuoteId,
                Tuote = tuoteNimi,
                Maara = maara,
                Yksikkohinta = hinta
            });

            LisaaTilaus_lista.ItemsSource = null;
            LisaaTilaus_lista.ItemsSource = tilausRivit;

            LisaaTilaus_asiakas.IsEnabled = false;

            LisaaTuote_maara.Clear();
            LisaaTuote_tuote.SelectedIndex = -1;
        }

        private void Luo_tilaus_Click(object sender, RoutedEventArgs e)
        {
            if (tilausRivit.Count == 0)
            {
                MessageBox.Show("Lisää vähintään yksi rivi.");
                return;
            }

            if (LisaaTilaus_asiakas.SelectedValue == null)
            {
                MessageBox.Show("Valitse asiakas.");
                return;
            }

            int asiakasId = (int)LisaaTilaus_asiakas.SelectedValue;
            decimal tilauksenSumma = tilausRivit.Sum(r => r.RivinSumma);
            int uusiTilausId;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                string orderSql = @"
                    INSERT INTO Orders (CustomerId, OrderDate, TotalPrice)
                    OUTPUT INSERTED.OrderId
                    VALUES (@C, GETDATE(), @Total)";

                using (SqlCommand cmd = new SqlCommand(orderSql, conn))
                {
                    cmd.Parameters.AddWithValue("@C", asiakasId);
                    cmd.Parameters.AddWithValue("@Total", tilauksenSumma);
                    uusiTilausId = (int)cmd.ExecuteScalar();
                }

                foreach (var r in tilausRivit)
                {
                    string itemSql = @"
                        INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
                        VALUES (@O, @P, @Q, @U, @T)";

                    using (SqlCommand cmd2 = new SqlCommand(itemSql, conn))
                    {
                        cmd2.Parameters.AddWithValue("@O", uusiTilausId);
                        cmd2.Parameters.AddWithValue("@P", r.TuoteId);
                        cmd2.Parameters.AddWithValue("@Q", r.Maara);
                        cmd2.Parameters.AddWithValue("@U", r.Yksikkohinta);
                        cmd2.Parameters.AddWithValue("@T", r.RivinSumma);
                        cmd2.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show("Tilaus luotu!");

            PaivitaTilaustenPoistoCombo();

            tilausRivit.Clear();
            LisaaTilaus_lista.ItemsSource = null;
            LisaaTilaus_asiakas.IsEnabled = true;
            LisaaTilaus_asiakas.SelectedIndex = -1;
        }

        private void PaivitaTilaustenPoistoCombo()
        {
            PaivitaComboBox(
                TilausPoista_tilaus,
                "SELECT OrderId FROM Orders",
                "OrderId",
                "OrderId"
            );
        }
        private void PaivitaTilausRivit(int orderId)
        {
            string sql = $@"
                SELECT OI.OrderItemId, P.Name AS Tuote, OI.Quantity, OI.UnitPrice, OI.TotalPrice
                FROM OrderItems OI
                INNER JOIN Products P ON OI.ProductId = P.Id
                WHERE OI.OrderId = {orderId}";

            PaivitaDataGrid(PoistaTilaus_lista, sql);
        }
        private void TilausPoista_tilaus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TilausPoista_tilaus.SelectedValue == null)
                return;

            int orderId = (int)TilausPoista_tilaus.SelectedValue;
            PaivitaTilausRivit(orderId);
        }
        private void Poista_kokoTilaus_Click(object sender, RoutedEventArgs e)
        {
            if (TilausPoista_tilaus.SelectedValue == null)
            {
                MessageBox.Show("Valitse poistettava tilaus.");
                return;
            }

            int orderId = (int)TilausPoista_tilaus.SelectedValue;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                SqlCommand cmd1 = new SqlCommand(
                    "DELETE FROM OrderItems WHERE OrderId = @id", conn);
                cmd1.Parameters.AddWithValue("@id", orderId);
                cmd1.ExecuteNonQuery();

                SqlCommand cmd2 = new SqlCommand(
                    "DELETE FROM Orders WHERE OrderId = @id", conn);
                cmd2.Parameters.AddWithValue("@id", orderId);
                cmd2.ExecuteNonQuery();
            }

            MessageBox.Show("Tilaus poistettu!");

            PaivitaTilaustenPoistoCombo();
            PoistaTilaus_lista.ItemsSource = null;
        }
        private void Poista_rivi_Click(object sender, RoutedEventArgs e)
        {
            if (PoistaTilaus_lista.SelectedItem == null)
            {
                MessageBox.Show("Valitse poistettava rivi.");
                return;
            }

            DataRowView row = (DataRowView)PoistaTilaus_lista.SelectedItem;
            int orderItemId = (int)row["OrderItemId"];

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM OrderItems WHERE OrderItemId = @id", conn);
                cmd.Parameters.AddWithValue("@id", orderItemId);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Rivi poistettu!");

            int orderId = (int)TilausPoista_tilaus.SelectedValue;
            PaivitaTilausRivit(orderId);
        }


    }
}