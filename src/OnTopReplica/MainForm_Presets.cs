using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OnTopReplica.Properties;
using OnTopReplica.WindowSeekers;

namespace OnTopReplica {

    //Capture and application of full window presets (see StoredRegion).
    partial class MainForm {

        /// <summary>
        /// Lowest opacity the user can pick from the opacity menu (25%), used to clamp stored values so
        /// that a preset can never make the window invisible.
        /// </summary>
        const byte MinimumPresetOpacity = 64;

        /// <summary>
        /// Set while a preset is being applied.
        /// </summary>
        /// <remarks>
        /// Click-through is normally cleared by OnActivated, which is the user's main way out of it.
        /// Applying a preset causes activation churn of its own (menu teardown, chrome change), so the
        /// guard is suppressed for the duration of the apply and restored immediately afterwards.
        /// </remarks>
        bool _applyingPreset = false;

        /// <summary>
        /// Gets whether the current state can meaningfully be stored as a preset.
        /// </summary>
        /// <remarks>
        /// Fullscreen would record the fullscreen rectangle instead of the user's window, and group
        /// switch mode leaves no single source window to bind to (CurrentThumbnailWindowHandle is
        /// deliberately null there).
        /// </remarks>
        public bool CanCapturePreset {
            get {
                return _thumbnailPanel.IsShowingThumbnail &&
                    !FullscreenManager.IsFullscreen &&
                    !IsGroupSwitchActive;
            }
        }

        private bool IsGroupSwitchActive {
            get {
                try {
                    var manager = _msgPumpManager.Get<MessagePumpProcessors.GroupSwitchManager>();
                    return (manager != null && manager.IsActive);
                }
                catch (Exception) {
                    //Processor not registered (yet): certainly not active.
                    return false;
                }
            }
        }

        #region Capture

        /// <summary>
        /// Captures the current window setup as a named preset.
        /// </summary>
        /// <param name="name">Name of the preset.</param>
        /// <param name="clickThrough">
        /// Whether the preset should enable click-through. This cannot be read from the live state:
        /// OnActivated clears click-through, so it is always false by the time a menu can be used.
        /// </param>
        public StoredRegion CapturePreset(string name, bool clickThrough) {
            var region = SelectedThumbnailRegion;

            var preset = new StoredRegion((region != null) ? region.Clone() : null, name) {
                //Stored raw, without the chrome normalization Program.cs applies to RestoreLastPosition:
                //ApplyPreset restores chrome before the location, so normalizing here would double up.
                WindowLocation = this.Location,
                WindowClientSize = this.ClientSize,
                ChromeVisible = this.IsChromeVisible,
                ClickThrough = clickThrough,
                Opacity = CaptureOpacity()
            };

            var handle = CurrentThumbnailWindowHandle;
            if (handle != null) {
                preset.SourceWindowTitle = handle.Title;
                preset.SourceWindowClass = handle.Class;
            }

            Log.Write("Captured preset '{0}' at {1} size {2}", name, preset.WindowLocation, preset.WindowClientSize);

            return preset;
        }

        /// <summary>
        /// Reads the window's opacity, discarding the two values the application sets on its own behalf.
        /// </summary>
        private byte CaptureOpacity() {
            double opacity = this.Opacity;

            //0.0 means "hidden to tray" on Windows 7 (see Platforms.WindowsSeven), and
            //ClickThroughHoverOpacity is the transient fade applied while hovering in click-through
            //mode. Neither is a value the user picked, so don't store either.
            if (opacity <= 0.0 || opacity == ClickThroughHoverOpacity) {
                return 255;
            }

            int value = (int)Math.Round(opacity * 255.0);
            return (byte)Math.Max(MinimumPresetOpacity, Math.Min(255, value));
        }

        #endregion

        #region Application

        /// <summary>
        /// Applies a stored preset, restoring everything it carries.
        /// </summary>
        /// <param name="handle">
        /// Window to clone. When null, the preset's own source window binding is used to find one.
        /// </param>
        /// <param name="preset">Preset to apply.</param>
        /// <remarks>
        /// The order of operations below is dictated by side effects elsewhere and should not be
        /// rearranged casually:
        /// - SetThumbnail ends in SetAspectRatio(.., forceRefresh: true), which overwrites ClientSize,
        ///   so the size must be applied afterwards.
        /// - IsChromeVisible refuses to hide chrome while no thumbnail is shown, and shifts Location by
        ///   the frame border size itself, so it goes after the thumbnail and before the location.
        /// - ClickForwarding is deliberately NOT part of a preset: it is the only feature that writes
        ///   into the cloned application's message queue, and it stays a conscious manual toggle.
        /// </remarks>
        public void ApplyPreset(WindowHandle handle, StoredRegion preset) {
            if (preset == null)
                return;

            bool deferClickThrough = false;

            _applyingPreset = true;
            try {
                //Normalize any mode that would fight the restored geometry
                if (FullscreenManager.IsFullscreen) {
                    FullscreenManager.SwitchBack();
                }
                if (IsSidePanelOpen) {
                    CloseSidePanel();
                }
                ClickThroughEnabled = false;
                if (preset.WindowLocation.HasValue) {
                    //An absolute location and a screen dock would keep overriding each other.
                    PositionLock = null;
                }

                //Early, so that a later OnActivated -> RestoreForm cannot overwrite it through its
                //"Opacity == 0 means hidden" check.
                if (preset.Opacity.HasValue) {
                    this.Opacity = (double)preset.Opacity.Value / 255.0;
                    Program.Platform.OnFormStateChange(this);
                }

                if (handle == null) {
                    handle = SeekPresetWindow(preset);
                }
                if (handle == null) {
                    Log.Write("Preset '{0}' has no window to clone, applying nothing else", preset.Name);
                    return;
                }

                //Region is passed into SetThumbnail rather than through SelectedThumbnailRegion, whose
                //setter adds another aspect ratio refresh plus FixPositionAndSize and would move the window.
                SetThumbnail(handle, (preset.Region != null) ? preset.Region.Clone() : null);

                //SetThumbnail swallows failures into ThumbnailError -> UnsetThumbnail
                if (!_thumbnailPanel.IsShowingThumbnail) {
                    Log.Write("Preset '{0}' could not clone its window, skipping geometry", preset.Name);
                    return;
                }

                if (preset.ChromeVisible.HasValue) {
                    IsChromeVisible = preset.ChromeVisible.Value;
                }

                if (preset.WindowLocation.HasValue) {
                    this.Location = preset.WindowLocation.Value;
                }

                if (preset.WindowClientSize.HasValue) {
                    //Height is recomputed from the width so that the window still matches the source
                    //window's current aspect ratio, instead of letterboxing if the source has changed size.
                    int width = preset.WindowClientSize.Value.Width;
                    this.ClientSize = new Size(width, ComputeHeightFromWidth(width));
                }

                if (preset.WindowLocation.HasValue || preset.WindowClientSize.HasValue) {
                    EnsureOnAnyScreen();
                }

                deferClickThrough = preset.ClickThrough.GetValueOrDefault();

                Settings.Default.LastPresetName = preset.Name;

                Log.Write("Applied preset '{0}'", preset.Name);
            }
            finally {
                if (deferClickThrough && IsHandleCreated) {
                    //Applied on the next pass of the message loop, so that it lands after any activation
                    //still queued by the menu teardown and the chrome change.
                    BeginInvoke((MethodInvoker)delegate {
                        try {
                            ClickThroughEnabled = true;
                        }
                        finally {
                            _applyingPreset = false;
                        }
                    });
                }
                else {
                    _applyingPreset = false;
                }
            }
        }

        /// <summary>
        /// Looks for the window a preset was bound to.
        /// </summary>
        private WindowHandle SeekPresetWindow(StoredRegion preset) {
            if (!preset.HasSourceWindow)
                return null;

            //A stored HWND is meaningless in a later session and only adds noise to the scoring.
            var seeker = new RestoreWindowSeeker(IntPtr.Zero, preset.SourceWindowTitle, preset.SourceWindowClass) {
                //Mandatory: without it OnTopReplica can match its own window and clone itself.
                OwnerHandle = this.Handle,
                SkipNotVisibleWindows = true
            };
            seeker.Refresh();

            var result = seeker.Windows.FirstOrDefault();
            if (result == null) {
                Log.WriteDetails("Failed to find the window a preset was bound to",
                    "Preset '{0}', title '{1}', class '{2}'",
                    preset.Name, preset.SourceWindowTitle, preset.SourceWindowClass);
            }

            return result;
        }

        /// <summary>
        /// Ensures the window is reachable on some screen, moving it back to the primary one if not.
        /// </summary>
        /// <remarks>
        /// A preset saved on a monitor that is no longer attached would otherwise restore the window
        /// completely off-screen. FixPositionAndSize cannot help: it only clamps right/bottom overflow
        /// against the nearest screen, and leaves a far-off window where it is.
        /// </remarks>
        private void EnsureOnAnyScreen() {
            var bounds = this.Bounds;

            foreach (var screen in Screen.AllScreens) {
                var visible = Rectangle.Intersect(screen.WorkingArea, bounds);
                if (visible.Width >= FixMargin && visible.Height >= FixMargin) {
                    return;
                }
            }

            Log.Write("Preset position {0} is not on any screen, falling back to the primary one", bounds);

            var workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.X + 40, workingArea.Y + 40);
        }

        #endregion

        #region Storage

        /// <summary>
        /// Stores a preset, replacing any existing one with the same name, and persists it immediately.
        /// </summary>
        /// <remarks>
        /// Settings are otherwise only written on clean shutdown, which would lose a new preset to any
        /// crash or task-kill.
        /// </remarks>
        public static void StorePreset(StoredRegion preset) {
            if (Settings.Default.SavedRegions == null) {
                Settings.Default.SavedRegions = new StoredRegionArray();
            }

            var regions = Settings.Default.SavedRegions;

            int existing = regions.FindIndex(r =>
                string.Equals(r.Name, preset.Name, StringComparison.CurrentCultureIgnoreCase));

            if (existing >= 0) {
                regions[existing] = preset;
            }
            else {
                regions.Add(preset);
            }

            PersistSettings();
        }

        /// <summary>
        /// Writes the settings out, tolerating a profile that cannot be written to.
        /// </summary>
        public static void PersistSettings() {
            try {
                Settings.Default.Save();
            }
            catch (Exception ex) {
                Log.WriteException("Failed to persist presets", ex);
            }
        }

        #endregion

    }

}
