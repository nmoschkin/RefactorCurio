namespace CSRefactorCurio.Forms
{
    partial class JsonGeneratorForm
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
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtJson = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblInvalid = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.txtSaveAs = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkGenerate = new System.Windows.Forms.CheckBox();
            this.cbFloats = new System.Windows.Forms.ComboBox();
            this.lblFloat = new System.Windows.Forms.Label();
            this.lblInts = new System.Windows.Forms.Label();
            this.cbInts = new System.Windows.Forms.ComboBox();
            this.lblDefault = new System.Windows.Forms.Label();
            this.cbDefault = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(12, 9);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(173, 13);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "Enter or &Paste JSON Snippet Here:";
            // 
            // txtJson
            // 
            this.txtJson.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJson.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtJson.Location = new System.Drawing.Point(15, 25);
            this.txtJson.Multiline = true;
            this.txtJson.Name = "txtJson";
            this.txtJson.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtJson.Size = new System.Drawing.Size(540, 250);
            this.txtJson.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(477, 452);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(396, 452);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(12, 452);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 18;
            this.btnClear.Text = "Cl&ear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lblInvalid
            // 
            this.lblInvalid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblInvalid.AutoSize = true;
            this.lblInvalid.ForeColor = System.Drawing.Color.Red;
            this.lblInvalid.Location = new System.Drawing.Point(93, 457);
            this.lblInvalid.Name = "lblInvalid";
            this.lblInvalid.Size = new System.Drawing.Size(110, 13);
            this.lblInvalid.TabIndex = 19;
            this.lblInvalid.Text = "JSON String Is Invalid";
            this.lblInvalid.Visible = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 374);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "&Name of New Class:";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(121, 371);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(431, 20);
            this.txtName.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(48, 400);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "N&amespace:";
            // 
            // txtNamespace
            // 
            this.txtNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNamespace.Location = new System.Drawing.Point(121, 397);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(431, 20);
            this.txtNamespace.TabIndex = 12;
            // 
            // txtSaveAs
            // 
            this.txtSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaveAs.Location = new System.Drawing.Point(121, 423);
            this.txtSaveAs.Name = "txtSaveAs";
            this.txtSaveAs.Size = new System.Drawing.Size(403, 20);
            this.txtSaveAs.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(59, 426);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "&Save As...";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(525, 420);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(27, 23);
            this.btnBrowse.TabIndex = 15;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // chkGenerate
            // 
            this.chkGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkGenerate.AutoSize = true;
            this.chkGenerate.Location = new System.Drawing.Point(15, 281);
            this.chkGenerate.Name = "chkGenerate";
            this.chkGenerate.Size = new System.Drawing.Size(184, 17);
            this.chkGenerate.TabIndex = 2;
            this.chkGenerate.Text = "&Generate Time Converter Classes";
            this.chkGenerate.UseVisualStyleBackColor = true;
            // 
            // cbFloats
            // 
            this.cbFloats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbFloats.FormattingEnabled = true;
            this.cbFloats.Items.AddRange(new object[] {
            "Double",
            "Decimal"});
            this.cbFloats.Location = new System.Drawing.Point(15, 326);
            this.cbFloats.Name = "cbFloats";
            this.cbFloats.Size = new System.Drawing.Size(121, 21);
            this.cbFloats.TabIndex = 4;
            this.cbFloats.Text = "Double";
            // 
            // lblFloat
            // 
            this.lblFloat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblFloat.AutoSize = true;
            this.lblFloat.Location = new System.Drawing.Point(12, 310);
            this.lblFloat.Name = "lblFloat";
            this.lblFloat.Size = new System.Drawing.Size(97, 13);
            this.lblFloat.TabIndex = 3;
            this.lblFloat.Text = "Float Number Type";
            // 
            // lblInts
            // 
            this.lblInts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInts.AutoSize = true;
            this.lblInts.Location = new System.Drawing.Point(226, 310);
            this.lblInts.Name = "lblInts";
            this.lblInts.Size = new System.Drawing.Size(107, 13);
            this.lblInts.TabIndex = 5;
            this.lblInts.Text = "Integer Number Type";
            // 
            // cbInts
            // 
            this.cbInts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbInts.FormattingEnabled = true;
            this.cbInts.Items.AddRange(new object[] {
            "Long",
            "Int"});
            this.cbInts.Location = new System.Drawing.Point(229, 326);
            this.cbInts.Name = "cbInts";
            this.cbInts.Size = new System.Drawing.Size(112, 21);
            this.cbInts.TabIndex = 6;
            this.cbInts.Text = "Long";
            // 
            // lblDefault
            // 
            this.lblDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDefault.AutoSize = true;
            this.lblDefault.Location = new System.Drawing.Point(433, 310);
            this.lblDefault.Name = "lblDefault";
            this.lblDefault.Size = new System.Drawing.Size(119, 13);
            this.lblDefault.TabIndex = 7;
            this.lblDefault.Text = "Indeterminate Numbers:";
            // 
            // cbDefault
            // 
            this.cbDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDefault.FormattingEnabled = true;
            this.cbDefault.Items.AddRange(new object[] {
            "Floating Point",
            "Integer"});
            this.cbDefault.Location = new System.Drawing.Point(436, 326);
            this.cbDefault.Name = "cbDefault";
            this.cbDefault.Size = new System.Drawing.Size(121, 21);
            this.cbDefault.TabIndex = 8;
            this.cbDefault.Text = "Floating Point";
            // 
            // PasteJSONForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(564, 487);
            this.Controls.Add(this.lblDefault);
            this.Controls.Add(this.cbDefault);
            this.Controls.Add(this.lblInts);
            this.Controls.Add(this.cbInts);
            this.Controls.Add(this.lblFloat);
            this.Controls.Add(this.cbFloats);
            this.Controls.Add(this.chkGenerate);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtSaveAs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblInvalid);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtJson);
            this.Controls.Add(this.lblPrompt);
            this.Name = "PasteJSONForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Enter/Paste JSON String";
            this.Load += new System.EventHandler(this.PasteJSONForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.TextBox txtJson;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblInvalid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.TextBox txtSaveAs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkGenerate;
        private System.Windows.Forms.ComboBox cbFloats;
        private System.Windows.Forms.Label lblFloat;
        private System.Windows.Forms.Label lblInts;
        private System.Windows.Forms.ComboBox cbInts;
        private System.Windows.Forms.Label lblDefault;
        private System.Windows.Forms.ComboBox cbDefault;
    }
}