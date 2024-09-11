using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Npgsql;

namespace WindowsFormsApp3
{
    public partial class LoginForm : Form
    {
        private TextBox txtUser, txtPassword, txtHost, txtDbName;
        private Button btnConnect, btnGenerateCert;
        private Panel panel;
        private bool isDragging = false;
        private Point lastCursor;

        public LoginForm()
        {
            InitializeComponent();
            SetupForm();
            SetupControls();
            ApplyStyles();
        }

        private void SetupForm()
        {
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10);
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;

            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;
        }

        private void SetupControls()
        {
            Label label1 = new Label
            {
                Text = "Importação de Produtos",
                Location = new Point(30, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(label1);

            panel = new Panel
            {
                Location = new Point(50, 100),
                Size = new Size(300, 320),
                Padding = new Padding(10)
            };
            this.Controls.Add(panel);

            AddControlWithLabel("Usuário", "", new Point(40, 22), ref txtUser);
            AddControlWithLabel("Senha", "", new Point(40, 74), ref txtPassword, true);
            AddControlWithLabel("Nome Base", "base_new", new Point(40, 130), ref txtDbName);
            AddControlWithLabel("IP Base", "127.0.0.1", new Point(40, 185), ref txtHost);

            btnConnect = CreateRoundedButton("Entrar", new Point(30, 270));
            btnConnect.Click += btnConnect_Click;
            panel.Controls.Add(btnConnect);

            btnGenerateCert = CreateRoundedButton("Gerar Certificado", new Point(30, 320));
            btnGenerateCert.Click += btnGenerateCert_Click;
            panel.Controls.Add(btnGenerateCert);
            btnGenerateCert.Enabled = false;
        }

        private void ApplyStyles()
        {
            this.Region = CreateRoundedRegion();
        }

        private void AddControlWithLabel(string labelText, string defaultText, Point location, ref TextBox textBox, bool isPassword = false)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(10, panel.Controls.Count * 26),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            panel.Controls.Add(label);

            textBox = CreateRoundedTextBox(defaultText, location, labelText);
            if (isPassword)
            {
                textBox.UseSystemPasswordChar = true;
            }
            panel.Controls.Add(textBox);
        }

        private Region CreateRoundedRegion()
        {
            var radius = 40;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            var region = new Region(path);
            return region;
        }

        private TextBox CreateRoundedTextBox(string defaultText, Point location, string placeholderText)
        {
            var textBox = new TextBox
            {
                Location = location,
                Width = 220,
                Text = defaultText,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15),
                Padding = new Padding(10)
            };

            textBox.GotFocus += (sender, e) =>
            {
                if (textBox.Text == defaultText) textBox.Text = "";
            };

            textBox.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text)) textBox.Text = defaultText;
            };

            textBox.Paint += (s, e) =>
            {
                var graphics = e.Graphics;
                var rect = textBox.ClientRectangle;
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                var radius = 10;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(rect.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(rect.Width - radius, rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                graphics.FillPath(new SolidBrush(Color.FromArgb(45, 45, 48)), path);
                graphics.DrawPath(Pens.White, path);
            };

            return textBox;
        }

        private Button CreateRoundedButton(string text, Point location)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Width = 230,
                Height = 40,
                BackColor = Color.FromArgb(0, 113, 150),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 98, 130);
            return button;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtDbName.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string host = txtHost.Text;
            string user = txtUser.Text;
            string password = txtPassword.Text;
            string dbname = txtDbName.Text;

            using (var dbConnection = new DatabaseConnection(host, user, password, dbname))
            {
                if (dbConnection.TryConnect(out var connection))
                {
                    MessageBox.Show("Conexão ao banco de dados estabelecida com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Abre o MainForm
                    MainForm mainForm = new MainForm();
                    mainForm.Show();

                    // Fecha o LoginForm
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Falha ao conectar ao banco de dados. Verifique as credenciais e tente novamente.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnGenerateCert_Click(object sender, EventArgs e)
        {
            var userData = $"{txtUser.Text}_{txtDbName.Text}";
            var encryptedData = EncryptData(userData);

            var xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement("Certificado");
            xmlDocument.AppendChild(root);

            var encryptedElement = xmlDocument.CreateElement("ChaveCriptografada");
            encryptedElement.InnerText = encryptedData;
            root.AppendChild(encryptedElement);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Arquivo XML|*.xml",
                Title = "Salvar Certificado"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                xmlDocument.Save(saveFileDialog.FileName);
                MessageBox.Show("Certificado gerado e salvo com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string EncryptData(string data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateKey();
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }

                    var encryptedData = ms.ToArray();
                    var result = Convert.ToBase64String(encryptedData) + ":" + Convert.ToBase64String(aes.Key) + ":" + Convert.ToBase64String(aes.IV);
                    return result;
                }
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastCursor = e.Location;
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var currentCursorScreenPosition = PointToScreen(e.Location);
                Location = new Point(currentCursorScreenPosition.X - lastCursor.X, currentCursorScreenPosition.Y - lastCursor.Y);
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

    }

    public class DatabaseConnection : IDisposable
    {
        private readonly string connectionString;
        private NpgsqlConnection connection;

        public DatabaseConnection(string host, string user, string password, string dbName)
        {
            connectionString = $"Host={host};Username={user};Password={password};Database={dbName}";
        }

        public bool TryConnect(out NpgsqlConnection npgsqlConnection)
        {
            npgsqlConnection = null;
            try
            {
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
                npgsqlConnection = connection;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
        }
    }
}
