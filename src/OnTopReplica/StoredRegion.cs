using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Drawing;

namespace OnTopReplica {

	/// <summary>
	/// A named, saved setup of the replica window.
	/// </summary>
	/// <remarks>
	/// Originally this held nothing but a name and a crop region, and entries saved by older versions
	/// still look like that: every window setting below is nullable and a null simply means "this preset
	/// does not control that, leave whatever the window currently has".
	/// Note that <see cref="Region"/> itself may be null, meaning the whole source window is cloned.
	/// </remarks>
	public class StoredRegion {

        public StoredRegion(ThumbnailRegion r, string name) {
            Region = r;
            Name = name;
        }

        public ThumbnailRegion Region {
            get;
            set;
        }

		public string Name {
			get;
			set;
		}

        #region Window settings

        /// <summary>
        /// Location of the replica window, stored raw (i.e. as-is, whether or not chrome was visible).
        /// </summary>
        /// <remarks>
        /// Deliberately NOT normalized the way Program.cs does it for RestoreLastPosition. That code
        /// re-enables chrome before reading Location because Options.Apply restores chrome *after* the
        /// position. Presets restore chrome *before* the position, so normalizing here as well would
        /// compensate twice and land the window one frame border off.
        /// </remarks>
        public Point? WindowLocation {
            get;
            set;
        }

        /// <summary>
        /// Client size of the replica window.
        /// </summary>
        /// <remarks>
        /// The height is recomputed from the width on restore, so that the window keeps matching the
        /// source window's current aspect ratio instead of letterboxing.
        /// </remarks>
        public Size? WindowClientSize {
            get;
            set;
        }

        /// <summary>
        /// Window opacity, from 0 to 255 (matching the CLI's Options.Opacity).
        /// </summary>
        public byte? Opacity {
            get;
            set;
        }

        /// <summary>
        /// Whether click-through should be enabled when this preset is applied.
        /// </summary>
        public bool? ClickThrough {
            get;
            set;
        }

        /// <summary>
        /// Whether the window chrome should be visible.
        /// </summary>
        public bool? ChromeVisible {
            get;
            set;
        }

        /// <summary>
        /// Title of the cloned window, used to find it again in a later session.
        /// </summary>
        public string SourceWindowTitle {
            get;
            set;
        }

        /// <summary>
        /// Class of the cloned window, used to find it again in a later session.
        /// </summary>
        public string SourceWindowClass {
            get;
            set;
        }

        /// <summary>
        /// True if this preset carries any window setting at all (i.e. it is not a plain crop region
        /// saved by an older version of the application).
        /// </summary>
        public bool HasWindowSettings {
            get {
                return WindowLocation.HasValue || WindowClientSize.HasValue || Opacity.HasValue ||
                    ClickThrough.HasValue || ChromeVisible.HasValue ||
                    !string.IsNullOrEmpty(SourceWindowTitle) || !string.IsNullOrEmpty(SourceWindowClass);
            }
        }

        /// <summary>
        /// True if this preset knows which window it was cloning.
        /// </summary>
        public bool HasSourceWindow {
            get {
                return !string.IsNullOrEmpty(SourceWindowTitle) || !string.IsNullOrEmpty(SourceWindowClass);
            }
        }

        #endregion

        public override string ToString() {
			return Name;
		}

	}

}
