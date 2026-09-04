namespace OnTopReplica.SidePanels {
    partial class SavePresetPanel {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.groupPreset = new System.Windows.Forms.GroupBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textPresetName = new System.Windows.Forms.TextBox();
            this.checkClickThrough = new System.Windows.Forms.CheckBox();
            this.labelHint = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupPreset.SuspendLayout();
            this.SuspendLayout();
            //
            // groupPreset
            //
            this.groupPreset.Controls.Add(this.labelName);
            this.groupPreset.Controls.Add(this.textPresetName);
            this.groupPreset.Controls.Add(this.checkClickThrough);
            this.groupPreset.Controls.Add(this.labelHint);
            this.groupPreset.Controls.Add(this.buttonSave);
            this.groupPreset.Controls.Add(this.buttonCancel);
            this.groupPreset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupPreset.Location = new System.Drawing.Point(6, 6);
            this.groupPreset.Name = "groupPreset";
            this.groupPreset.Size = new System.Drawing.Size(259, 156);
            this.groupPreset.TabIndex = 0;
            this.groupPreset.TabStop = false;
            this.groupPreset.Text = "Save preset";
            //
            // labelName
            //
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(9, 26);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(72, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Preset name:";
            //
            // textPresetName
            //
            this.textPresetName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textPresetName.Location = new System.Drawing.Point(12, 44);
            this.textPresetName.MaxLength = 64;
            this.textPresetName.Name = "textPresetName";
            this.textPresetName.Size = new System.Drawing.Size(235, 20);
            this.textPresetName.TabIndex = 1;
            this.textPresetName.TextChanged += new System.EventHandler(this.PresetName_changed);
            this.textPresetName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PresetName_keyDown);
            //
            // checkClickThrough
            //
            this.checkClickThrough.AutoSize = true;
            this.checkClickThrough.Location = new System.Drawing.Point(12, 76);
            this.checkClickThrough.Name = "checkClickThrough";
            this.checkClickThrough.Size = new System.Drawing.Size(129, 17);
            this.checkClickThrough.TabIndex = 2;
            this.checkClickThrough.Text = "Enable click-through";
            this.checkClickThrough.UseVisualStyleBackColor = true;
            //
            // labelHint
            //
            this.labelHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Location = new System.Drawing.Point(9, 99);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(238, 17);
            this.labelHint.TabIndex = 3;
            this.labelHint.Text = "";
            //
            // buttonSave
            //
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(172, 124);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.Save_click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(91, 124);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.Cancel_click);
            //
            // SavePresetPanel
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupPreset);
            this.MinimumSize = new System.Drawing.Size(240, 168);
            this.Name = "SavePresetPanel";
            this.Padding = new System.Windows.Forms.Padding(6);
            this.Size = new System.Drawing.Size(271, 168);
            this.groupPreset.ResumeLayout(false);
            this.groupPreset.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupPreset;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textPresetName;
        private System.Windows.Forms.CheckBox checkClickThrough;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
