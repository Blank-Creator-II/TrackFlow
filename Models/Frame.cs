using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
/*
Since MaterialSkin is a pain in the butt when manually setting colors
I have created a simple Panel called Frame that is the same as Panel
but paints itself so MaterialSkin won't hijack it and change the BackColor

P.S: Use it just like Panel's except the coloring var is different read that bellow

var card = new Frame
{
    Title = "Rent",
    Subtitle = "Monthly rent for apartment.\nPaid via bank transfer.",
    Icon = IconLibrary.GetBitmap(AppIcon.Expense,40,MainForm.PrimaryLight),
    TitleFontSize = 14f,
    SubtitleFontSize = 11f,
    IconSize = new Size(40, 40), // user-specified size
    AllowIconUpscale = false,    // won't upscale small images (keeps them crisp)
    Size = new Size(380, 120),
    Location = new Point(20, 150)
    Margin = new Padding(0, 0, 0, 10),
    NormalColor = MainForm.PrimaryMid,
    HoverColor = MainForm.PrimaryGrey,
    PressedColor = MainForm.PrimaryAsh,
    HoverDelayMs = 100,
    Cursor = Cursors.Hand
};
this.Controls.Add(card);
*/

namespace TrackFlow.Models;
public class Frame : Panel
{
    // hover/pressed state
    private bool _hovering;
    private bool _pressed;
    private bool _hoverCommitted;

    private readonly System.Windows.Forms.Timer _hoverTimer;
    private int _hoverDelayMs = 100; // default short delay

    // background colors
    private Color _normalColor = SystemColors.Control;
    private Color _hoverColor = ControlPaint.Light(SystemColors.Control);
    private Color _pressedColor = ControlPaint.Dark(SystemColors.Control);

    // content
    private string _title = string.Empty;
    private string _subtitle = string.Empty;
    private Image? _icon;
    private Size _iconSize = new Size(28, 28);
    private int _iconMargin = 12;
    private bool _userSetIconSize = false;
    private bool _allowIconUpscale = false;
    private Padding _contentPadding = new Padding(12);

    // fonts (the user can set full Font objects or just sizes)
    private Font? _titleFont; // owned by this control (disposed)
    private Font? _subtitleFont; // owned by this control (disposed)
    private string _titleFontFamily = SystemFonts.DefaultFont.FontFamily.Name;
    private float _titleFontSize = 11f;
    private FontStyle _titleFontStyle = FontStyle.Bold;
    private string _subtitleFontFamily = SystemFonts.DefaultFont.FontFamily.Name;
    private float _subtitleFontSize = 9f;
    private FontStyle _subtitleFontStyle = FontStyle.Regular;
    private Color _titleColor = ControlPaint.Light(SystemColors.Control);
    private Color _subtitleColor = ControlPaint.Light(SystemColors.Control);
    private StringAlignment _htitleAlignment = StringAlignment.Near;
    private StringAlignment _hsubtitleAlignment = StringAlignment.Near;
    private StringAlignment _vtitleAlignment = StringAlignment.Near;
    private StringAlignment _vsubtitleAlignment = StringAlignment.Near;

    // a date holder only used for calendars
    private DateTime _date;

    public Frame()
    {
        // Painting styles
        SetStyle(ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor, true);

        // sensible defaults
        Size = new Size(200, 100);

        _hoverTimer = new System.Windows.Forms.Timer();
        _hoverTimer.Interval = _hoverDelayMs;
        _hoverTimer.Tick += HoverTimer_Tick;

        // create the initial fonts
        RecreateFonts();

        // default colors tuned for dark material look (override if needed)
        _normalColor = Color.FromArgb(255, 45, 45, 45);
        _hoverColor = Color.FromArgb(255, 65, 65, 65);
        _pressedColor = Color.FromArgb(255, 30, 30, 30);
    }

    // ---- hover timer tick ----
    private void HoverTimer_Tick(object? sender, EventArgs e)
    {
        _hoverTimer.Stop();
        if (_hovering && !_pressed)
        {
            _hoverCommitted = true;
            Invalidate();
        }
    }

    // ---- date properties ----
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime DateHolder
    {
        get => _date;
        set { if (_date == value) return; _date = value; Invalidate(); }
    }
    // ---- color properties ----
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color NormalColor
    {
        get => _normalColor;
        set { if (_normalColor == value) return; _normalColor = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color HoverColor
    {
        get => _hoverColor;
        set { if (_hoverColor == value) return; _hoverColor = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color PressedColor
    {
        get => _pressedColor;
        set { if (_pressedColor == value) return; _pressedColor = value; Invalidate(); }
    }

    // ---- hover delay ----
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int HoverDelayMs
    {
        get => _hoverDelayMs;
        set
        {
            _hoverDelayMs = Math.Max(0, value);
            _hoverTimer.Interval = _hoverDelayMs;
        }
    }

    // ---- textual/icon content ----
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        get => _title;
        set { _title = value ?? string.Empty; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Subtitle
    {
        get => _subtitle;
        set { _subtitle = value ?? string.Empty; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StringAlignment HTitleAlignment
    {
        get => _htitleAlignment;
        set { _htitleAlignment = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StringAlignment HSubtitleAlignment
    {
        get => _hsubtitleAlignment;
        set { _hsubtitleAlignment = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StringAlignment VTitleAlignment
    {
        get => _vtitleAlignment;
        set { _vtitleAlignment = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StringAlignment VSubtitleAlignment
    {
        get => _vsubtitleAlignment;
        set { _vsubtitleAlignment = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            // if user did not explicitly set IconSize, adopt natural image size (clamped)
            if (!_userSetIconSize && _icon != null)
            {
                // clamp to reasonable default max to avoid enormous icons
                int maxDim = Math.Max(24, Math.Min(64, Math.Min(_icon.Width, _icon.Height)));
                _iconSize = new Size(Math.Min(_icon.Width, maxDim), Math.Min(_icon.Height, maxDim));
            }
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Size IconSize
    {
        get => _iconSize;
        set
        {
            _userSetIconSize = true;
            _iconSize = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int IconMargin
    {
        get => _iconMargin;
        set { _iconMargin = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowIconUpscale
    {
        get => _allowIconUpscale;
        set { _allowIconUpscale = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Padding ContentPadding
    {
        get => _contentPadding;
        set { _contentPadding = value; Invalidate(); }
    }

    // ---- font size / family properties (convenience) ----
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float TitleFontSize
    {
        get => _titleFontSize;
        set { _titleFontSize = value > 0 ? value : 11f; RecreateFonts(); Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TitleFontFamily
    {
        get => _titleFontFamily;
        set { _titleFontFamily = string.IsNullOrWhiteSpace(value) ? SystemFonts.DefaultFont.FontFamily.Name : value; RecreateFonts(); Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FontStyle TitleFontStyle
    {
        get => _titleFontStyle;
        set { _titleFontStyle = value; RecreateFonts(); Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float SubtitleFontSize
    {
        get => _subtitleFontSize;
        set { _subtitleFontSize = value > 0 ? value : 9f; RecreateFonts(); Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SubtitleFontFamily
    {
        get => _subtitleFontFamily;
        set { _subtitleFontFamily = string.IsNullOrWhiteSpace(value) ? SystemFonts.DefaultFont.FontFamily.Name : value; RecreateFonts(); Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FontStyle SubtitleFontStyle
    {
        get => _subtitleFontStyle;
        set { _subtitleFontStyle = value; RecreateFonts(); Invalidate(); }
    }

    // Allow user to set full Font objects instead of size/family properties.
    // If set, these override the TitleFontSize/Family until cleared.
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Font? TitleFont
    {
        get => _titleFont;
        set
        {
            // if user provides a font object, dispose old ones and use this
            _titleFont?.Dispose();
            _titleFont = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Font? SubtitleFont
    {
        get => _subtitleFont;
        set
        {
            _subtitleFont?.Dispose();
            _subtitleFont = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color TitleColor
    {
        get => _titleColor;
        set { _titleColor = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color SubtitleColor
    {
        get => _subtitleColor;
        set { _subtitleColor = value; Invalidate(); }
    }

    // convenience for painting
    private Color CurrentPaintColor
    {
        get
        {
            if (_pressed) return _pressedColor;
            if (_hoverCommitted) return _hoverColor;
            return _normalColor;
        }
    }

    // recreate fonts based on family/size/style properties if the user did not set explicit Font objects
    private void RecreateFonts()
    {
        // dispose existing only if they were created by us (we assume safe to dispose always here)
        _titleFont?.Dispose();
        _subtitleFont?.Dispose();

        try
        {
            _titleFont = new Font(_titleFontFamily, _titleFontSize, _titleFontStyle, GraphicsUnit.Point);
        }
        catch
        {
            _titleFont = new Font(SystemFonts.DefaultFont.FontFamily, _titleFontSize, _titleFontStyle);
        }

        try
        {
            _subtitleFont = new Font(_subtitleFontFamily, _subtitleFontSize, _subtitleFontStyle, GraphicsUnit.Point);
        }
        catch
        {
            _subtitleFont = new Font(SystemFonts.DefaultFont.FontFamily, _subtitleFontSize, _subtitleFontStyle);
        }
    }

    // ---- mouse handling ----
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovering = true;
        _hoverCommitted = false;
        _hoverTimer.Stop();
        if (_hoverDelayMs == 0)
        {
            _hoverCommitted = true;
            Invalidate();
        }
        else _hoverTimer.Start();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovering = false;
        _pressed = false;
        _hoverCommitted = false;
        _hoverTimer.Stop();
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            _hoverTimer.Stop();
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_pressed)
        {
            _pressed = false;
            Invalidate();
            OnClick(EventArgs.Empty);
        }
    }

    // ---- painting ----
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // intentionally do nothing (we fill entire background in OnPaint)
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // best text rendering
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        // paint background
        using (var b = new SolidBrush(CurrentPaintColor))
        {
            g.FillRectangle(b, ClientRectangle);
        }

        // compute content rect
        var contentRect = new Rectangle(
            ContentPadding.Left,
            ContentPadding.Top,
            Math.Max(0, ClientSize.Width - ContentPadding.Horizontal),
            Math.Max(0, ClientSize.Height - ContentPadding.Vertical)
        );

        // layout: icon on left, then text column
        int iconAreaWidth = 0;
        if (_icon != null)
        {
            iconAreaWidth = _iconSize.Width + _iconMargin; // small gap after icon
        }

        var textArea = new Rectangle(
            contentRect.Left + iconAreaWidth,
            contentRect.Top,
            Math.Max(0, contentRect.Width - iconAreaWidth),
            contentRect.Height
        );

        // draw icon (center vertically) - avoid upscaling by default
        if (_icon != null)
        {
            // desired destination size
            var desiredW = _iconSize.Width;
            var desiredH = _iconSize.Height;

            // natural image size
            var srcW = _icon.Width;
            var srcH = _icon.Height;

            // determine final draw size while preserving aspect ratio
            float srcAspect = (float)srcW / srcH;
            int drawW = desiredW;
            int drawH = desiredH;

            // preserve aspect by fitting desired box
            if (desiredW > 0 && desiredH > 0)
            {
                if (srcAspect >= 1f)
                {
                    // wide image
                    drawW = desiredW;
                    drawH = Math.Max(1, (int)(desiredW / srcAspect));
                }
                else
                {
                    // tall image
                    drawH = desiredH;
                    drawW = Math.Max(1, (int)(desiredH * srcAspect));
                }
            }

            // if upscaling not allowed, clamp to natural image size
            if (!_allowIconUpscale)
            {
                if (drawW > srcW)
                {
                    float scale = (float)srcW / drawW;
                    drawW = srcW;
                    drawH = Math.Max(1, (int)(drawH * scale));
                }
                if (drawH > srcH)
                {
                    float scale = (float)srcH / drawH;
                    drawH = srcH;
                    drawW = Math.Max(1, (int)(drawW * scale));
                }
            }

            var iconRect = new Rectangle(
                contentRect.Left + Math.Max(0, (_iconSize.Width - drawW) / 2),
                contentRect.Top + Math.Max(0, (contentRect.Height - drawH) / 2),
                drawW,
                drawH
            );

            // draw image with high-quality settings
            try
            {
                g.DrawImage(_icon, iconRect);
            }
            catch
            {
                // ignore draw errors
            }
        }

        // pick brushes for text
        using var titleBrush = new SolidBrush(_titleColor);
        using var subtitleBrush = new SolidBrush(_subtitleColor);

        // draw title (single line, ellipsize)
        if (!string.IsNullOrEmpty(_title) && _titleFont != null)
        {
            var titleFormat = new StringFormat(StringFormatFlags.NoWrap)
            {
                Trimming = StringTrimming.EllipsisCharacter,
                Alignment = _htitleAlignment,
                LineAlignment = _vtitleAlignment
            };

            var titleSizeF = g.MeasureString(_title, _titleFont, new SizeF(textArea.Width, float.MaxValue), titleFormat);
            int titleHeight = Math.Max(1, (int)Math.Ceiling(titleSizeF.Height));

            var titleRect = new Rectangle(textArea.Left, textArea.Top, textArea.Width, titleHeight);
            g.DrawString(_title, _titleFont, titleBrush, titleRect, titleFormat);

            // draw subtitle wrapped into remaining space
            if (!string.IsNullOrWhiteSpace(_subtitle) && _subtitleFont != null)
            {
                var subtitleTop = titleRect.Bottom;
                var subtitleHeight = Math.Max(0, textArea.Bottom - subtitleTop);
                if (subtitleHeight > 0)
                {
                    var subtitleRect = new Rectangle(textArea.Left, subtitleTop, textArea.Width, subtitleHeight);
                    using var subtitleFormat = new StringFormat
                    {
                        Trimming = StringTrimming.EllipsisCharacter,
                        Alignment = _hsubtitleAlignment,
                        LineAlignment = _vsubtitleAlignment
                    };
                    g.DrawString(_subtitle, _subtitleFont, subtitleBrush, subtitleRect, subtitleFormat);
                }
            }
        }
        else
        {
            // no title: just render subtitle (wrapped)
            if (!string.IsNullOrWhiteSpace(_subtitle) && _subtitleFont != null)
            {
                var subtitleRect = new Rectangle(textArea.Left, textArea.Top, textArea.Width, textArea.Height);
                using var subtitleFormat = new StringFormat
                {
                    Trimming = StringTrimming.EllipsisCharacter,
                    Alignment = _hsubtitleAlignment,
                    LineAlignment = _vsubtitleAlignment
                };
                g.DrawString(_subtitle, _subtitleFont, subtitleBrush, subtitleRect, subtitleFormat);
            }
        }

        base.OnPaint(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hoverTimer.Dispose();
            _titleFont?.Dispose();
            _subtitleFont?.Dispose();
        }
        base.Dispose(disposing);
    }
}