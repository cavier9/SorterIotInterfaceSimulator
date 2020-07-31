namespace WFPresentacion
{
    partial class frmPrueba
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txttcpport = new System.Windows.Forms.TextBox();
            this.txtip = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnConectar = new System.Windows.Forms.Button();
            this.dgserialescargados = new System.Windows.Forms.DataGridView();
            this.lblusuario = new System.Windows.Forms.Label();
            this.lbltotales = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExporta = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dgRechazados = new System.Windows.Forms.DataGridView();
            this.label4 = new System.Windows.Forms.Label();
            this.dgTotalesaceptados = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgserialescargados)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgRechazados)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgTotalesaceptados)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txttcpport
            // 
            this.txttcpport.Location = new System.Drawing.Point(21, 45);
            this.txttcpport.Name = "txttcpport";
            this.txttcpport.ReadOnly = true;
            this.txttcpport.Size = new System.Drawing.Size(100, 22);
            this.txttcpport.TabIndex = 6;
            // 
            // txtip
            // 
            this.txtip.Location = new System.Drawing.Point(21, 17);
            this.txtip.Name = "txtip";
            this.txtip.ReadOnly = true;
            this.txtip.Size = new System.Drawing.Size(100, 22);
            this.txtip.TabIndex = 7;
            // 
            // btnSend
            // 
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(292, 17);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(147, 62);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "CREAR SESSION";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnConectar
            // 
            this.btnConectar.Location = new System.Drawing.Point(137, 17);
            this.btnConectar.Name = "btnConectar";
            this.btnConectar.Size = new System.Drawing.Size(149, 62);
            this.btnConectar.TabIndex = 5;
            this.btnConectar.Text = "CONECTAR";
            this.btnConectar.UseVisualStyleBackColor = true;
            this.btnConectar.Click += new System.EventHandler(this.btnConectar_Click);
            // 
            // dgserialescargados
            // 
            this.dgserialescargados.AllowUserToAddRows = false;
            this.dgserialescargados.AllowUserToDeleteRows = false;
            this.dgserialescargados.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgserialescargados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgserialescargados.Location = new System.Drawing.Point(12, 121);
            this.dgserialescargados.Name = "dgserialescargados";
            this.dgserialescargados.ReadOnly = true;
            this.dgserialescargados.RowHeadersWidth = 51;
            this.dgserialescargados.RowTemplate.Height = 24;
            this.dgserialescargados.Size = new System.Drawing.Size(782, 480);
            this.dgserialescargados.TabIndex = 8;
            // 
            // lblusuario
            // 
            this.lblusuario.AutoSize = true;
            this.lblusuario.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblusuario.Location = new System.Drawing.Point(458, 17);
            this.lblusuario.Name = "lblusuario";
            this.lblusuario.Size = new System.Drawing.Size(18, 25);
            this.lblusuario.TabIndex = 9;
            this.lblusuario.Text = ".";
            // 
            // lbltotales
            // 
            this.lbltotales.AutoSize = true;
            this.lbltotales.Location = new System.Drawing.Point(558, 62);
            this.lbltotales.Name = "lbltotales";
            this.lbltotales.Size = new System.Drawing.Size(16, 17);
            this.lbltotales.TabIndex = 11;
            this.lbltotales.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(458, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "Contados:";
            // 
            // btnExporta
            // 
            this.btnExporta.Enabled = false;
            this.btnExporta.Location = new System.Drawing.Point(803, 17);
            this.btnExporta.Name = "btnExporta";
            this.btnExporta.Size = new System.Drawing.Size(143, 62);
            this.btnExporta.TabIndex = 4;
            this.btnExporta.Text = "\"GUARDAR \r\n &&  \r\nBORRAR\"\r\n";
            this.btnExporta.UseVisualStyleBackColor = true;
            this.btnExporta.Click += new System.EventHandler(this.btnExporta_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 17);
            this.label2.TabIndex = 12;
            this.label2.Text = "Series";
            // 
            // dgRechazados
            // 
            this.dgRechazados.AllowUserToAddRows = false;
            this.dgRechazados.AllowUserToDeleteRows = false;
            this.dgRechazados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRechazados.Location = new System.Drawing.Point(803, 484);
            this.dgRechazados.Name = "dgRechazados";
            this.dgRechazados.ReadOnly = true;
            this.dgRechazados.RowHeadersWidth = 51;
            this.dgRechazados.RowTemplate.Height = 24;
            this.dgRechazados.Size = new System.Drawing.Size(536, 119);
            this.dgRechazados.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(800, 464);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(190, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "BILLETES RECHAZADOS";
            // 
            // dgTotalesaceptados
            // 
            this.dgTotalesaceptados.AllowUserToAddRows = false;
            this.dgTotalesaceptados.AllowUserToDeleteRows = false;
            this.dgTotalesaceptados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgTotalesaceptados.Location = new System.Drawing.Point(803, 121);
            this.dgTotalesaceptados.Name = "dgTotalesaceptados";
            this.dgTotalesaceptados.ReadOnly = true;
            this.dgTotalesaceptados.RowHeadersWidth = 51;
            this.dgTotalesaceptados.RowTemplate.Height = 24;
            this.dgTotalesaceptados.Size = new System.Drawing.Size(533, 322);
            this.dgTotalesaceptados.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(800, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(178, 17);
            this.label3.TabIndex = 16;
            this.label3.Text = "BILLETES ACEPTADOS";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::WFPresentacion.Properties.Resources.cashmaching;
            this.pictureBox1.Location = new System.Drawing.Point(969, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(303, 71);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            // 
            // frmPrueba
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 628);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.dgRechazados);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dgTotalesaceptados);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbltotales);
            this.Controls.Add(this.lblusuario);
            this.Controls.Add(this.txttcpport);
            this.Controls.Add(this.txtip);
            this.Controls.Add(this.btnExporta);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnConectar);
            this.Controls.Add(this.dgserialescargados);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrueba";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "USF-200 Extracion TCP de Datos";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPrueba_FormClosing);
            this.Load += new System.EventHandler(this.frmPrueba_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgserialescargados)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgRechazados)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgTotalesaceptados)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txttcpport;
        private System.Windows.Forms.TextBox txtip;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnConectar;
        private System.Windows.Forms.DataGridView dgserialescargados;
        private System.Windows.Forms.Label lblusuario;
        private System.Windows.Forms.Label lbltotales;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnExporta;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dgRechazados;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dgTotalesaceptados;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}