namespace Deploy
{
    partial class ConfigureControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxOuter = new System.Windows.Forms.GroupBox();
            this.labelNotice = new System.Windows.Forms.Label();
            this.textBoxConfigurationString = new System.Windows.Forms.TextBox();
            this.timerTimeout = new System.Windows.Forms.Timer(this.components);
            this.groupBoxOuter.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxOuter
            // 
            this.groupBoxOuter.Controls.Add(this.labelNotice);
            this.groupBoxOuter.Controls.Add(this.textBoxConfigurationString);
            this.groupBoxOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOuter.Location = new System.Drawing.Point(0, 0);
            this.groupBoxOuter.Name = "groupBoxOuter";
            this.groupBoxOuter.Size = new System.Drawing.Size(404, 142);
            this.groupBoxOuter.TabIndex = 0;
            this.groupBoxOuter.TabStop = false;
            this.groupBoxOuter.Text = "Configuration";
            this.groupBoxOuter.Enter += new System.EventHandler(this.groupBoxOuter_Enter);
            // 
            // labelNotice
            // 
            this.labelNotice.AutoEllipsis = true;
            this.labelNotice.BackColor = System.Drawing.SystemColors.Info;
            this.labelNotice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelNotice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelNotice.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNotice.ForeColor = System.Drawing.SystemColors.InfoText;
            this.labelNotice.Location = new System.Drawing.Point(3, 38);
            this.labelNotice.Margin = new System.Windows.Forms.Padding(1, 10, 1, 4);
            this.labelNotice.Name = "labelNotice";
            this.labelNotice.Size = new System.Drawing.Size(398, 101);
            this.labelNotice.TabIndex = 1;
            this.labelNotice.Text = "Notice Information";
            this.labelNotice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelNotice.Click += new System.EventHandler(this.labelNotice_Click);
            // 
            // textBoxConfigurationString
            // 
            this.textBoxConfigurationString.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBoxConfigurationString.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxConfigurationString.Location = new System.Drawing.Point(3, 16);
            this.textBoxConfigurationString.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            this.textBoxConfigurationString.Name = "textBoxConfigurationString";
            this.textBoxConfigurationString.ReadOnly = true;
            this.textBoxConfigurationString.Size = new System.Drawing.Size(398, 22);
            this.textBoxConfigurationString.TabIndex = 0;
            // 
            // timerTimeout
            // 
            this.timerTimeout.Enabled = true;
            this.timerTimeout.Interval = 250;
            this.timerTimeout.Tick += new System.EventHandler(this.timerTimeout_Tick);
            // 
            // ConfigureControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxOuter);
            this.MinimumSize = new System.Drawing.Size(0, 142);
            this.Name = "ConfigureControl";
            this.Size = new System.Drawing.Size(404, 142);
            this.groupBoxOuter.ResumeLayout(false);
            this.groupBoxOuter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxOuter;
        private System.Windows.Forms.TextBox textBoxConfigurationString;
        private System.Windows.Forms.Timer timerTimeout;
        private System.Windows.Forms.Label labelNotice;
    }
}
