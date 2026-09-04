using System;
using System.Linq;
using System.Windows.Forms;
using OnTopReplica.Properties;

namespace OnTopReplica.SidePanels {

    /// <summary>
    /// Side panel that stores the current window setup under a name.
    /// </summary>
    partial class SavePresetPanel : SidePanel {

        public SavePresetPanel() {
            InitializeComponent();

            Localize();
        }

        /// <summary>
        /// Localizes the panel's labels.
        /// </summary>
        private void Localize() {
            this.SuspendLayout();

            groupPreset.Text = Strings.PresetsTitle;
            labelName.Text = Strings.PresetsName;
            checkClickThrough.Text = Strings.PresetsClickThrough;
            buttonSave.Text = Strings.PresetsSaveButton;
            buttonCancel.Text = Strings.PresetsCancelButton;

            toolTip.SetToolTip(checkClickThrough, Strings.PresetsClickThroughTT);

            this.ResumeLayout();
        }

        public override string Title {
            get {
                return Strings.PresetsTitle;
            }
        }

        public override void OnFirstShown(MainForm form) {
            base.OnFirstShown(form);

            //Offer the last used preset's name, so that re-saving a preset is a two-click operation.
            textPresetName.Text = Settings.Default.LastPresetName ?? string.Empty;
            textPresetName.SelectAll();
            textPresetName.Focus();

            UpdateState();
        }

        /// <summary>
        /// Refreshes the save button and the overwrite hint from the name currently typed.
        /// </summary>
        private void UpdateState() {
            string name = textPresetName.Text.Trim();

            buttonSave.Enabled = (name.Length > 0);
            labelHint.Text = FindExisting(name) != null ? Strings.PresetsOverwrite : string.Empty;
        }

        /// <summary>
        /// Finds a stored preset by name, or null.
        /// </summary>
        private static StoredRegion FindExisting(string name) {
            if (string.IsNullOrEmpty(name) || Settings.Default.SavedRegions == null)
                return null;

            return Settings.Default.SavedRegions.FirstOrDefault(r =>
                string.Equals(r.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        private void Save() {
            string name = textPresetName.Text.Trim();
            if (name.Length == 0)
                return;

            var preset = ParentMainForm.CapturePreset(name, checkClickThrough.Checked);
            MainForm.StorePreset(preset);

            OnRequestClosing();
        }

        #region GUI event handlers

        private void PresetName_changed(object sender, EventArgs e) {
            UpdateState();
        }

        private void PresetName_keyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                e.Handled = e.SuppressKeyPress = true;
                Save();
            }
            else if (e.KeyCode == Keys.Escape) {
                e.Handled = e.SuppressKeyPress = true;
                OnRequestClosing();
            }
        }

        private void Save_click(object sender, EventArgs e) {
            Save();
        }

        private void Cancel_click(object sender, EventArgs e) {
            OnRequestClosing();
        }

        #endregion

    }

}
