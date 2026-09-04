using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace OnTopReplica {

    /// <summary>
    /// Strongly styped array of StoredRegion elements.
    /// </summary>
    /// <remarks>
    /// Handles XML serialization.
    /// There is no attribute-driven serializer behind this: any field added to StoredRegion must be
    /// written here by hand in BOTH ReadXml and WriteXml, or it silently never persists.
    /// The "name" attribute and the Rectangle/Padding element names must stay as they are, so that
    /// Settings.Upgrade() keeps reading files written by previous versions. Everything else lives in an
    /// optional Settings element, whose absence simply leaves a preset with no window settings.
    /// </remarks>
	public class StoredRegionArray : List<StoredRegion>, IXmlSerializable {

        #region IXmlSerializable Members

		public System.Xml.Schema.XmlSchema GetSchema() {
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader) {
			this.Clear();

            var doc = XDocument.Load(reader);
            foreach (var xmlRegion in doc.Descendants("StoredRegion")) {
                System.Diagnostics.Debug.WriteLine(string.Format("Found region '{0}'.", xmlRegion.Attribute("name")));

                StoredRegion parsedRegion = ParseStoredRegion(xmlRegion);
                if (parsedRegion != null) {
                    this.Add(parsedRegion);
                }
            }
		}

        private StoredRegion ParseStoredRegion(XElement xmlRegion) {
            var xName = xmlRegion.Attribute("name");
            if (xName == null || string.IsNullOrWhiteSpace(xName.Value)) {
                System.Diagnostics.Debug.Fail("Parsed stored region has no name attribute.");
                return null;
            }

            //A null region is legal: it means the whole source window is cloned.
            ThumbnailRegion region = ParseRegion(xmlRegion);

            var stored = new StoredRegion(region, xName.Value);
            ParseWindowSettings(xmlRegion.Element("Settings"), stored);

            return stored;
        }

        private ThumbnailRegion ParseRegion(XElement xmlRegion) {
            var xRectangle = xmlRegion.Element("Rectangle");
            if (xRectangle != null) {
                System.Drawing.Rectangle rectangle = ParseRectangle(xRectangle);
                return new ThumbnailRegion(rectangle);
            }

            var xPadding = xmlRegion.Element("Padding");
            if (xPadding != null) {
                System.Windows.Forms.Padding padding = ParsePadding(xPadding);
                return new ThumbnailRegion(padding);
            }

            return null;
        }

        private System.Windows.Forms.Padding ParsePadding(XElement xPadding) {
            var p = new System.Windows.Forms.Padding();
            try {
                p.Left = Int32.Parse(xPadding.Element("Left").Value);
                p.Top = Int32.Parse(xPadding.Element("Top").Value);
                p.Right = Int32.Parse(xPadding.Element("Right").Value);
                p.Bottom = Int32.Parse(xPadding.Element("Bottom").Value);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail("Failure while parsing padding data.", ex.ToString());
            }
            return p;
        }

        private System.Drawing.Rectangle ParseRectangle(XElement xRectangle) {
            var r = new System.Drawing.Rectangle();
            try {
                r.X = Int32.Parse(xRectangle.Element("X").Value);
                r.Y = Int32.Parse(xRectangle.Element("Y").Value);
                r.Width = Int32.Parse(xRectangle.Element("Width").Value);
                r.Height = Int32.Parse(xRectangle.Element("Height").Value);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail("Failure while parsing rectangle data.", ex.ToString());
            }
            return r;
        }

        #region Window settings parsing

        private void ParseWindowSettings(XElement xSettings, StoredRegion target) {
            if (xSettings == null)
                return;

            try {
                var xLocation = xSettings.Element("Location");
                if (xLocation != null) {
                    target.WindowLocation = new System.Drawing.Point(
                        ParseInt(xLocation.Element("X")),
                        ParseInt(xLocation.Element("Y"))
                    );
                }

                var xSize = xSettings.Element("ClientSize");
                if (xSize != null) {
                    target.WindowClientSize = new System.Drawing.Size(
                        ParseInt(xSize.Element("Width")),
                        ParseInt(xSize.Element("Height"))
                    );
                }

                var xOpacity = xSettings.Element("Opacity");
                if (xOpacity != null) {
                    target.Opacity = (byte)Math.Max(0, Math.Min(255, ParseInt(xOpacity)));
                }

                target.ClickThrough = ParseNullableBool(xSettings.Element("ClickThrough"));
                target.ChromeVisible = ParseNullableBool(xSettings.Element("Chrome"));

                var xSource = xSettings.Element("SourceWindow");
                if (xSource != null) {
                    var xTitle = xSource.Attribute("title");
                    var xClass = xSource.Attribute("class");
                    target.SourceWindowTitle = (xTitle != null) ? xTitle.Value : null;
                    target.SourceWindowClass = (xClass != null) ? xClass.Value : null;
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail("Failure while parsing stored region window settings.", ex.ToString());
            }
        }

        private int ParseInt(XElement element) {
            if (element == null)
                return 0;

            int value;
            if (!Int32.TryParse(element.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return 0;

            return value;
        }

        private bool? ParseNullableBool(XElement element) {
            if (element == null)
                return null;

            bool value;
            if (!Boolean.TryParse(element.Value, out value))
                return null;

            return value;
        }

        #endregion

		public void WriteXml(System.Xml.XmlWriter writer) {
            foreach (var region in this) {
                WriteRegion(writer, region);
            }
		}

        private void WriteRegion(XmlWriter writer, StoredRegion region) {
            writer.WriteStartElement("StoredRegion");
            writer.WriteAttributeString("name", region.Name);

            //A preset without a region clones the whole window. The marker keeps the element non-empty
            //and makes the intent obvious to anyone reading the file.
            if (region.Region == null) {
                writer.WriteElementString("Whole", string.Empty);
            }
            else if (region.Region.Relative) {
                WriteRelativeRegion(writer, region);
            }
            else {
                WriteAbsoluteRegion(writer, region);
            }

            if (region.HasWindowSettings) {
                WriteWindowSettings(writer, region);
            }

            writer.WriteEndElement();
        }

        private void WriteAbsoluteRegion(XmlWriter writer, StoredRegion region) {
            writer.WriteStartElement("Rectangle");

            var bounds = region.Region.Bounds;
            writer.WriteElementString("X", bounds.X.ToString());
            writer.WriteElementString("Y", bounds.Y.ToString());
            writer.WriteElementString("Width", bounds.Width.ToString());
            writer.WriteElementString("Height", bounds.Height.ToString());

            writer.WriteEndElement();
        }

        private void WriteRelativeRegion(XmlWriter writer, StoredRegion region) {
            writer.WriteStartElement("Padding");

            var padding = region.Region.BoundsAsPadding;
            writer.WriteElementString("Left", padding.Left.ToString());
            writer.WriteElementString("Top", padding.Top.ToString());
            writer.WriteElementString("Right", padding.Right.ToString());
            writer.WriteElementString("Bottom", padding.Bottom.ToString());

            writer.WriteEndElement();
        }

        private void WriteWindowSettings(XmlWriter writer, StoredRegion region) {
            writer.WriteStartElement("Settings");

            if (region.WindowLocation.HasValue) {
                writer.WriteStartElement("Location");
                WriteInt(writer, "X", region.WindowLocation.Value.X);
                WriteInt(writer, "Y", region.WindowLocation.Value.Y);
                writer.WriteEndElement();
            }

            if (region.WindowClientSize.HasValue) {
                writer.WriteStartElement("ClientSize");
                WriteInt(writer, "Width", region.WindowClientSize.Value.Width);
                WriteInt(writer, "Height", region.WindowClientSize.Value.Height);
                writer.WriteEndElement();
            }

            if (region.Opacity.HasValue) {
                WriteInt(writer, "Opacity", region.Opacity.Value);
            }

            if (region.ClickThrough.HasValue) {
                WriteBool(writer, "ClickThrough", region.ClickThrough.Value);
            }

            if (region.ChromeVisible.HasValue) {
                WriteBool(writer, "Chrome", region.ChromeVisible.Value);
            }

            if (region.HasSourceWindow) {
                writer.WriteStartElement("SourceWindow");
                writer.WriteAttributeString("class", region.SourceWindowClass ?? string.Empty);
                writer.WriteAttributeString("title", region.SourceWindowTitle ?? string.Empty);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteInt(XmlWriter writer, string name, int value) {
            writer.WriteElementString(name, value.ToString(CultureInfo.InvariantCulture));
        }

        private void WriteBool(XmlWriter writer, string name, bool value) {
            writer.WriteElementString(name, value ? "true" : "false");
        }

		#endregion

	}

}
