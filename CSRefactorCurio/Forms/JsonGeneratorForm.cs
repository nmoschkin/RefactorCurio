﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSRefactorCurio.Forms
{
    public partial class JsonGeneratorForm : Form
    {
        EnvDTE.ProjectItem context;

        public JsonGeneratorForm()
        {
            InitializeComponent();
            txtJson.TextChanged += TxtJson_TextChanged;
            txtJson.KeyDown += TxtJson_KeyDown;
        }

        public JsonGeneratorForm(EnvDTE.ProjectItem context) : this()
        {
            
            this.context = context;
            
            if (context.ContainingProject is EnvDTE.Project proj)
            {
                var en = proj.Properties;

                foreach (EnvDTE.Property prop in en)
                {
                    if (prop.Name == "DefaultNamespace")
                    {
                     
                        if (prop.Value is string s)
                        {
                            txtNamespace.Text = s;
                        }

                        break;
                    }
                }

                if (string.IsNullOrEmpty(txtNamespace.Text))
                {
                    txtNamespace.Text = proj.Name;
                }
            }
        }

        private void TxtJson_TextChanged(object sender, EventArgs e)
        {
            ValidateJson();
        }

        private void TxtJson_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Modifiers == Keys.Control)
            //{
            //    if (e.KeyCode == Keys.V)
            //    {
            //        txtJson.Paste();
            //    }
            //    else if (e.KeyCode == Keys.C)
            //    {
            //        txtJson.Copy();
            //    }
            //    else if (e.KeyCode == Keys.X)
            //    {
            //        txtJson.Cut();
            //    }
            //}
        }

        public string JsonText
        {
            get => txtJson.Text;
            set => txtJson.Text = value;
        }

        public void PasteFromClipboard()
        {
            Clipboard.SetText(JsonText);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtJson.Clear();
        }

        private void PasteJSONForm_Load(object sender, EventArgs e)
        {
            ValidateJson();
        }

        private bool ValidateJson()
        {
            
            if (!string.IsNullOrEmpty(txtJson.Text))
            {
                try
                {
                    JsonConvert.DeserializeObject<JToken>(txtJson.Text.Trim());
                    
                    lblInvalid.Visible = false;
                    btnOK.Enabled = true;
                    return true;
                }
                catch
                {
                }
            }

            lblInvalid.Visible = true;
            btnOK.Enabled = false;

            return false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {

        }
    }
}
