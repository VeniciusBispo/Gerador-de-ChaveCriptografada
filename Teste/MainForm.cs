using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApp3
{
    public partial class MainForm : Form
    {
        private TextBox txtUserName;
        private Button btnGenerateCert;
        private TextBox txtKey;

        public MainForm()
        {
            SetupForm();
            SetupControls();
        }

        private void SetupForm()
        {
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10);
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetupControls()
        {
            Label label1 = new Label
            {
                Text = "Gerar Certificado",
                Location = new Point(30, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(label1);

            Label labelUserName = new Label
            {
                Text = "Nome do Usuário:",
                Location = new Point(30, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White
            };
            this.Controls.Add(labelUserName);

            txtUserName = new TextBox
            {
                Location = new Point(30, 85),
                Width = 300,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Black
            };
            this.Controls.Add(txtUserName);

            Label labelKey = new Label
            {
                Text = "Chave gerada:",
                Location = new Point(30, 130),
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White
            };
            this.Controls.Add(labelKey);

            txtKey = new TextBox
            {
                Location = new Point(30, 155),
                Width = 300,
                Height = 60,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Black,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtKey);

            btnGenerateCert = new Button
            {
                Text = "Gerar Certificado",
                Location = new Point(30, 230),
                Width = 300,
                Height = 40,
                BackColor = Color.FromArgb(0, 113, 150),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White
            };
            btnGenerateCert.Click += btnGenerateCert_Click;
            this.Controls.Add(btnGenerateCert);
        }

        private void btnGenerateCert_Click(object sender, EventArgs e)
        {
            var userName = txtUserName.Text;

            var userData = GenerateRandomString(2000);
            var firstPart = userData.Substring(0, 1000);
            var secondPart = userData.Substring(1000, 1000);

            var (encryptedFirstPart, key, iv) = EncryptDataAes(firstPart);
            var (encryptedSecondPart, _, _) = EncryptDataAes(secondPart); // Use o mesmo key e iv para ambos

            var combinedEncryptedData = encryptedFirstPart + "::" + encryptedSecondPart;

            txtKey.Text = combinedEncryptedData;

            var xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement("Certificado");
            xmlDocument.AppendChild(root);

            var userNameElement = xmlDocument.CreateElement("NomeUsuario");
            userNameElement.InnerText = userName;
            root.AppendChild(userNameElement);

            var encryptedElement = xmlDocument.CreateElement("ChaveCriptografada");
            encryptedElement.InnerText = combinedEncryptedData;
            root.AppendChild(encryptedElement);

            var keyElement = xmlDocument.CreateElement("Chave");
            keyElement.InnerText = key;
            root.AppendChild(keyElement);

            var ivElement = xmlDocument.CreateElement("IV");
            ivElement.InnerText = iv;
            root.AppendChild(ivElement);

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


        private string GenerateRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringBuilder = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }
            return stringBuilder.ToString();
        }

        private (string EncryptedData, string Key, string IV) EncryptDataAes(string data)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateKey();
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }

                    var encryptedData = ms.ToArray();
                    var encryptedBase64 = Convert.ToBase64String(encryptedData);

                    // Substitua ":" por "_" em cada parte
                    encryptedBase64 = encryptedBase64.Replace(":", "_");

                    // Retorne o resultado
                    return (encryptedBase64, Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
                }
            }
        }

    }
}
