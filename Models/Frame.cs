using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
/*
Since MaterialSkin is a pain in the butt when manually setting colors
I have created a simple Panel called Frame that is the same as Panel
but paints itself so MaterialSkin won't hijack it and change the BackColor

P.S: Use it just like Panel's except the coloring var is different read that bellow
*/
namespace TrackFlow.Models;

public class Frame : Panel
{
    private bool _hovering;
    private bool _pressed;
    private bool _hoverCommitted;

    private readonly System.Windows.Forms.Timer _hoverTimer;

    private int _hoverDelayMs = 1000; // default: 1 second

    private Color _normalColor = SystemColors.Control;
    private Color _hoverColor = ControlPaint.Light(SystemColors.Control);
    private Color _pressedColor = ControlPaint.Dark(SystemColors.Control);

    public Frame()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        Size = new Size(200, 100);

        _hoverTimer = new System.Windows.Forms.Timer();
        _hoverTimer.Interval = _hoverDelayMs;
        _hoverTimer.Tick += HoverTimer_Tick;
    }

    // ---- internal hover delay logic ----
    private void HoverTimer_Tick(object? sender, EventArgs e)
    {
        _hoverTimer.Stop();

        if (_hovering && !_pressed)
        {
            _hoverCommitted = true;
            Invalidate();
        }
    }

    // ---- colors ----
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Color NormalColor
    {
        get => _normalColor;
        set
        {
            if (_normalColor == value) return;
            _normalColor = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Color HoverColor
    {
        get => _hoverColor;
        set
        {
            if (_hoverColor == value) return;
            _hoverColor = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Color PressedColor
    {
        get => _pressedColor;
        set
        {
            if (_pressedColor == value) return;
            _pressedColor = value;
            Invalidate();
        }
    }

    // Optional: allow changing delay in code
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public int HoverDelayMs
    {
        get => _hoverDelayMs;
        set
        {
            if (value < 0) value = 0;
            _hoverDelayMs = value;
            _hoverTimer.Interval = value;
        }
    }

    private Color CurrentPaintColor
    {
        get
        {
            if (_pressed) return _pressedColor;
            if (_hoverCommitted) return _hoverColor;
            return _normalColor;
        }
    }

    // ---- mouse handling ----
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);

        _hovering = true;
        _hoverCommitted = false;

        _hoverTimer.Stop();
        _hoverTimer.Start();
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
            _hoverTimer.Stop(); // pressed overrides hover delay
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

    protected override void OnPaint(PaintEventArgs e)
    {
        using var b = new SolidBrush(CurrentPaintColor);
        e.Graphics.FillRectangle(b, ClientRectangle);

        base.OnPaint(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hoverTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}