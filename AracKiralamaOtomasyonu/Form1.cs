using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AracKiralamaOtomasyonu
{
    public partial class Form1 : Form
    {
        // Veritabanı bağlantı adresin
        string baglantiAdresi = "Server=localhost;Database=AracKiralamaDB;Uid=root;Pwd=934653;";

        public Form1()
        {
            InitializeComponent();

            if (lblDurum != null) lblDurum.Visible = false;

            // --- GÜNCELLENMİŞ HAVALI HOŞGELDİN MESAJI ---
            try
            {
                using (MySqlConnection baglanti = new MySqlConnection(baglantiAdresi))
                {
                    baglanti.Open();
                    MySqlCommand komut = new MySqlCommand("SELECT FN_SirketIsmi()", baglanti);
                    object sonuc = komut.ExecuteScalar();

                    if (sonuc != null)
                    {
                        // Mesajı biraz süsledik ve İKON ekledik
                        MessageBox.Show(
                            "🏢 " + sonuc.ToString() + " 🏢\n\n" +
                            "✅ Veritabanı Bağlantısı: BAŞARILI\n" +
                            "📅 Tarih: " + DateTime.Now.ToShortDateString() + "\n\n" +
                            "Sisteme giriş yapıldı. İyi çalışmalar dileriz!",

                            "GÜVENLİ OTOMASYON SİSTEMİ", // Pencere Başlığı (Büyük harf)
                            MessageBoxButtons.OK,        // Sadece Tamam butonu
                            MessageBoxIcon.Information   // MAVİ BİLGİ İKONU (Pencereyi büyütür)
                        );

                        this.Text = sonuc.ToString(); // Form başlığına da yazsın
                    }
                }
            }
            catch
            {
                // Hata olursa sessizce devam
            }
            // ----------------------------------------------------------

            ButonlariAyarla();
        }

        // --- YÖNETİCİ MODU ---
        private void chkAdmin_CheckedChanged_1(object sender, EventArgs e)
        {
            if (chkAdmin.Checked)
            {
                string girilenSifre = SifreSor();
                if (girilenSifre == "1234")
                {
                    MessageBox.Show("Giriş Başarılı! Yönetici yetkileri açıldı.");
                    ButonlariAyarla();
                }
                else
                {
                    MessageBox.Show("HATALI ŞİFRE!");
                    chkAdmin.Checked = false;
                }
            }
            else
            {
                ButonlariAyarla();
            }
        }

        // Yedek tetikleyici
        private void chkAdmin_CheckedChanged(object sender, EventArgs e) { chkAdmin_CheckedChanged_1(sender, e); }

        private string SifreSor()
        {
            Form prompt = new Form() { Width = 300, Height = 150, Text = "Yönetici Şifresi", StartPosition = FormStartPosition.CenterScreen };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Şifre:" };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240, PasswordChar = '*' };
            Button confirmation = new Button() { Text = "Tamam", Left = 180, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            prompt.Controls.Add(textBox); prompt.Controls.Add(confirmation); prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void ButonlariAyarla()
        {
            bool yetkiVar = chkAdmin.Checked;
            button3.Enabled = yetkiVar;
            if (btnZamYap != null) btnZamYap.Enabled = yetkiVar;
            this.Text = yetkiVar ? "YÖNETİCİ MODU (Admin)" : "Personel Modu (Kısıtlı)";
        }

        // --- LİSTELE ---
        private void button1_Click(object sender, EventArgs e) { VerileriGetir(""); }

        // --- KAYDET ---
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection baglanti = new MySqlConnection(baglantiAdresi))
                {
                    baglanti.Open();
                    int secilenMarkaID = 1;
                    if (comboBox1.SelectedItem != null)
                    {
                        string s = comboBox1.SelectedItem.ToString();
                        if (s == "Mercedes") secilenMarkaID = 1;
                        else if (s == "BMW") secilenMarkaID = 2;
                        else if (s == "Audi") secilenMarkaID = 3;
                        else if (s == "Renault") secilenMarkaID = 4;
                    }

                    MySqlCommand komut = new MySqlCommand("SP_AracEkle", baglanti);
                    komut.CommandType = CommandType.StoredProcedure;
                    komut.Parameters.AddWithValue("@p_Plaka", textBox1.Text);

                    if (!string.IsNullOrEmpty(textBox3.Text)) komut.Parameters.AddWithValue("@p_Model", int.Parse(textBox3.Text));
                    else throw new Exception("Model yılı giriniz.");

                    if (!string.IsNullOrEmpty(textBox2.Text)) komut.Parameters.AddWithValue("@p_Fiyat", decimal.Parse(textBox2.Text));
                    else throw new Exception("Fiyat giriniz.");

                    komut.Parameters.AddWithValue("@p_MarkaID", secilenMarkaID);
                    komut.Parameters.AddWithValue("@p_SubeID", 1);

                    komut.ExecuteNonQuery();
                    MessageBox.Show("Araç eklendi!");
                    VerileriGetir("");
                }
            }
            catch (Exception hata) { MessageBox.Show("Hata: " + hata.Message); }
        }

        // --- SİL ---
        private void button3_Click(object sender, EventArgs e)
        {
            if (!chkAdmin.Checked) { MessageBox.Show("Yönetici yetkisi gerekir!"); return; }
            if (dataGridView1.CurrentRow == null) return;
            try
            {
                string plaka = dataGridView1.CurrentRow.Cells["Plaka"].Value.ToString();
                if (MessageBox.Show("Silinsin mi?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (MySqlConnection baglanti = new MySqlConnection(baglantiAdresi))
                    {
                        baglanti.Open();
                        MySqlCommand komut = new MySqlCommand("DELETE FROM Araclar WHERE Plaka=@p", baglanti);
                        komut.Parameters.AddWithValue("@p", plaka);
                        komut.ExecuteNonQuery();
                        VerileriGetir("");
                    }
                }
            }
            catch (Exception hata) { MessageBox.Show("Hata: " + hata.Message); }
        }

        // --- MÜŞTERİ LİSTELE ---
        private void btnMusteriListele_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection baglanti = new MySqlConnection(baglantiAdresi))
                {
                    baglanti.Open();
                    string sql = "SELECT * FROM View_MusteriMaskeli";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                    MessageBox.Show("Müşteriler maskelenerek listelendi.");
                }
            }
            catch (Exception hata) { MessageBox.Show("Hata: " + hata.Message); }
        }

        // --- KİRALA (BURASI DÜZELTİLDİ) ---
        private void btnKirala_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null || string.IsNullOrEmpty(txtMusteriID.Text))
            {
                MessageBox.Show("Lütfen listeden bir araç seçin ve Müşteri ID kutusuna '1' yazın.");
                return;
            }

            try
            {
                int aracID = 0;
                if (dataGridView1.Columns.Contains("AracID") && dataGridView1.CurrentRow.Cells["AracID"].Value != DBNull.Value)
                    aracID = Convert.ToInt32(dataGridView1.CurrentRow.Cells["AracID"].Value);

                using (MySqlConnection baglanti = new MySqlConnection(baglantiAdresi))
                {
                    baglanti.Open();
                    if (aracID == 0)
                    {
                        string plaka = dataGridView1.CurrentRow.Cells["Plaka"].Value.ToString();
                        MySqlCommand idBul = new MySqlCommand("SELECT AracID FROM Araclar WHERE Plaka=@p", baglanti);
                        idBul.Parameters.AddWithValue("@p", plaka);
                        object sonuc = idBul.ExecuteScalar();
                        if (sonuc != null) aracID = Convert.ToInt32(sonuc);
                    }

                    decimal gunlukFiyat = Convert.ToDecimal(dataGridView1.CurrentRow.Cells["GunlukFiyat"].Value);
                    int gunSayisi = (dtpTeslim.Value - dtpAlis.Value).Days;
                    if (gunSayisi <= 0) { MessageBox.Show("Hata: Tarihleri kontrol edin."); return; }

                    // DÜZELTME: Senin hata mesajındaki 'PlanlananTeslimTarihi' ismini tam olarak buraya yazdım.
                    string sql = "INSERT INTO kiralamalar (MusteriID, AracID, AlisTarihi, PlanlananTeslimTarihi, ToplamUcret) VALUES (@m, @a, @alis, @teslim, @ucret)";

                    MySqlCommand komut = new MySqlCommand(sql, baglanti);
                    komut.Parameters.AddWithValue("@m", int.Parse(txtMusteriID.Text));
                    komut.Parameters.AddWithValue("@a", aracID);
                    komut.Parameters.AddWithValue("@alis", dtpAlis.Value);
                    komut.Parameters.AddWithValue("@teslim", dtpTeslim.Value);
                    komut.Parameters.AddWithValue("@ucret", gunSayisi * gunlukFiyat);

                    komut.ExecuteNonQuery();
                    MessageBox.Show("✅ KİRALAMA BAŞARILI!\nTutar: " + (gunSayisi * gunlukFiyat) + " TL");
                    VerileriGetir("");
                }
            }
            catch (Exception hata) { MessageBox.Show("Kiralama Hatası: " + hata.Message); }
        }

        // --- ZAM YAP ---
        private void btnZamYap_Click(object sender, EventArgs e)
        {
            if (!chkAdmin.Checked) { MessageBox.Show("Yetki yok!"); return; }
            if (MessageBox.Show("%10 Zam yapılsın mı?", "Onay", MessageBoxButtons.YesNo) == DialogResult.No) return;
            try
            {
                using (MySqlConnection baglanti = new MySqlConnection(baglantiAdresi))
                {
                    baglanti.Open();
                    MySqlCommand komut = new MySqlCommand("SP_TopluFiyatGuncelle", baglanti);
                    komut.CommandType = CommandType.StoredProcedure;
                    komut.Parameters.AddWithValue("@p_Oran", 10);
                    komut.ExecuteNonQuery();
                    MessageBox.Show("Zam yapıldı!");
                    VerileriGetir("");
                }
            }
            catch (Exception hata) { MessageBox.Show("Hata: " + hata.Message); }
        }

        private void textBox4_TextChanged(object sender, EventArgs e) { VerileriGetir(textBox4.Text); }

        private void VerileriGetir(string aranan)
        {
            try
            {
                using (MySqlConnection baglanti = new MySqlConnection(baglantiAdresi))
                {
                    baglanti.Open();
                    string sql = "SELECT *, FN_KDVliFiyat(GunlukFiyat) AS KDVliFiyat FROM View_MusaitAraclar WHERE Plaka LIKE @p OR MarkaAdi LIKE @p";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@p", "%" + aranan + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                    if (lblDurum != null) lblDurum.Visible = (dt.Rows.Count == 0);
                    if (lblDurum != null && lblDurum.Visible) lblDurum.Text = "Kayıt Yok";
                }
            }
            catch (Exception ex) { MessageBox.Show("Veri Çekme Hatası: " + ex.Message); }
        }

        public void textBox1_TextChanged(object sender, EventArgs e) { }
        public void lblDurum_Click(object sender, EventArgs e) { }
    }
}