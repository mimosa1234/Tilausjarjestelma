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

            PaivitaVarastoTuoteCombo();
            PaivitaVarastosaldoLista();
        }

        // Yleiset metodit

        // Yleinen metodi datagridin päivittämiseen
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

        // Yleinen metodi comboboxin päivittämiseen
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
        // Yleinen metodi tietueen lisäämiseen
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
        // Yleinen metodi tietueen poistamiseen
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

        // Asiakkaat
        // Päivitä asiakaslista
        private void PaivitaAsiakasLista()
        {
            PaivitaDataGrid(Asiakkaat_lista, "SELECT * FROM Customers");
        }

        // Lisää asiakas
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

        // Päivitä asiakaskombo
        private void PaivitaAsiakasCombo()
        {
            PaivitaComboBox(
                Asiakas_poisto,
                "SELECT Id, FirstName + ' ' + LastName AS Nimi FROM Customers",
                "Nimi",
                "Id"
            );
        }

        // Poista asiakas
        private void Poista_asiakas_Click(object sender, RoutedEventArgs e)
        {
            if (Asiakas_poisto.SelectedValue == null)
            {
                MessageBox.Show("Valitse poistettava asiakas.");
                return;
            }

            int id = (int)Asiakas_poisto.SelectedValue;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                // Tarkista onko asiakkaalla tilauksia
                using (SqlCommand checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Orders WHERE CustomerId = @id", conn))
                {
                    checkCmd.Parameters.AddWithValue("@id", id);

                    int tilauksia = (int)checkCmd.ExecuteScalar();

                    if (tilauksia > 0)
                    {
                        MessageBox.Show(
                            "Asiakasta ei voi poistaa, koska hänellä on olemassa olevia tilauksia.",
                            "Poisto estetty",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }
                }

                // Jos ei tilauksia → poisto sallittu
                using (SqlCommand deleteCmd = new SqlCommand(
                    "DELETE FROM Customers WHERE Id = @Id", conn))
                {
                    deleteCmd.Parameters.AddWithValue("@Id", id);
                    deleteCmd.ExecuteNonQuery();
                }
            }

            // Päivitä näkymät
            PaivitaAsiakasLista();
            PaivitaAsiakasCombo();

            LisaaTilaus_asiakas.SelectedIndex = -1;
            LisaaTilaus_asiakas.IsEnabled = true;
            PaivitaTilausAsiakasCombo();
        }


        // Kategoriat
        // Paivita kategoriat lista
        private void PaivitaKategoriatLista()
        {
            PaivitaDataGrid(Kategoriat_lista, "SELECT * FROM Categories");
        }

        // Lisaa kategoria
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

        // Päivitä kategoria combo
        private void PaivitaKategoriaCombo()
        {
            PaivitaComboBox(
                Kategoria_poisto,
                "SELECT Id, Name FROM Categories",
                "Name",
                "Id"
            );
        }

        // Poista kategoria
        private void Poista_kategoria_Click(object sender, RoutedEventArgs e)
        {
            if (Kategoria_poisto.SelectedValue == null)
            {
                MessageBox.Show("Valitse poistettava kategoria.");
                return;
            }

            int id = (int)Kategoria_poisto.SelectedValue;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                // Tarkista onko kategoriassa tuotteita
                SqlCommand checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Products WHERE CategoryId = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", id);

                int tuotteita = (int)checkCmd.ExecuteScalar();

                if (tuotteita > 0)
                {
                    MessageBox.Show(
                        "Kategoriaa ei voi poistaa, koska siihen kuuluu tuotteita.\nPoista tai siirrä tuotteet ensin.");
                    return;
                }

                // Jos ei tuotteita → poisto sallittu
                SqlCommand deleteCmd = new SqlCommand(
                    "DELETE FROM Categories WHERE Id = @id", conn);
                deleteCmd.Parameters.AddWithValue("@id", id);
                deleteCmd.ExecuteNonQuery();
            }

            Kategoria_poisto.SelectedIndex = -1;

            PaivitaKategoriatLista();
            PaivitaKategoriaCombo();
            PaivitaTuoteKategoriaCombo();
        }


        // Tuotteet
        // Päivitä tuotelista
        private void PaivitaTuoteLista()
        {
            PaivitaDataGrid(Tuotteet_lista, "SELECT * FROM Products");
        }

        // Lisää tuote
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

            Tuotteet_nimi.Clear();
            Tuotteet_hinta.Clear();
            Tuotteet_kuvaus.Clear();
            Tuotteet_varastosaldo.Clear();
            Tuotteet_kategoria.SelectedIndex = -1;

            PaivitaTuoteLista();
            PaivitaTuoteCombo();

            PaivitaTuoteKategoriaCombo();
            PaivitaTilausTuoteCombo();
            PaivitaVarastoTuoteCombo();
            PaivitaVarastosaldoLista();

        }

        // Päivitä tuotekombo
        private void PaivitaTuoteCombo()
        {
            PaivitaComboBox(
                Tuotteet_poista,
                "SELECT Id, Name FROM Products",
                "Name",
                "Id"
            );
        }

        // Päivitä tuotekategoriakombo
        private void PaivitaTuoteKategoriaCombo()
        {
            PaivitaComboBox(
                Tuotteet_kategoria,
                "SELECT Id, Name FROM Categories",
                "Name",
                "Id"
            );
        }

        // Poista tuote
        private void Poista_tuote_Click(object sender, RoutedEventArgs e)
        {
            if (Tuotteet_poista.SelectedValue == null)
            {
                MessageBox.Show("Valitse poistettava tuote.");
                return;
            }

            int id = (int)Tuotteet_poista.SelectedValue;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                // 1) Tarkista onko tuotetta käytetty tilauksissa
                using (SqlCommand checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM OrderItems WHERE ProductId = @id", conn))
                {
                    checkCmd.Parameters.AddWithValue("@id", id);
                    int kpl = (int)checkCmd.ExecuteScalar();

                    if (kpl > 0)
                    {
                        MessageBox.Show(
                            "Tuotetta ei voi poistaa, koska sitä on käytetty tilauksissa.\n" +
                            "Tuote tulee poistaa tilauksista ennen tuotteen poistamista.",
                            "Poisto estetty",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }
                }

                // 2) Poista tuote jos sitä ei ole käytetty tilauksissa
                using (SqlCommand deleteCmd = new SqlCommand(
                    "DELETE FROM Products WHERE Id = @id", conn))
                {
                    deleteCmd.Parameters.AddWithValue("@id", id);
                    deleteCmd.ExecuteNonQuery();
                }
            }

            // Päivitä näkymät ja nollaa valinnat
            Tuotteet_poista.SelectedIndex = -1;
            LisaaTuote_tuote.SelectedIndex = -1;

            PaivitaTuoteLista();
            PaivitaTuoteCombo();
            PaivitaTilausTuoteCombo();
            PaivitaTuoteKategoriaCombo();
            PaivitaVarastoTuoteCombo();
            PaivitaVarastosaldoLista();
        }

        // Tilaukset
        // Päivitä tilaus asiakaskombo
        private void PaivitaTilausAsiakasCombo()
        {
            PaivitaComboBox(
                LisaaTilaus_asiakas,
                "SELECT Id, FirstName + ' ' + LastName AS Nimi FROM Customers",
                "Nimi",
                "Id"
            );
        }

        // Päivitä tilaus tuotekombo
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

        // Lisää rivi tilaukseen
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

            // Hae varastosaldo ja hinta
            int saldo;
            decimal hinta;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Stock, Price FROM Products WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", tuoteId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show("Tuotetta ei löytynyt.");
                            return;
                        }

                        saldo = reader.GetInt32(0);
                        hinta = reader.GetDecimal(1);
                    }
                }
            }

            // Tarkista onko tuote jo tilauksessa
            var olemassaOleva = tilausRivit.FirstOrDefault(r => r.TuoteId == tuoteId);

            if (olemassaOleva != null)
            {
                int uusiMaara = olemassaOleva.Maara + maara;

                if (uusiMaara > saldo)
                {
                    MessageBox.Show(
                        $"Varastossa ei ole tarpeeksi tuotetta.\n" +
                        $"Saatavilla: {saldo} kpl\n" +
                        $"Tilauksessa jo: {olemassaOleva.Maara} kpl",
                        "Varastosaldo ei riitä",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Päivitä olemassa olevan rivin määrä
                olemassaOleva.Maara = uusiMaara;
            }
            else
            {
                if (maara > saldo)
                {
                    MessageBox.Show(
                        $"Varastossa ei ole tarpeeksi tuotetta.\n" +
                        $"Saatavilla: {saldo} kpl",
                        "Varastosaldo ei riitä",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
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
            }

            // Päivitä tilauslista
            LisaaTilaus_lista.ItemsSource = null;
            LisaaTilaus_lista.ItemsSource = tilausRivit;

            // Lukitse asiakas ensimmäisen rivin jälkeen
            LisaaTilaus_asiakas.IsEnabled = false;

            // Tyhjennä syötteet
            LisaaTuote_maara.Clear();
            LisaaTuote_tuote.SelectedIndex = -1;
        }


        // Luo tilaus
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

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    int uusiTilausId;

                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Orders (CustomerId, OrderDate, TotalPrice)
                        OUTPUT INSERTED.OrderId
                        VALUES (@C, GETDATE(), @Total)", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@C", asiakasId);
                        cmd.Parameters.AddWithValue("@Total", tilauksenSumma);
                        uusiTilausId = (int)cmd.ExecuteScalar();
                    }

                    foreach (var r in tilausRivit)
                    {
                        using (SqlCommand cmd2 = new SqlCommand(@"
                            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
                            VALUES (@O, @P, @Q, @U, @T)", conn, tran))
                        {
                            cmd2.Parameters.AddWithValue("@O", uusiTilausId);
                            cmd2.Parameters.AddWithValue("@P", r.TuoteId);
                            cmd2.Parameters.AddWithValue("@Q", r.Maara);
                            cmd2.Parameters.AddWithValue("@U", r.Yksikkohinta);
                            cmd2.Parameters.AddWithValue("@T", r.RivinSumma);
                            cmd2.ExecuteNonQuery();
                        }

                        using (SqlCommand stockCmd = new SqlCommand(@"
                            UPDATE Products SET Stock = Stock - @q WHERE Id = @p", conn, tran))
                        {
                            stockCmd.Parameters.AddWithValue("@q", r.Maara);
                            stockCmd.Parameters.AddWithValue("@p", r.TuoteId);
                            stockCmd.ExecuteNonQuery();
                        }
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Tilauksen luonti epäonnistui:\n" + ex.Message);
                    return;
                }
            }

            PaivitaVarastosaldoLista();
            PaivitaVarastoTuoteCombo();
            PaivitaTilaustenPoistoCombo();

            tilausRivit.Clear();
            LisaaTilaus_lista.ItemsSource = null;
            LisaaTilaus_asiakas.IsEnabled = true;
            LisaaTilaus_asiakas.SelectedIndex = -1;
        }


        // Poista tilaus
        private void PaivitaTilaustenPoistoCombo()
        {
            PaivitaComboBox(
                TilausPoista_tilaus,
                "SELECT OrderId FROM Orders",
                "OrderId",
                "OrderId"
            );
        }

        // Päivitä tilauksen rivit
        private void PaivitaTilausRivit(int orderId)
        {
            string sql = $@"
                SELECT OI.OrderItemId, P.Name AS Tuote, OI.Quantity, OI.UnitPrice, OI.TotalPrice
                FROM OrderItems OI
                INNER JOIN Products P ON OI.ProductId = P.Id
                WHERE OI.OrderId = {orderId}";

            PaivitaDataGrid(PoistaTilaus_lista, sql);
        }

        // Tilausten poisto tapahtumat
        private void TilausPoista_tilaus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TilausPoista_tilaus.SelectedValue == null)
                return;

            int orderId = (int)TilausPoista_tilaus.SelectedValue;
            PaivitaTilausRivit(orderId);
        }

        // Poista koko tilaus
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

                SqlCommand haeRivit = new SqlCommand(
                    "SELECT ProductId, Quantity FROM OrderItems WHERE OrderId = @id", conn);
                haeRivit.Parameters.AddWithValue("@id", orderId);

                List<(int tuoteId, int maara)> palautettavat = new();

                using (SqlDataReader reader = haeRivit.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        palautettavat.Add((
                            reader.GetInt32(0),
                            reader.GetInt32(1)
                        ));
                    }
                }
               
                foreach (var r in palautettavat)
                {
                    using (SqlCommand palautaSaldo = new SqlCommand(
                        "UPDATE Products SET Stock = Stock + @q WHERE Id = @p", conn))
                    {
                        palautaSaldo.Parameters.AddWithValue("@q", r.maara);
                        palautaSaldo.Parameters.AddWithValue("@p", r.tuoteId);
                        palautaSaldo.ExecuteNonQuery();
                    }
                }

                SqlCommand cmd1 = new SqlCommand(
                    "DELETE FROM OrderItems WHERE OrderId = @id", conn);
                cmd1.Parameters.AddWithValue("@id", orderId);
                cmd1.ExecuteNonQuery();

                SqlCommand cmd2 = new SqlCommand(
                    "DELETE FROM Orders WHERE OrderId = @id", conn);
                cmd2.Parameters.AddWithValue("@id", orderId);
                cmd2.ExecuteNonQuery();
            }

            PaivitaTilaustenPoistoCombo();
            PaivitaVarastosaldoLista();
            PoistaTilaus_lista.ItemsSource = null;
        }


        // Poista tilauksen rivi
        private void Poista_rivi_Click(object sender, RoutedEventArgs e)
        {
            if (PoistaTilaus_lista.SelectedItem == null)
            {
                MessageBox.Show("Valitse poistettava rivi.");
                return;
            }

            DataRowView row = (DataRowView)PoistaTilaus_lista.SelectedItem;

            int orderItemId = (int)row["OrderItemId"];
            int maara = (int)row["Quantity"];

            int tuoteId;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                // Hae ProductId ennen poistoa
                using (SqlCommand haeTuote = new SqlCommand(
                    "SELECT ProductId FROM OrderItems WHERE OrderItemId = @id", conn))
                {
                    haeTuote.Parameters.AddWithValue("@id", orderItemId);
                    tuoteId = (int)haeTuote.ExecuteScalar();
                }

                // Palauta varastosaldo
                using (SqlCommand palautaSaldo = new SqlCommand(
                    "UPDATE Products SET Stock = Stock + @q WHERE Id = @p", conn))
                {
                    palautaSaldo.Parameters.AddWithValue("@q", maara);
                    palautaSaldo.Parameters.AddWithValue("@p", tuoteId);
                    palautaSaldo.ExecuteNonQuery();
                }

                // Poista tilausrivi
                using (SqlCommand poistaRivi = new SqlCommand(
                    "DELETE FROM OrderItems WHERE OrderItemId = @id", conn))
                {
                    poistaRivi.Parameters.AddWithValue("@id", orderItemId);
                    poistaRivi.ExecuteNonQuery();
                }
            }

            // Päivitä näkymät
            int orderId = (int)TilausPoista_tilaus.SelectedValue;
            PaivitaTilausRivit(orderId);
            PaivitaVarastosaldoLista();
        }


        // Varastosaldo
        // Päivitä varastotuotekombo
        private void PaivitaVarastoTuoteCombo()
        {
            PaivitaComboBox(
                Muokkaa_varastosaldo,
                "SELECT Id, Name FROM Products",
                "Name",
                "Id"
            );
        }

        // Päivitä varastosaldo lista
        private void PaivitaVarastosaldoLista()
        {
            PaivitaDataGrid(Varastosaldo_lista,
                "SELECT Id, Name, Stock FROM Products");
        }

        // Varastosaldon muokkaustapahtumat
        private void Muokkaa_varastosaldo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Muokkaa_varastosaldo.SelectedValue == null)
                return;

            int id = (int)Muokkaa_varastosaldo.SelectedValue;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT Stock FROM Products WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int saldo = (int)cmd.ExecuteScalar();
                NykyinenSaldo_varastosaldo.Text = $"Saldo = {saldo}";
            }
        }

        // Päivitä varastosaldo
        private void Paivita_saldo_Click(object sender, RoutedEventArgs e)
        {
            if (Muokkaa_varastosaldo.SelectedValue == null)
            {
                MessageBox.Show("Valitse tuote.");
                return;
            }

            if (!int.TryParse(UusiArvo_varastosaldo.Text, out int arvo))
            {
                MessageBox.Show("Anna numeroarvo.");
                return;
            }

            int id = (int)Muokkaa_varastosaldo.SelectedValue;
            int nykyinenSaldo;

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();

                // Hae nykyinen saldo
                SqlCommand cmd1 = new SqlCommand(
                    "SELECT Stock FROM Products WHERE Id = @id", conn);
                cmd1.Parameters.AddWithValue("@id", id);
                nykyinenSaldo = (int)cmd1.ExecuteScalar();
            }

            int uusiSaldo = nykyinenSaldo;

            if (AsetaUusiSaldo_varastosaldo.IsChecked == true)
                uusiSaldo = arvo;

            if (LisaaVarastoon_varastosaldo.IsChecked == true)
                uusiSaldo = nykyinenSaldo + arvo;

            if (VahennaVarastosta_varastosaldo.IsChecked == true)
            {
                if (arvo > nykyinenSaldo)
                {
                    MessageBox.Show("Saldo ei voi mennä miinukselle!");
                    return;
                }
                uusiSaldo = nykyinenSaldo - arvo;
            }

            using (SqlConnection conn = new SqlConnection(polku))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Products SET Stock = @s WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@s", uusiSaldo);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // Päivitä näkymä
            NykyinenSaldo_varastosaldo.Text = $"Saldo = {uusiSaldo}";
            PaivitaVarastosaldoLista();

            UusiArvo_varastosaldo.Text = "";

            AsetaUusiSaldo_varastosaldo.IsChecked = false;
            LisaaVarastoon_varastosaldo.IsChecked = false;
            VahennaVarastosta_varastosaldo.IsChecked = false;

            NykyinenSaldo_varastosaldo.Text = "Saldo = -";

            Muokkaa_varastosaldo.SelectedIndex = -1;
        }


    }
}