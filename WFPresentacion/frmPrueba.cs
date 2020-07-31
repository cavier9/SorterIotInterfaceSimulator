using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFPresentacion.CustomView;
using WFPresentacion.ViewModel;

namespace WFPresentacion
{
    public partial class frmPrueba : Form
    {
        MainWindowViewModel2 viewModel;
        CustomTextBox MessageLogTextBox1;
        string pathtxt;


        public frmPrueba()
        {
            InitializeComponent();
            MessageLogTextBox1 = new CustomTextBox();
            this.viewModel = new MainWindowViewModel2( dgserialescargados, dgTotalesaceptados, dgRechazados, this,lbltotales,this.btnExporta);

            txtip.Text = viewModel.TcpAddress;
            txttcpport.Text = viewModel.TcpPortNo;
            pathtxt = Directory.GetCurrentDirectory() ;
        }
              
        private void btnConectar_Click(object sender, EventArgs e)
        {
         
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (btnConectar.Text == "CONECTAR")
                {
                    if (!this.viewModel.TcpConnectFunc())
                    {
                        return;
                    }
                    btnConectar.Text = "DESCONECTAR";
                    btnSend.Enabled = true;
                    btnConectar.BackColor = Color.Green;
                    btnSend_Click(null, new EventArgs());
                }
                else
                {
                    this.viewModel.TcpDisconnectFunc();
                    btnConectar.Text = "CONECTAR";
                    btnSend.Enabled = false;
                    btnConectar.BackColor = Color.Gray;
                    //btnSend_Click(null, new EventArgs());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string[] lines;
                if (btnSend.Text == "CREAR SESSION")
                {
                    frmUsuario frmUsuario = new frmUsuario();
                    frmUsuario.ShowDialog();
                    if (string.IsNullOrEmpty(frmUsuario.Usuario))
                    {
                        return;
                    }


                    lines = System.IO.File.ReadAllLines(pathtxt + @"\Message\001_CreateSession.txt");
                    this.viewModel.SendMessage = "";
                    foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        this.viewModel.SendMessage += "\t" + line;
                    }
                    this.viewModel.TcpSendJsonFunc();


                    lines = System.IO.File.ReadAllLines(pathtxt + @"\Message\103_GetDenominationInformation.txt");
                    this.viewModel.SendMessage = "";
                    foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        this.viewModel.SendMessage += "\t" + line;
                    }
                    this.viewModel.TcpSendJsonFunc();

                    lines = System.IO.File.ReadAllLines(pathtxt + @"\Message\205_GetCurrency.txt");
                    this.viewModel.SendMessage = "";
                    foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        this.viewModel.SendMessage += "\t" + line;
                    }
                    this.viewModel.TcpSendJsonFunc();


                    btnSend.Text = "CERRAR SESSION";
                    btnSend.BackColor = Color.Green;
                    this.lblusuario.Text = frmUsuario.Usuario;
                    this.btnConectar.Enabled = false;
                }
                else
                {
                    lines = System.IO.File.ReadAllLines(pathtxt + @"\Message\002_CloseSession.txt");
                    this.viewModel.SendMessage = "";
                    foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        this.viewModel.SendMessage += "\t" + line;
                    }
                    this.viewModel.TcpSendJsonFunc();
                    btnSend.Text = "CREAR SESSION";
                    btnSend.BackColor = Color.Gray;
                    this.lblusuario.Text = "";
                    this.btnConectar.Enabled = true;

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void frmPrueba_Load(object sender, EventArgs e)
        {
           
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            viewModel.Limpiar();
        }

        private void btnExporta_Click(object sender, EventArgs e)
        {

            viewModel.ExportarInformacion(this.lblusuario.Text);
            btnSend_Click(null, new EventArgs());
            btnSend_Click(null, new EventArgs());
        }

        private void frmPrueba_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.Dispose();
                this.viewModel = null;
            }
        }
    }
}
