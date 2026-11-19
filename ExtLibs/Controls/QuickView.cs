using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SkiaSharp.Views.Desktop;

namespace MissionPlanner.Controls
{
    public partial class QuickView : SkiaSharp.Views.Desktop.SKControl
    {
        [System.ComponentModel.Browsable(true)]
        public string desc
        {
            get { return _desc; } set { if (_desc == value) return; _desc = value; Invalidate(); }
        }

        double _number = -9999;

        [System.ComponentModel.Browsable(true)]
        public double number
        {
            get { return _number; }
            set
            {
                lock (this)
                {
                    if (_number.Equals(value))
                        return;
                    _number = value;
                    Invalidate();
                }
            }
        }

        string _numberformat = "0.00";
        private string _desc = "";
        private Color _numbercolor;

        [System.ComponentModel.Browsable(true)]
        public string numberformat
        {
            get
            {
                return _numberformat;
            }
            set
            {
                if (_numberformat.Equals(value))
                    return;
                _numberformat = value;
                this.Invalidate();
            }
        }

        [System.ComponentModel.Browsable(true)]
        public Color numberColor { get { return _numbercolor; } set { if (_numbercolor == value) return; _numbercolor = value; Invalidate(); } }

        //We use this property as a backup store for the numberColor, so it is possible to change numberColor temporary.
        public Color numberColorBackup { get; set; }

        public QuickView()
        {
            InitializeComponent();

            PaintSurface+= OnPaintSurface;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e2)
        {
            var e = new SkiaGraphics(e2.Surface);
            e2.Surface.Canvas.Clear();
            int y = 0;
            {
                // Korugan: tracked-uppercase sand label
                var descfont = KoruganStyle.Enabled ? (KoruganStyle.Semi(this.Font) ?? this.Font) : this.Font;
                var desctext = KoruganStyle.Enabled ? (desc ?? "").ToUpperInvariant() : desc;
                var desccolor = KoruganStyle.Enabled ? KoruganStyle.Sand : this.ForeColor;

                Size extent = e.MeasureString(desctext, descfont).ToSize();

                var mid = extent.Width / 2;

                e.DrawString(desctext, descfont, new SolidBrush(desccolor), this.Width / 2 - mid, 5);

                y = extent.Height;
            }
            //
            {
                var numb = number.ToString(numberformat);

                // Korugan: numbers in IBM Plex Mono
                var numfamily = (KoruganStyle.Enabled && KoruganStyle.NumFamily != null) ? KoruganStyle.NumFamily : this.Font.FontFamily;
                var numstyle = (KoruganStyle.Enabled && KoruganStyle.NumFamily != null) ? FontStyle.Regular : this.Font.Style;

                Size extent = e.MeasureString("0".PadLeft(numb.Length+1,'0'), new Font(numfamily, (float)newSize, numstyle)).ToSize();

                float hRatio = (this.Height - y) / (float)(extent.Height);
                float wRatio = this.Width / (float)extent.Width;
                float ratio = (hRatio < wRatio) ? hRatio : wRatio;

                newSize = (newSize * ratio);// * 0.75f; // pixel to points

                newSize -= newSize % 5;

                if (newSize < 8 || newSize > 999999)
                    newSize = 8;

                extent = e.MeasureString(numb, new Font(numfamily, (float)newSize, numstyle)).ToSize();

                e.DrawString(numb, new Font(numfamily, (float)newSize, numstyle), new SolidBrush(this.numberColor), this.Width / 2 - extent.Width / 2, y + ((this.Height - y) / 2 - extent.Height / 2));
            }
        }

        public override void Refresh()
        {
            if (this.Visible)
                base.Refresh();
        }

        protected override void WndProc(ref Message m) // seems to crash here on linux... so try ignore it
        {
            try
            {
                base.WndProc(ref m);
            }
            catch { }
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            if (this.Visible && this.ThisReallyVisible())
                base.OnInvalidated(e);
        }

        float newSize = 8;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }
}
