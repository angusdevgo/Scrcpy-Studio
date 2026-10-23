using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScrcpyStudio
{
    #region Satelite 现代化薄荷玉石绿设计系统 (Satelite Mint & Porcelain Glass)

    public static class UITheme
    {
        // 核心背景层次 (100% 还原 Satelite 参考图)
        public static readonly Color WindowBg       = Color.FromArgb(227, 239, 233);   // #E3EFE9 柔和浅薄荷底色
        public static readonly Color HeaderPillBg   = Color.FromArgb(210, 228, 220);   // #D2E4DC 顶部大胶囊底
        public static readonly Color CardBg         = Color.FromArgb(248, 252, 250);   // #F8FCFA 瓷白微透悬浮卡片
        public static readonly Color CardHoverBg    = Color.FromArgb(255, 255, 255);   // #FFFFFF 纯白高光卡片
        public static readonly Color CardBorder     = Color.FromArgb(208, 226, 218);   // #D0E2DA 超轻边框
        public static readonly Color InnerBorder    = Color.FromArgb(220, 234, 227);   // #DCEAE3 内分割线

        // 按钮与交互胶囊
        public static readonly Color PillActiveBg   = Color.FromArgb(255, 255, 255);   // #FFFFFF 活跃标签纯白胶囊
        public static readonly Color PillInactiveBg = Color.Transparent;               // 未选透明
        public static readonly Color AccentGreen    = Color.FromArgb(34, 163, 107);    // #22A36B Satelite 标志性翠绿主按键
        public static readonly Color AccentHover    = Color.FromArgb(40, 185, 122);    // #28B97A 浅翠绿悬停
        public static readonly Color AccentDown     = Color.FromArgb(28, 140, 92);     // #1C8C5C 按下态
        public static readonly Color SecondaryBtnBg = Color.FromArgb(215, 232, 224);   // #D7E8E0 次级药丸底
        public static readonly Color SecondaryHover = Color.FromArgb(205, 225, 215);   // 次级悬停

        // 状态与警示
        public static readonly Color DangerRed      = Color.FromArgb(239, 68, 68);     // #EF4444 停止红
        public static readonly Color DangerHover    = Color.FromArgb(248, 113, 113);
        public static readonly Color StatusDotGreen = Color.FromArgb(34, 163, 107);
        public static readonly Color StatusDotGray  = Color.FromArgb(156, 175, 166);

        // 文字排版层级
        public static readonly Color TextPrimary    = Color.FromArgb(26, 44, 36);      // #1A2C24 极深墨青灰（清晰温润）
        public static readonly Color TextSecondary  = Color.FromArgb(78, 102, 91);     // #4E665B 次级说明墨绿灰
        public static readonly Color TextMuted      = Color.FromArgb(122, 148, 136);   // #7A9488 辅助浅灰绿
        public static readonly Color TextDisabled   = Color.FromArgb(170, 192, 182);   // 禁用态

        // 字体层次（高清大字号，告别微缩模糊）
        public static Font FontBrand;
        public static Font FontHeroTitle;
        public static Font FontPageTitle;
        public static Font FontPageSub;
        public static Font FontCardHeader;
        public static Font FontItemTitle;
        public static Font FontItemSub;
        public static Font FontBody;
        public static Font FontBodyBold;
        public static Font FontMono;
        public static Font FontSmall;

        public static float Scale = 1.0f;

        public static void InitDpi(float scale)
        {
            Scale = scale;
            // 采用标准物理点字号，配合高 DPI 自动高清抗锯齿，绝不重复二次放大导致排版巨型膨胀
            FontBrand      = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            FontHeroTitle  = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
            FontPageTitle  = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
            FontPageSub    = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular);
            FontCardHeader = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            FontItemTitle  = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            FontItemSub    = new Font("Microsoft YaHei UI", 8F, FontStyle.Regular);
            FontBody       = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
            FontBodyBold   = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            FontMono       = new Font("Consolas", 9F, FontStyle.Regular);
            FontSmall      = new Font("Microsoft YaHei UI", 8F, FontStyle.Regular);
        }

        public static int Dpi(int val)
        {
            return (int)Math.Round(val * Scale);
        }
    }

    #endregion

    #region 几何绘制与圆角阴影工具 Geometry

    public static class Geometry
    {
        public static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            if (radius < 1) radius = 1;
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static GraphicsPath Pill(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();
            int d = rect.Height;
            if (d < 1) d = 1;
            if (d > rect.Width) d = rect.Width;
            path.AddArc(rect.X, rect.Y, d, d, 90, 180);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 180);
            path.CloseFigure();
            return path;
        }
    }

    #endregion

    #region Satelite 原生高清控件库

    // 1. MintCard (柔和悬浮大圆角白瓷卡片)
    public class MintCard : Panel
    {
        public int Radius { get; set; }
        public Color BorderColor { get; set; }

        public MintCard()
        {
            Radius = 14;
            BorderColor = UITheme.CardBorder;
            BackColor = UITheme.CardBg;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath path = Geometry.RoundedRect(rect, UITheme.Dpi(Radius)))
                using (SolidBrush brush = new SolidBrush(BackColor))
                using (Pen pen = new Pen(BorderColor, 1.2f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }
            }
            catch { }
        }
    }

    // 2. MintButton (Satelite 绿色大胶囊药丸按钮)
    public class MintButton : Control
    {
        public enum ButtonStyle { Accent, Secondary, Danger, Ghost, CloseGhost }

        private bool _isHovered;
        private bool _isPressed;

        public ButtonStyle Style { get; set; }
        public int Radius { get; set; }

        public MintButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = UITheme.FontBodyBold;
            ForeColor = Color.White;
            Radius = 16;
            Style = ButtonStyle.Secondary;
            Size = new Size(UITheme.Dpi(110), UITheme.Dpi(36));
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _isPressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _isPressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Color bg = UITheme.SecondaryBtnBg;
                Color fg = UITheme.TextPrimary;
                Color border = Color.Transparent;

                if (!Enabled)
                {
                    bg = Color.FromArgb(220, 232, 226);
                    fg = UITheme.TextDisabled;
                }
                else
                {
                    switch (Style)
                    {
                        case ButtonStyle.Accent:
                            bg = _isPressed ? UITheme.AccentDown : (_isHovered ? UITheme.AccentHover : UITheme.AccentGreen);
                            fg = Color.White;
                            break;
                        case ButtonStyle.Danger:
                            bg = _isPressed ? Color.FromArgb(210, 50, 50) : (_isHovered ? UITheme.DangerHover : UITheme.DangerRed);
                            fg = Color.White;
                            break;
                        case ButtonStyle.Secondary:
                            bg = _isPressed ? Color.FromArgb(195, 218, 208) : (_isHovered ? UITheme.SecondaryHover : UITheme.SecondaryBtnBg);
                            fg = UITheme.TextPrimary;
                            break;
                        case ButtonStyle.Ghost:
                            bg = _isPressed ? Color.FromArgb(210, 226, 218) : (_isHovered ? Color.FromArgb(220, 235, 228) : Color.Transparent);
                            fg = UITheme.TextPrimary;
                            break;
                        case ButtonStyle.CloseGhost:
                            bg = _isPressed ? Color.FromArgb(220, 70, 70) : (_isHovered ? Color.FromArgb(240, 90, 90) : Color.Transparent);
                            fg = _isHovered ? Color.White : UITheme.TextSecondary;
                            break;
                    }
                }

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath path = Geometry.RoundedRect(rect, UITheme.Dpi(10)))
                {
                    using (SolidBrush brush = new SolidBrush(bg))
                        g.FillPath(brush, path);
                    if (border != Color.Transparent)
                    {
                        using (Pen pen = new Pen(border, 1f))
                            g.DrawPath(pen, path);
                    }
                }

                TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            catch { }
        }
    }

    // 3. MintComboBox (白瓷圆角下拉选择框)
    public class MintComboBox : Control
    {
        private List<string> _items = new List<string>();
        private int _selectedIndex = -1;
        private bool _isHovered;
        private bool _isDropped;
        private Form _dropForm;
        private ListBox _dropListBox;

        public event EventHandler SelectedIndexChanged;

        public List<string> Items { get { return _items; } }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value && value >= -1 && value < _items.Count)
                {
                    _selectedIndex = value;
                    Invalidate();
                    if (SelectedIndexChanged != null)
                        SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public string SelectedItem
        {
            get
            {
                if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                    return _items[_selectedIndex];
                return null;
            }
            set
            {
                int idx = _items.IndexOf(value);
                if (idx >= 0) SelectedIndex = idx;
            }
        }

        public MintComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
            Size = new Size(UITheme.Dpi(180), UITheme.Dpi(36));
            Cursor = Cursors.Hand;
            Font = UITheme.FontBody;
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_isDropped) CloseDropdown();
            else ShowDropdown();
        }

        private void ShowDropdown()
        {
            if (_items.Count == 0) return;

            Point screenPt = this.PointToScreen(new Point(0, this.Height + 3));
            int itemHeight = UITheme.Dpi(34);
            int dropHeight = Math.Min(_items.Count * itemHeight + 6, UITheme.Dpi(280));

            _dropForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                TopMost = true,
                BackColor = UITheme.CardBorder
            };

            _dropListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = UITheme.TextPrimary,
                Font = this.Font,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = itemHeight,
                IntegralHeight = false
            };

            foreach (string item in _items)
                _dropListBox.Items.Add(item);

            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                _dropListBox.SelectedIndex = _selectedIndex;

            _dropListBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= _items.Count) return;
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool isSel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color bg = isSel ? Color.FromArgb(232, 245, 238) : Color.White;
                Color fg = isSel ? UITheme.AccentGreen : UITheme.TextPrimary;

                using (SolidBrush b = new SolidBrush(bg))
                    g.FillRectangle(b, e.Bounds);

                if (isSel)
                {
                    using (SolidBrush barB = new SolidBrush(UITheme.AccentGreen))
                        g.FillRectangle(barB, e.Bounds.X + 2, e.Bounds.Y + 6, 3, e.Bounds.Height - 12);
                }

                Rectangle textRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y, e.Bounds.Width - 16, e.Bounds.Height);
                TextRenderer.DrawText(g, _items[e.Index], this.Font, textRect, fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);
            };

            _dropListBox.Click += (s, e) =>
            {
                if (_dropListBox.SelectedIndex >= 0)
                {
                    int chosen = _dropListBox.SelectedIndex;
                    CloseDropdown();
                    this.SelectedIndex = chosen;
                }
                else
                {
                    CloseDropdown();
                }
            };

            _dropForm.Deactivate += (s, e) => this.BeginInvoke(new Action(() => CloseDropdown()));
            _dropForm.Controls.Add(_dropListBox);
            _dropForm.Size = new Size(this.Width, dropHeight);
            _dropForm.Location = screenPt;

            _isDropped = true;
            _dropForm.Show();
            _dropListBox.Focus();
        }

        private void CloseDropdown()
        {
            try
            {
                if (_dropForm != null)
                {
                    Form f = _dropForm;
                    _dropForm = null;
                    _dropListBox = null;
                    if (!f.IsDisposed)
                    {
                        f.Close();
                        f.Dispose();
                    }
                }
            }
            catch { }
            finally
            {
                _isDropped = false;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                Color border = (_isDropped || _isHovered) ? UITheme.AccentGreen : UITheme.CardBorder;
                Color bg = _isHovered ? Color.FromArgb(255, 255, 255) : Color.FromArgb(246, 251, 248);

                using (GraphicsPath path = Geometry.RoundedRect(rect, UITheme.Dpi(8)))
                using (SolidBrush brush = new SolidBrush(bg))
                using (Pen pen = new Pen(border, 1.2f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

                string displayText = (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                                   ? _items[_selectedIndex] : "请选择...";
                Rectangle textRect = new Rectangle(10, 0, Width - 32, Height);
                TextRenderer.DrawText(g, displayText, Font, textRect, UITheme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);

                int arrowX = Width - 20;
                int arrowY = Height / 2 - 2;
                using (Pen arrowPen = new Pen(_isHovered ? UITheme.AccentGreen : UITheme.TextSecondary, 1.8f))
                {
                    arrowPen.StartCap = LineCap.Round;
                    arrowPen.EndCap = LineCap.Round;
                    if (_isDropped)
                    {
                        g.DrawLine(arrowPen, arrowX, arrowY + 3, arrowX + 4, arrowY);
                        g.DrawLine(arrowPen, arrowX + 4, arrowY, arrowX + 8, arrowY + 3);
                    }
                    else
                    {
                        g.DrawLine(arrowPen, arrowX, arrowY, arrowX + 4, arrowY + 4);
                        g.DrawLine(arrowPen, arrowX + 4, arrowY + 4, arrowX + 8, arrowY);
                    }
                }
            }
            catch { }
        }
    }

    // 4. MintToggle (Satelite 风格圆润滑块开关)
    public class MintToggle : Control
    {
        private bool _checked;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    Invalidate();
                    if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler CheckedChanged;

        public MintToggle()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(UITheme.Dpi(48), UITheme.Dpi(26));
            Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                Color bg = _checked ? UITheme.AccentGreen : Color.FromArgb(210, 228, 220);
                Color border = _checked ? UITheme.AccentHover : Color.FromArgb(190, 214, 204);

                using (GraphicsPath path = Geometry.Pill(rect))
                using (SolidBrush brush = new SolidBrush(bg))
                using (Pen pen = new Pen(border, 1f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

                int thumbSize = Height - 6;
                int thumbX = _checked ? (Width - thumbSize - 3) : 3;
                Rectangle thumbRect = new Rectangle(thumbX, 3, thumbSize, thumbSize);
                using (SolidBrush thumbBrush = new SolidBrush(Color.White))
                    g.FillEllipse(thumbBrush, thumbRect);
            }
            catch { }
        }
    }

    // 5. MintTextBox (白瓷感输入框)
    public class MintTextBox : Panel
    {
        private TextBox _inner;
        private bool _isFocused;

        public new string Text
        {
            get { return _inner.Text; }
            set { _inner.Text = value; }
        }

        public TextBox InnerTextBox { get { return _inner; } }

        public MintTextBox()
        {
            Size = new Size(UITheme.Dpi(160), UITheme.Dpi(36));
            BackColor = Color.White;
            DoubleBuffered = true;

            _inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = UITheme.TextPrimary,
                Font = UITheme.FontMono,
                Location = new Point(10, 7),
                Width = Width - 20
            };

            _inner.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _inner.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            Controls.Add(_inner);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            if (_inner != null)
            {
                _inner.Width = Width - 20;
                _inner.Location = new Point(10, (Height - _inner.PreferredHeight) / 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color border = _isFocused ? UITheme.AccentGreen : UITheme.CardBorder;

            using (GraphicsPath path = Geometry.RoundedRect(rect, UITheme.Dpi(8)))
            using (SolidBrush brush = new SolidBrush(Color.White))
            using (Pen pen = new Pen(border, _isFocused ? 1.5f : 1.2f))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }
    }

    // 6. PillNavTab (100% 还原参考图顶部的纯白滑动胶囊导航标签，支持真正透明通道)
    public class PillNavTab : Control
    {
        private bool _isSelected;
        private bool _isHovered;

        public string TabName { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; Invalidate(); }
        }

        public PillNavTab(string name)
        {
            TabName = name;
            Size = new Size(UITheme.Dpi(104), UITheme.Dpi(36));
            Cursor = Cursors.Hand;
            BackColor = UITheme.HeaderPillBg; // 与父级胶囊背景完美同色，彻底消除方块底色边缘
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                Color fg = UITheme.TextSecondary;

                if (_isSelected)
                {
                    using (GraphicsPath path = Geometry.Pill(rect))
                    using (SolidBrush brush = new SolidBrush(Color.White))
                        g.FillPath(brush, path);
                    fg = UITheme.TextPrimary;
                }
                else if (_isHovered)
                {
                    using (GraphicsPath path = Geometry.Pill(rect))
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                        g.FillPath(brush, path);
                    fg = UITheme.TextPrimary;
                }

                Font f = _isSelected ? UITheme.FontBodyBold : UITheme.FontBody;
                TextRenderer.DrawText(g, TabName, f, ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            catch { }
        }
    }

    // 7. MintChip (画质预设胶囊)
    public class MintChip : Control
    {
        private bool _isSelected;
        private bool _isHovered;

        public bool Selected
        {
            get { return _isSelected; }
            set { _isSelected = value; Invalidate(); }
        }

        public bool ReadOnlyChip { get; set; }

        public MintChip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(UITheme.Dpi(86), UITheme.Dpi(32));
            Cursor = Cursors.Hand;
            Font = UITheme.FontBody;
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                Color bg;
                Color fg;

                if (_isSelected)
                {
                    bg = Color.White;
                    fg = UITheme.AccentGreen;
                }
                else if (_isHovered && !ReadOnlyChip)
                {
                    bg = Color.FromArgb(235, 245, 240);
                    fg = UITheme.TextPrimary;
                }
                else
                {
                    bg = Color.FromArgb(218, 234, 226);
                    fg = UITheme.TextSecondary;
                }

                using (GraphicsPath path = Geometry.Pill(rect))
                using (SolidBrush brush = new SolidBrush(bg))
                    g.FillPath(brush, path);

                TextRenderer.DrawText(g, Text, Font, rect, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            catch { }
        }
    }

    // 8. MintToast (Satelite 悬浮通知药丸)
    public enum ToastKind { Info, Success, Error, Warning }

    public class MintToastHost : Panel
    {
        private class ToastItem
        {
            public string Text;
            public ToastKind Kind;
            public DateTime Born;
            public float Alpha;
        }

        private List<ToastItem> items = new List<ToastItem>();
        private Timer timer;

        public MintToastHost()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Width = UITheme.Dpi(380);
            Height = UITheme.Dpi(40);

            timer = new Timer();
            timer.Interval = 30;
            timer.Tick += OnTick;
        }

        public void Show(string text, ToastKind kind)
        {
            items.Clear();
            ToastItem item = new ToastItem();
            item.Text = text;
            item.Kind = kind;
            item.Born = DateTime.Now;
            item.Alpha = 0;
            items.Add(item);
            Visible = true;
            timer.Start();
            Invalidate();
        }

        private void OnTick(object sender, EventArgs e)
        {
            bool active = false;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                double age = (DateTime.Now - items[i].Born).TotalMilliseconds;
                float alpha;
                if (age < 120) alpha = (float)(age / 120.0);
                else if (age > 2200) alpha = (float)Math.Max(0, 1 - (age - 2200) / 250.0);
                else alpha = 1;

                items[i].Alpha = alpha;
                if (age > 2450) items.RemoveAt(i);
                else active = true;
            }

            Invalidate();
            if (!active)
            {
                timer.Stop();
                Visible = false; // 提示结束后自动隐藏，完全不占屏幕任何区域
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (items.Count == 0 || items[0].Alpha <= 0) return;
                ToastItem item = items[0];
                int alpha = (int)(item.Alpha * 255);
                if (alpha <= 0) return;

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                Color accent = KindColor(item.Kind);
                Color bg = Color.FromArgb((int)(250 * item.Alpha), 255, 255, 255);
                Color border = Color.FromArgb((int)(180 * item.Alpha), UITheme.CardBorder);

                using (GraphicsPath path = Geometry.RoundedRect(rect, UITheme.Dpi(10)))
                using (SolidBrush brush = new SolidBrush(bg))
                using (Pen pen = new Pen(border, 1.2f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

                using (SolidBrush dot = new SolidBrush(Color.FromArgb(alpha, accent)))
                    g.FillEllipse(dot, 18, Height / 2 - 4, 8, 8);

                Rectangle textRect = new Rectangle(36, 0, Width - 50, Height);
                TextRenderer.DrawText(g, item.Text, UITheme.FontBodyBold, textRect,
                    Color.FromArgb(alpha, UITheme.TextPrimary),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            catch { }
        }

        private Color KindColor(ToastKind kind)
        {
            switch (kind)
            {
                case ToastKind.Success: return UITheme.AccentGreen;
                case ToastKind.Error: return UITheme.DangerRed;
                default: return UITheme.AccentGreen;
            }
        }
    }

    #endregion

    #region 主窗体 (自适应流体高清 Satelite 架构)

    public class MainForm : Form
    {
        private int TitleBarH { get { return UITheme.Dpi(44); } }
        private int TopNavH   { get { return UITheme.Dpi(56); } }

        private string scrcpyExePath = "";
        private string adbExePath = "";
        private Process scrcpyProcess;

        private Panel customTitleBar;
        private Label lblTitleLogo;
        private Label lblTitleText;
        private MintButton btnMin;
        private MintButton btnMax;
        private MintButton btnClose;

        private Panel topNavPanel;
        private Panel navCapsule;
        private Label capsuleBadge;
        private Label rightInd;
        private Panel contentPanel;
        private List<PillNavTab> navTabs = new List<PillNavTab>();

        // 4 个分页容器
        private Panel pageDashboard;
        private Panel pageDisplay;
        private Panel pageTools;
        private Panel pageSettings;

        // Dashboard 响应式控件引用
        private MintCard cardHero;
        private Label heroIcon;
        private Panel heroRightGroup;
        private MintCard cardBanner;
        private Label lblBanner;
        private MintButton btnFastGo;

        private MintCard c1;
        private MintCard c2;
        private MintCard c3;


        private Label lblHeroState;
        private Label lblHeroDeviceName;
        private Label lblHeroSub;
        private MintButton btnLaunchPrimary;
        private MintButton btnStopPrimary;
        private MintButton btnMore;

        private Label lblRTitle;
        private Label r1;
        private Label r2;
        private Label r3;
        private MintComboBox comboDevices;
        private MintButton btnRefreshDev;
        private MintTextBox txtWirelessIp;
        private MintButton btnAdbTcpip;
        private MintButton btnWirelessConnect;
        private MintButton btnShot;
        private MintButton btnWake;
        private MintButton btnRestart;

        private MintToggle togScreenOff;
        private MintToggle togAlwaysOnTop;
        private MintToggle togStayAwake;
        private MintToggle togAudio;
        private MintToggle togBorderless;
        private MintToggle togTouches;

        // Display 响应式控件引用
        private MintCard cardPresets;
        private Label lblPTitle;
        private MintCard cardVideo;
        private MintCard cardAudio;
        private MintChip chipPerf;
        private MintChip chipBalanced;
        private MintChip chipQuality;
        private MintChip chipCustom;
        private bool applyingPreset;

        private MintComboBox comboResolution;
        private MintComboBox comboFps;
        private MintComboBox comboBitrate;
        private MintComboBox comboCodec;
        private MintComboBox comboAudioCodec;
        private MintComboBox comboAudioBitrate;
        private MintComboBox comboBuffer;
        private MintComboBox comboRecord;

        // Tools 响应式控件引用
        private MintCard cardKeys;
        private MintCard cardClip;
        private MintTextBox txtClip;
        private MintButton btnSendText;
        private MintButton btnOpenWorkDir;
        private MintButton btnInstallApk;
        private List<MintButton> toolButtons = new List<MintButton>();

        // Settings 响应式控件引用
        private MintCard cardPath;
        private MintCard cardLog;
        private Label lblScrcpyPathVal;
        private Label lblAdbPathVal;
        private Label lblScrcpyStatus;
        private Label lblEngineOrigin;
        private Label lblAdbStatus;
        private MintButton btnBrowseScrcpy;
        private MintButton btnBrowseAdb;
        private MintButton btnAutoDetect;
        private MintButton btnRestartAdbServer;
        private MintButton btnClearLog;
        private TextBox txtLogMonitor;

        // 状态与 Toast
        private MintToastHost toastHost;

        // 悬浮导航栏
        private FloatingNavBar navBar;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int WM_NCHITTEST = 0x0084;

        public MainForm()
        {
            InitWindowArchitecture();
            LoadApplicationIcon();
            DetectScrcpyEngine();
            BuildCustomTitleBar();
            BuildTopNavigationBar();
            BuildContentPages();
            SelectNavTab(0);
            RefreshDeviceList();
        }

        private void InitWindowArchitecture()
        {
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;

            // 恢复设计基准比例（1060 × 680，配合系统 DPI 自动映射高清像素，不强行放大工作区）
            int defaultW = UITheme.Dpi(1060);
            int defaultH = UITheme.Dpi(680);

            ClientSize = new Size(defaultW, defaultH);
            MinimumSize = new Size(UITheme.Dpi(960), UITheme.Dpi(620)); // 支持八向自由缩放调节！
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UITheme.WindowBg;
            Font = UITheme.FontBody;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);
        }

        // 无边框窗口可自由八向拉伸调节大小
        protected override void WndProc(ref Message m)
        {
            int bw = UITheme.Dpi(8);
            if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);
                Point p = PointToClient(new Point(m.LParam.ToInt32()));

                bool left = p.X <= bw;
                bool right = p.X >= ClientSize.Width - bw;
                bool top = p.Y <= bw;
                bool bottom = p.Y >= ClientSize.Height - bw;

                if (left && top) { m.Result = (IntPtr)13; return; }
                if (right && top) { m.Result = (IntPtr)14; return; }
                if (left && bottom) { m.Result = (IntPtr)16; return; }
                if (right && bottom) { m.Result = (IntPtr)17; return; }
                if (left) { m.Result = (IntPtr)10; return; }
                if (right) { m.Result = (IntPtr)11; return; }
                if (top) { m.Result = (IntPtr)12; return; }
                if (bottom) { m.Result = (IntPtr)15; return; }
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutResponsiveChrome();
            LayoutResponsivePages();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(UITheme.CardBorder, 1.2f))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        private void LoadApplicationIcon()
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            if (!File.Exists(iconPath))
                iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\app\data\icon.ico");

            try
            {
                if (File.Exists(iconPath))
                    Icon = new Icon(iconPath);
            }
            catch { }
        }

        #region 单文件自包含核心 (Embedded Core Bundle)

        /// <summary>
        /// 把所有运行时依赖（scrcpy.exe / adb.exe / DLL / scrcpy-server）编译进本 exe，
        /// 首次运行时释放到 %LOCALAPPDATA%\Scrcpy\core，做到"一个 exe 就能跑"。
        /// </summary>
        private static class CoreBundle
        {
            private const string BundleId = "Scrcpy.Core.v5.0.1.r3";
            private const string MarkerFile = ".bundle";

            private static string _root;

            /// <summary>
            /// 引擎释放目录：
            ///   1. 优先 &lt;exe 所在目录&gt;\core —— 安装到哪个盘就全在哪个盘，整个目录可整体拷走
            ///   2. 该目录不可写（如 Program Files 无权限）时，退回 %LOCALAPPDATA%\Scrcpy\core
            /// </summary>
            public static string BundleRoot
            {
                get
                {
                    if (_root != null) return _root;

                    string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    string local = Path.Combine(exeDir, "core");
                    if (CanWrite(exeDir))
                    {
                        _root = local;
                        return _root;
                    }

                    _root = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Scrcpy", "core");
                    return _root;
                }
            }

            private static bool CanWrite(string dir)
            {
                try
                {
                    if (!Directory.Exists(dir)) return false;
                    string probe = Path.Combine(dir, ".write_probe");
                    using (FileStream fs = new FileStream(probe, FileMode.Create, FileAccess.Write, FileShare.None)) { }
                    File.Delete(probe);
                    return true;
                }
                catch { return false; }
            }

            public static bool HasEmbeddedPayload()
            {
                try
                {
                    string[] names = typeof(CoreBundle).Assembly.GetManifestResourceNames();
                    foreach (string n in names)
                    {
                        if (n.StartsWith("Core.", StringComparison.OrdinalIgnoreCase)) return true;
                    }
                }
                catch { }
                return false;
            }

            /// <summary>确保核心已释放到磁盘，返回 scrcpy.exe 的完整路径（失败返回空串）</summary>
            public static string EnsureExtracted()
            {
                try
                {
                    string root = BundleRoot;
                    string marker = Path.Combine(root, MarkerFile);
                    string exe = Path.Combine(root, "scrcpy.exe");

                    if (File.Exists(marker) && File.Exists(exe))
                    {
                        string id = "";
                        try { id = File.ReadAllText(marker).Trim(); } catch { }
                        if (id == BundleId) return exe;
                    }

                    if (!HasEmbeddedPayload()) return "";

                    if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                    System.Reflection.Assembly asm = typeof(CoreBundle).Assembly;
                    string[] names = asm.GetManifestResourceNames();
                    foreach (string name in names)
                    {
                        if (!name.StartsWith("Core.", StringComparison.OrdinalIgnoreCase)) continue;

                        string fileName = name.Substring("Core.".Length);
                        if (fileName.Length == 0) continue;

                        string outPath = Path.Combine(root, fileName);
                        try
                        {
                            using (Stream src = asm.GetManifestResourceStream(name))
                            {
                                if (src == null) continue;
                                using (FileStream dst = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                    src.CopyTo(dst, 81920);
                            }
                        }
                        catch { }
                    }

                    try { File.WriteAllText(marker, BundleId); } catch { }

                    return File.Exists(exe) ? exe : "";
                }
                catch { }
                return "";
            }

            /// <summary>若用户想彻底清理，可调用（保留给设置页使用）</summary>
            public static void Cleanup()
            {
                try
                {
                    string root = BundleRoot;
                    if (Directory.Exists(root)) Directory.Delete(root, true);
                }
                catch { }
            }
        }

        #endregion

        #region 引擎路径探测

        private void DetectScrcpyEngine()
        {
            string cfgFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scrcpy_gui.cfg");
            if (File.Exists(cfgFile))
            {
                try
                {
                    string saved = File.ReadAllText(cfgFile).Trim();
                    if (File.Exists(saved)) { SetCorePath(saved); return; }
                }
                catch { }
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 0. 便携 core 目录优先（外置核心，便于替换/调试）
            string portable = Path.Combine(baseDir, "core", "scrcpy.exe");
            if (File.Exists(portable)) { SetCorePath(Path.GetFullPath(portable)); return; }

            // 1. 单文件自包含：把内嵌核心释放到本机缓存目录
            string bundled = CoreBundle.EnsureExtracted();
            if (!string.IsNullOrEmpty(bundled) && File.Exists(bundled))
            {
                SetCorePath(bundled);
                return;
            }

            List<string> candidates = new List<string>();

            // 2. 同目录 / core 子目录
            candidates.Add(Path.Combine(baseDir, "core", "scrcpy.exe"));
            candidates.Add(Path.Combine(baseDir, "scrcpy.exe"));

            // 2. 逐级向上探测（开发目录 / 成品目录 / 构建输出目录）
            DirectoryInfo dir = new DirectoryInfo(baseDir);
            for (int level = 0; level < 6 && dir != null; level++)
            {
                string d = dir.FullName;
                candidates.Add(Path.Combine(d, "core", "scrcpy.exe"));
                candidates.Add(Path.Combine(d, "scrcpy.exe"));
                candidates.Add(Path.Combine(d, "build-win64", "app", "scrcpy.exe"));
                candidates.Add(Path.Combine(d, "成品", "core", "scrcpy.exe"));
                candidates.Add(Path.Combine(d, "..", "成品", "core", "scrcpy.exe"));
                dir = dir.Parent;
            }

            // 3. 常见安装位置
            candidates.Add(@"D:\Data\Scrcpy\scrcpy.exe");
            candidates.Add(@"C:\Data\Scrcpy\scrcpy.exe");
            candidates.Add(@"C:\scrcpy\scrcpy.exe");

            // 自身 exe 路径：GUI 与引擎同名时，绝不能被当成引擎（防自启动递归）
            string selfExe = "";
            try { selfExe = Path.GetFullPath(Application.ExecutablePath); }
            catch { }

            foreach (string p in candidates)
            {
                try
                {
                    if (!File.Exists(p)) continue;
                    string full = Path.GetFullPath(p);
                    if (!string.IsNullOrEmpty(selfExe)
                        && string.Equals(full, selfExe, StringComparison.OrdinalIgnoreCase))
                        continue;
                    SetCorePath(full);
                    return;
                }
                catch { }
            }

            SetCorePath(Path.Combine(baseDir, "core", "scrcpy.exe"));
        }

        private void SetCorePath(string path)
        {
            scrcpyExePath = path;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                string adb = Path.Combine(dir, "adb.exe");
                adbExePath = File.Exists(adb) ? adb : "adb";
            }
            else
            {
                adbExePath = "adb";
            }

            try
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scrcpy_gui.cfg"), path);
            }
            catch { }

            UpdateEngineStatusDisplay();
        }

        private void UpdateEngineStatusDisplay()
        {
            if (lblScrcpyPathVal != null)
                lblScrcpyPathVal.Text = string.IsNullOrEmpty(scrcpyExePath) ? "未指定" : scrcpyExePath;

            if (lblAdbPathVal != null)
                lblAdbPathVal.Text = string.IsNullOrEmpty(adbExePath) ? "未指定" : adbExePath;

            // 标注：当前引擎是否来自 exe 内嵌包（首次启动自动释放）
            if (lblEngineOrigin != null)
            {
                string bundleRoot = CoreBundle.BundleRoot;
                bool fromBundle = !string.IsNullOrEmpty(scrcpyExePath)
                                  && scrcpyExePath.StartsWith(bundleRoot, StringComparison.OrdinalIgnoreCase)
                                  && File.Exists(Path.Combine(bundleRoot, ".bundle"));
                if (fromBundle)
                {
                    lblEngineOrigin.Text = "● 内嵌引擎 · 已随 exe 打包并释放到本目录 core 文件夹，无需任何外部文件";
                    lblEngineOrigin.ForeColor = UITheme.AccentGreen;
                }
                else if (!string.IsNullOrEmpty(scrcpyExePath))
                {
                    lblEngineOrigin.Text = "● 外置引擎 · 正在使用自定义 core\\scrcpy.exe";
                    lblEngineOrigin.ForeColor = Color.FromArgb(217, 119, 6);
                }
                else
                {
                    lblEngineOrigin.Text = "● 未检测到引擎";
                    lblEngineOrigin.ForeColor = UITheme.DangerRed;
                }
            }

            bool scrcpyOk = File.Exists(scrcpyExePath);
            bool adbOk = File.Exists(adbExePath) || adbExePath == "adb";

            if (lblScrcpyStatus != null)
            {
                lblScrcpyStatus.Text = scrcpyOk ? "● 引擎就绪" : "● 核心缺失";
                lblScrcpyStatus.ForeColor = scrcpyOk ? UITheme.AccentGreen : UITheme.DangerRed;
            }

            if (lblAdbStatus != null)
            {
                lblAdbStatus.Text = adbOk ? "● 服务就绪" : "● 未找到 ADB";
                lblAdbStatus.ForeColor = adbOk ? UITheme.AccentGreen : Color.FromArgb(217, 119, 6);
            }
        }

        #endregion

        #region 顶栏与 Satelite 悬浮居中大药丸导航栏

        private void BuildCustomTitleBar()
        {
            customTitleBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, TitleBarH),
                BackColor = UITheme.WindowBg
            };
            customTitleBar.MouseDown += DragWindow;
            customTitleBar.DoubleClick += (s, e) => ToggleMaximized();

            // 标题栏 Logo 与完整文字，按比例拉宽间距，彻底杜绝切除
            lblTitleLogo = new Label
            {
                Text = "◈",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = UITheme.AccentGreen,
                BackColor = UITheme.WindowBg,
                Location = new Point(UITheme.Dpi(16), UITheme.Dpi(8)),
                AutoSize = true
            };
            lblTitleLogo.MouseDown += DragWindow;

            lblTitleText = new Label
            {
                Text = "Scrcpy",
                Font = UITheme.FontBrand,
                ForeColor = UITheme.TextPrimary,
                BackColor = UITheme.WindowBg,
                Location = new Point(UITheme.Dpi(40), UITheme.Dpi(9)),
                AutoSize = true
            };
            lblTitleText.MouseDown += DragWindow;

            btnMin = new MintButton
            {
                Text = "—",
                Style = MintButton.ButtonStyle.Ghost,
                Size = new Size(UITheme.Dpi(36), UITheme.Dpi(28)),
                Radius = 4
            };
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            btnMax = new MintButton
            {
                Text = "□",
                Style = MintButton.ButtonStyle.Ghost,
                Size = new Size(UITheme.Dpi(36), UITheme.Dpi(28)),
                Radius = 4
            };
            btnMax.Click += (s, e) => ToggleMaximized();

            btnClose = new MintButton
            {
                Text = "✕",
                Style = MintButton.ButtonStyle.CloseGhost,
                Size = new Size(UITheme.Dpi(36), UITheme.Dpi(28)),
                Radius = 4
            };
            btnClose.Click += (s, e) => Application.Exit();

            customTitleBar.Controls.AddRange(new Control[] { lblTitleLogo, lblTitleText, btnMin, btnMax, btnClose });
            Controls.Add(customTitleBar);
        }

        private void ToggleMaximized()
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else
                WindowState = FormWindowState.Maximized;
            if (btnMax != null)
                btnMax.Text = (WindowState == FormWindowState.Maximized) ? "❐" : "□";
        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void BuildTopNavigationBar()
        {
            topNavPanel = new Panel
            {
                Location = new Point(0, TitleBarH),
                Size = new Size(ClientSize.Width, TopNavH),
                BackColor = UITheme.WindowBg
            };
            topNavPanel.MouseDown += DragWindow;

            int capsuleW = UITheme.Dpi(720);
            int capsuleH = UITheme.Dpi(46);

            navCapsule = new Panel
            {
                Size = new Size(capsuleW, capsuleH),
                Location = new Point((ClientSize.Width - capsuleW) / 2, UITheme.Dpi(4)),
                BackColor = Color.Transparent
            };
            navCapsule.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, navCapsule.Width - 1, navCapsule.Height - 1);
                using (GraphicsPath p = Geometry.Pill(r))
                using (SolidBrush b = new SolidBrush(UITheme.HeaderPillBg))
                    g.FillPath(b, p);
            };

            capsuleBadge = new Label
            {
                Text = "◈ SCRCPY",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.AccentGreen,
                BackColor = Color.Transparent,
                Location = new Point(UITheme.Dpi(16), UITheme.Dpi(12)),
                AutoSize = true
            };
            navCapsule.Controls.Add(capsuleBadge);

            string[] tabs = new string[] { "概览工作台", "画质性能", "实用工具", "引擎设置" };
            int tabW = UITheme.Dpi(104);
            int tabH = UITheme.Dpi(36);
            int startX = UITheme.Dpi(130);
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;
                PillNavTab tab = new PillNavTab(tabs[i])
                {
                    Location = new Point(startX, UITheme.Dpi(5)),
                    Size = new Size(tabW, tabH)
                };
                tab.Click += (s, e) => SelectNavTab(index);
                navCapsule.Controls.Add(tab);
                navTabs.Add(tab);
                startX += tabW + UITheme.Dpi(6);
            }

            rightInd = new Label
            {
                Text = "● 投屏引擎",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(navCapsule.Width - UITheme.Dpi(100), UITheme.Dpi(14)),
                AutoSize = true
            };
            navCapsule.Controls.Add(rightInd);

            topNavPanel.Controls.Add(navCapsule);
            Controls.Add(topNavPanel);
        }

        #endregion

        #region 内容主容器与 Tab 切换

        private void BuildContentPages()
        {
            int padX = UITheme.Dpi(20);
            int contentY = TitleBarH + TopNavH + UITheme.Dpi(4);
            contentPanel = new Panel
            {
                Location = new Point(padX, contentY),
                Size = new Size(ClientSize.Width - padX * 2, ClientSize.Height - contentY - UITheme.Dpi(12)),
                BackColor = UITheme.WindowBg
            };

            pageDashboard = CreatePageDashboard();
            pageDisplay = CreatePageDisplay();
            pageTools = CreatePageTools();
            pageSettings = CreatePageSettings();

            contentPanel.Controls.AddRange(new Control[] { pageDashboard, pageDisplay, pageTools, pageSettings });

            toastHost = new MintToastHost();
            toastHost.Visible = false; // 初始隐藏，避免空状态遮挡底栏卡片
            toastHost.Location = new Point((contentPanel.Width - toastHost.Width) / 2, contentPanel.Height - toastHost.Height - UITheme.Dpi(6));
            contentPanel.Controls.Add(toastHost);
            toastHost.BringToFront();

            Controls.Add(contentPanel);
        }

        private void LayoutResponsiveChrome()
        {
            if (customTitleBar == null) return;
            customTitleBar.Width = ClientSize.Width;

            int btnW = UITheme.Dpi(36);
            int btnH = UITheme.Dpi(28);
            int top = UITheme.Dpi(6);

            if (btnClose != null) btnClose.SetBounds(ClientSize.Width - btnW - UITheme.Dpi(8), top, btnW, btnH);
            if (btnMax != null) btnMax.SetBounds(ClientSize.Width - btnW * 2 - UITheme.Dpi(14), top, btnW, btnH);
            if (btnMin != null) btnMin.SetBounds(ClientSize.Width - btnW * 3 - UITheme.Dpi(20), top, btnW, btnH);

            if (topNavPanel != null)
            {
                topNavPanel.Width = ClientSize.Width;
                if (navCapsule != null)
                    navCapsule.Left = (ClientSize.Width - navCapsule.Width) / 2;
            }

            int padX = UITheme.Dpi(20);
            if (contentPanel != null)
            {
                int contentY = TitleBarH + TopNavH + UITheme.Dpi(4);
                contentPanel.SetBounds(padX, contentY, ClientSize.Width - padX * 2, ClientSize.Height - contentY - UITheme.Dpi(12));
            }

            if (toastHost != null && contentPanel != null)
            {
                toastHost.Left = (contentPanel.Width - toastHost.Width) / 2;
                toastHost.Top = contentPanel.Height - toastHost.Height - UITheme.Dpi(6);
                toastHost.BringToFront();
            }
        }

        private void LayoutResponsivePages()
        {
            if (contentPanel == null) return;
            int cw = contentPanel.Width;
            int ch = contentPanel.Height;

            // 1. Dashboard 响应式自适应
            if (cardHero != null)
            {
                cardHero.Width = cw;
                if (heroRightGroup != null)
                    heroRightGroup.Left = Math.Max(UITheme.Dpi(520), cw - heroRightGroup.Width - UITheme.Dpi(16));
            }
            if (cardBanner != null)
            {
                cardBanner.Width = cw;
                if (btnFastGo != null) btnFastGo.Left = cw - btnFastGo.Width - UITheme.Dpi(16);
            }
            if (c1 != null && c2 != null && c3 != null)
            {
                int cardGap = UITheme.Dpi(14);
                int cardW = (cw - cardGap * 2) / 3;
                int botTop = cardBanner != null ? cardBanner.Bottom + UITheme.Dpi(12) : UITheme.Dpi(256);
                int botH = Math.Max(UITheme.Dpi(180), ch - botTop);

                c1.SetBounds(0, botTop, cardW, botH);
                c2.SetBounds(cardW + cardGap, botTop, cardW, botH);
                c3.SetBounds((cardW + cardGap) * 2, botTop, cardW, botH);

                AdjustSwitchPosition(c1, cardW);
                AdjustSwitchPosition(c2, cardW);
            }

            // 2. Display 响应式自适应
            if (cardPresets != null) cardPresets.Width = cw;
            if (cardVideo != null && cardAudio != null)
            {
                int gap = UITheme.Dpi(14);
                int colW = (cw - gap) / 2;
                int topY = cardPresets != null ? cardPresets.Bottom + UITheme.Dpi(10) : UITheme.Dpi(70);
                int colH = Math.Max(UITheme.Dpi(320), ch - topY);

                cardVideo.SetBounds(0, topY, colW, colH);
                cardAudio.SetBounds(colW + gap, topY, colW, colH);

                AdjustDisplayRow(cardVideo, colW);
                AdjustDisplayRow(cardAudio, colW);
            }

            // 3. Tools 响应式自适应
            if (cardKeys != null)
            {
                cardKeys.Width = cw;
                int keyCols = 5;
                int keyGap = UITheme.Dpi(12);
                int btnW = (cw - UITheme.Dpi(36) - keyGap * (keyCols - 1)) / keyCols;
                int btnH = UITheme.Dpi(40);
                for (int i = 0; i < toolButtons.Count; i++)
                {
                    int row = i / keyCols;
                    int col = i % keyCols;
                    int x = UITheme.Dpi(18) + col * (btnW + keyGap);
                    int y = UITheme.Dpi(48) + row * UITheme.Dpi(58);
                    toolButtons[i].SetBounds(x, y, btnW, btnH);
                }
            }
            if (cardClip != null)
            {
                cardClip.Width = cw;
                int topY = cardKeys != null ? cardKeys.Bottom + UITheme.Dpi(10) : UITheme.Dpi(250);
                cardClip.SetBounds(0, topY, cw, Math.Max(UITheme.Dpi(180), ch - topY));

                if (txtClip != null && btnSendText != null)
                {
                    txtClip.SetBounds(UITheme.Dpi(18), UITheme.Dpi(68), cw - UITheme.Dpi(180), UITheme.Dpi(38));
                    btnSendText.SetBounds(cw - UITheme.Dpi(150), UITheme.Dpi(68), UITheme.Dpi(132), UITheme.Dpi(38));
                }
                foreach (Control c in cardClip.Controls)
                {
                    if (c.Height == 1) c.Width = cw - UITheme.Dpi(36);
                }
            }

            // 4. Settings 响应式自适应
            if (cardPath != null)
            {
                cardPath.Width = cw;
                if (lblScrcpyPathVal != null)
                    lblScrcpyPathVal.Width = Math.Max(UITheme.Dpi(200), cw - UITheme.Dpi(400));
                if (lblAdbPathVal != null)
                    lblAdbPathVal.Width = Math.Max(UITheme.Dpi(200), cw - UITheme.Dpi(400));
                if (lblScrcpyStatus != null) lblScrcpyStatus.Left = cw - UITheme.Dpi(250);
                if (lblAdbStatus != null) lblAdbStatus.Left = cw - UITheme.Dpi(250);
                if (btnBrowseScrcpy != null) btnBrowseScrcpy.Left = cw - UITheme.Dpi(140);
                if (btnBrowseAdb != null) btnBrowseAdb.Left = cw - UITheme.Dpi(140);
            }
            if (cardLog != null)
            {
                cardLog.Width = cw;
                int topY = cardPath != null ? cardPath.Bottom + UITheme.Dpi(10) : UITheme.Dpi(180);
                cardLog.SetBounds(0, topY, cw, Math.Max(UITheme.Dpi(180), ch - topY));

                if (btnClearLog != null) btnClearLog.Left = cw - btnClearLog.Width - UITheme.Dpi(16);
                if (txtLogMonitor != null) txtLogMonitor.SetBounds(UITheme.Dpi(18), UITheme.Dpi(46), cw - UITheme.Dpi(36), cardLog.Height - UITheme.Dpi(58));
            }
        }

        private void AdjustSwitchPosition(MintCard c, int cardW)
        {
            foreach (Control sub in c.Controls)
            {
                if (sub is MintToggle)
                    sub.Left = Math.Max(UITheme.Dpi(150), cardW - sub.Width - UITheme.Dpi(16));
            }
        }

        private void AdjustDisplayRow(MintCard card, int colW)
        {
            foreach (Control sub in card.Controls)
            {
                if (sub is MintComboBox)
                {
                    int comboW = Math.Min(UITheme.Dpi(240), Math.Max(UITheme.Dpi(160), colW - UITheme.Dpi(200)));
                    sub.SetBounds(colW - comboW - UITheme.Dpi(18), sub.Top, comboW, UITheme.Dpi(36));
                }
            }
        }

        private void SelectNavTab(int index)
        {
            for (int i = 0; i < navTabs.Count; i++)
                navTabs[i].IsSelected = (i == index);

            pageDashboard.Visible = (index == 0);
            pageDisplay.Visible = (index == 1);
            pageTools.Visible = (index == 2);
            pageSettings.Visible = (index == 3);
        }

        private void ShowToast(string text, ToastKind kind)
        {
            AppendLog(text);
            if (toastHost != null)
                toastHost.Show(text, kind);
        }

        private void AppendLog(string message)
        {
            if (txtLogMonitor != null && !txtLogMonitor.IsDisposed)
            {
                if (txtLogMonitor.InvokeRequired)
                {
                    txtLogMonitor.BeginInvoke(new Action(() => AppendLog(message)));
                    return;
                }
                string time = DateTime.Now.ToString("HH:mm:ss");
                txtLogMonitor.AppendText(string.Format("[{0}] {1}\r\n", time, message));

            }
        }

        #endregion

        #region Page 1: 投屏工作台 (100% 还原参考图 Satelite 卡片网格)

        private Panel CreatePageDashboard()
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.WindowBg };
            int w = ClientSize.Width - UITheme.Dpi(40);

            // 1. 顶部 Hero 大卡片 (Satelite 核心状态区)
            cardHero = new MintCard { Location = new Point(0, 0), Size = new Size(w, UITheme.Dpi(186)), Radius = 16 };

            // 左侧状态大图标 (使用平滑纯矢量绘制的手机形态)
            heroIcon = new Label
            {
                Text = "📱",
                Font = new Font("Segoe UI Emoji", 32F),
                Location = new Point(UITheme.Dpi(24), UITheme.Dpi(32)),
                Size = new Size(UITheme.Dpi(72), UITheme.Dpi(72)),
                BackColor = Color.Transparent
            };
            cardHero.Controls.Add(heroIcon);

            // 中间状态信息
            lblHeroState = new Label
            {
                Text = "● STANDBY · SCRCPY 5.0.1",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(UITheme.Dpi(124), UITheme.Dpi(22)),
                AutoSize = true
            };
            lblHeroDeviceName = new Label
            {
                Text = "未连接设备",
                Font = UITheme.FontHeroTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(UITheme.Dpi(120), UITheme.Dpi(44)),
                AutoSize = true
            };
            lblHeroSub = new Label
            {
                Text = "开启手机 USB 调试并连接电脑，点击启动即可零延迟超清投屏",
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.Transparent,
                Location = new Point(UITheme.Dpi(124), UITheme.Dpi(96)),
                AutoSize = true
            };

            // 大动作按钮 (Satelite 风格大胶囊药丸)
            btnLaunchPrimary = new MintButton
            {
                Text = "启动投屏",
                Location = new Point(UITheme.Dpi(124), UITheme.Dpi(128)),
                Size = new Size(UITheme.Dpi(126), UITheme.Dpi(40)),
                Style = MintButton.ButtonStyle.Accent,
                Font = UITheme.FontBodyBold
            };
            btnLaunchPrimary.Click += BtnLaunchPrimary_Click;

            btnStopPrimary = new MintButton
            {
                Text = "终止投屏",
                Location = new Point(UITheme.Dpi(260), UITheme.Dpi(128)),
                Size = new Size(UITheme.Dpi(112), UITheme.Dpi(40)),
                Style = MintButton.ButtonStyle.Danger,
                Font = UITheme.FontBodyBold,
                Enabled = false
            };
            btnStopPrimary.Click += BtnStopPrimary_Click;

            btnMore = new MintButton
            {
                Text = "···",
                Location = new Point(UITheme.Dpi(382), UITheme.Dpi(128)),
                Size = new Size(UITheme.Dpi(52), UITheme.Dpi(40)),
                Style = MintButton.ButtonStyle.Secondary
            };
            btnMore.Click += (s, e) => ToggleNavBar();

            cardHero.Controls.AddRange(new Control[] {
                lblHeroState, lblHeroDeviceName, lblHeroSub,
                btnLaunchPrimary, btnStopPrimary, btnMore
            });

            // 右侧快捷切换组 (按比例拉宽，彻底杜绝重叠与挤压)
            int rightW = UITheme.Dpi(420);
            int rightH = UITheme.Dpi(160);

            heroRightGroup = new Panel
            {
                Location = new Point(w - rightW - UITheme.Dpi(16), UITheme.Dpi(14)),
                Size = new Size(rightW, rightH),
                BackColor = Color.Transparent
            };
            lblRTitle = new Label { Text = "快捷", Font = UITheme.FontSmall, ForeColor = UITheme.TextSecondary, Location = new Point(UITheme.Dpi(6), UITheme.Dpi(4)), AutoSize = true };
            heroRightGroup.Controls.Add(lblRTitle);

            // 第一行：设备选择
            r1 = new Label { Text = "设备列表", Font = UITheme.FontItemSub, ForeColor = UITheme.TextPrimary, Location = new Point(UITheme.Dpi(6), UITheme.Dpi(28)), AutoSize = true };
            comboDevices = new MintComboBox { Location = new Point(UITheme.Dpi(80), UITheme.Dpi(22)), Size = new Size(UITheme.Dpi(210), UITheme.Dpi(34)) };
            btnRefreshDev = new MintButton { Text = "刷新", Location = new Point(UITheme.Dpi(300), UITheme.Dpi(22)), Size = new Size(UITheme.Dpi(104), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Secondary };
            btnRefreshDev.Click += (s, e) => RefreshDeviceList();
            heroRightGroup.Controls.AddRange(new Control[] { r1, comboDevices, btnRefreshDev });

            // 第二行：无线配对
            r2 = new Label { Text = "无线配对", Font = UITheme.FontItemSub, ForeColor = UITheme.TextPrimary, Location = new Point(UITheme.Dpi(6), UITheme.Dpi(70)), AutoSize = true };
            txtWirelessIp = new MintTextBox { Location = new Point(UITheme.Dpi(80), UITheme.Dpi(64)), Size = new Size(UITheme.Dpi(120), UITheme.Dpi(34)) };
            txtWirelessIp.Text = "192.168.1.";
            btnAdbTcpip = new MintButton { Text = "开端口", Location = new Point(UITheme.Dpi(210), UITheme.Dpi(64)), Size = new Size(UITheme.Dpi(80), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Secondary };
            btnAdbTcpip.Click += BtnAdbTcpip_Click;
            btnWirelessConnect = new MintButton { Text = "连接", Location = new Point(UITheme.Dpi(300), UITheme.Dpi(64)), Size = new Size(UITheme.Dpi(104), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Accent };
            btnWirelessConnect.Click += BtnWirelessConnect_Click;
            heroRightGroup.Controls.AddRange(new Control[] { r2, txtWirelessIp, btnAdbTcpip, btnWirelessConnect });

            // 第三行：快捷动作
            r3 = new Label { Text = "辅助工具", Font = UITheme.FontItemSub, ForeColor = UITheme.TextPrimary, Location = new Point(UITheme.Dpi(6), UITheme.Dpi(112)), AutoSize = true };
            btnShot = new MintButton { Text = "📸 截屏", Location = new Point(UITheme.Dpi(80), UITheme.Dpi(106)), Size = new Size(UITheme.Dpi(100), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Secondary };
            btnShot.Click += (s, e) => TakeDeviceScreenshot();
            btnWake = new MintButton { Text = "💡 唤醒", Location = new Point(UITheme.Dpi(190), UITheme.Dpi(106)), Size = new Size(UITheme.Dpi(100), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Secondary };
            btnWake.Click += (s, e) => SendAdbKeyEvent(224);
            btnRestart = new MintButton { Text = "🔄 重启", Location = new Point(UITheme.Dpi(300), UITheme.Dpi(106)), Size = new Size(UITheme.Dpi(104), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Secondary };
            btnRestart.Click += (s, e) => RestartAdbServer();
            heroRightGroup.Controls.AddRange(new Control[] { r3, btnShot, btnWake, btnRestart });

            cardHero.Controls.Add(heroRightGroup);
            pnl.Controls.Add(cardHero);

            // 2. 中间提示通栏条 (1:1 还原参考图浅绿微条)
            cardBanner = new MintCard
            {
                Location = new Point(0, cardHero.Bottom + UITheme.Dpi(10)),
                Size = new Size(w, UITheme.Dpi(52)),
                Radius = 14,
                BackColor = Color.FromArgb(235, 246, 240)
            };
            lblBanner = new Label
            {
                Text = "⚡ 零黑框极客模式已激活 · 投屏时自动关闭手机物理屏幕省电并保护隐私",
                Font = UITheme.FontBody,
                ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(UITheme.Dpi(20), UITheme.Dpi(16)),
                AutoSize = true
            };
            btnFastGo = new MintButton
            {
                Text = "画质调校",
                Location = new Point(w - UITheme.Dpi(124), UITheme.Dpi(9)),
                Size = new Size(UITheme.Dpi(106), UITheme.Dpi(34)),
                Style = MintButton.ButtonStyle.Accent
            };
            btnFastGo.Click += (s, e) => SelectNavTab(1);
            cardBanner.Controls.AddRange(new Control[] { lblBanner, btnFastGo });
            pnl.Controls.Add(cardBanner);

            // 3. 底部 3 列 Satelite 瓷白卡片矩阵 (1:1 还原参考图下半部)
            int cardGap = UITheme.Dpi(14);
            int cardW = (w - cardGap * 2) / 3;
            int botTop = cardBanner.Bottom + UITheme.Dpi(10);
            int botH = UITheme.Dpi(220);

            // 卡片 1: 渲染与显示特性
            c1 = new MintCard { Location = new Point(0, botTop), Size = new Size(cardW, botH), Radius = 14 };
            Label c1Title = new Label { Text = "投屏特性 · 渲染策略", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(16), UITheme.Dpi(14)), AutoSize = true };
            c1.Controls.Add(c1Title);
            AddMiniSwitch(c1, UITheme.Dpi(16), UITheme.Dpi(48), "自动息屏", "关闭手机物理屏幕省电降温", out togScreenOff, false);
            AddMiniSwitch(c1, UITheme.Dpi(16), UITheme.Dpi(102), "窗口置顶", "固定在所有应用最前端", out togAlwaysOnTop, false);
            AddMiniSwitch(c1, UITheme.Dpi(16), UITheme.Dpi(156), "无边框模式", "隐藏投屏原生标题栏", out togBorderless, false);
            pnl.Controls.Add(c1);

            // 卡片 2: 系统常亮与交互
            c2 = new MintCard { Location = new Point(cardW + cardGap, botTop), Size = new Size(cardW, botH), Radius = 14 };
            Label c2Title = new Label { Text = "系统策略 · 交互感知", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(16), UITheme.Dpi(14)), AutoSize = true };
            c2.Controls.Add(c2Title);
            AddMiniSwitch(c2, UITheme.Dpi(16), UITheme.Dpi(48), "保持常亮", "投屏期间阻止设备自动锁屏", out togStayAwake, true);
            AddMiniSwitch(c2, UITheme.Dpi(16), UITheme.Dpi(102), "音频转发", "声音极低延迟回传电脑", out togAudio, true);
            AddMiniSwitch(c2, UITheme.Dpi(16), UITheme.Dpi(156), "绘制触控点", "画面同步呈现点击滑动圆点", out togTouches, false);
            pnl.Controls.Add(c2);

            // 卡片 3: 快捷键速查与核心状态
            c3 = new MintCard { Location = new Point((cardW + cardGap) * 2, botTop), Size = new Size(cardW, botH), Radius = 14 };
            Label c3Title = new Label { Text = "快捷指令 · 核心运行", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(16), UITheme.Dpi(14)), AutoSize = true };
            c3.Controls.Add(c3Title);

            string[] scList = new string[] {
                "• Alt+F 全屏切换", "• Alt+G 1:1原画比例", "• Alt+H 主页 (Home)",
                "• Alt+B 返回 (Back)", "• Alt+S 任务切换", "• Alt+P 电源键"
            };
            int scY = UITheme.Dpi(48);
            foreach (string sc in scList)
            {
                Label lblSc = new Label { Text = sc, Font = UITheme.FontItemSub, ForeColor = UITheme.TextSecondary, Location = new Point(UITheme.Dpi(16), scY), AutoSize = true };
                c3.Controls.Add(lblSc);
                scY += UITheme.Dpi(26);
            }
            pnl.Controls.Add(c3);

            return pnl;
        }

        private void AddMiniSwitch(Control parent, int x, int y, string title, string sub, out MintToggle toggle, bool def)
        {
            Label lblT = new Label
            {
                Text = title,
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(x, y),
                AutoSize = true
            };
            Label lblS = new Label
            {
                Text = sub,
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.Transparent,
                Location = new Point(x, y + UITheme.Dpi(22)),
                AutoSize = true
            };
            toggle = new MintToggle
            {
                Location = new Point(Math.Max(UITheme.Dpi(180), parent.Width - UITheme.Dpi(62)), y + UITheme.Dpi(8)),
                Checked = def
            };
            parent.Controls.AddRange(new Control[] { lblT, lblS, toggle });
        }

        #endregion

        #region Page 2: 画质与性能调校 (Display)

        private Panel CreatePageDisplay()
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.WindowBg };
            int w = ClientSize.Width - UITheme.Dpi(40);

            // 1. 顶部预设方案芯片组
            cardPresets = new MintCard { Location = new Point(0, 0), Size = new Size(w, UITheme.Dpi(64)), Radius = 14 };
            lblPTitle = new Label
            {
                Text = "⚡ 画质预设方案 :",
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(UITheme.Dpi(18), UITheme.Dpi(20)),
                AutoSize = true
            };

            int chipW = UITheme.Dpi(96);
            int chipH = UITheme.Dpi(34);
            int chipY = UITheme.Dpi(15);

            chipPerf = new MintChip { Text = "⚡ 性能", Location = new Point(UITheme.Dpi(150), chipY), Size = new Size(chipW, chipH) };
            chipBalanced = new MintChip { Text = "⚖️ 均衡", Location = new Point(UITheme.Dpi(256), chipY), Size = new Size(chipW, chipH) };
            chipQuality = new MintChip { Text = "💎 画质", Location = new Point(UITheme.Dpi(362), chipY), Size = new Size(chipW, chipH) };
            chipCustom = new MintChip { Text = "⚙️ 自定义", Location = new Point(UITheme.Dpi(468), chipY), Size = new Size(chipW + UITheme.Dpi(8), chipH), ReadOnlyChip = true };

            chipPerf.Click += (s, e) => ApplyPreset(0);
            chipBalanced.Click += (s, e) => ApplyPreset(1);
            chipQuality.Click += (s, e) => ApplyPreset(2);

            cardPresets.Controls.AddRange(new Control[] { lblPTitle, chipPerf, chipBalanced, chipQuality, chipCustom });
            pnl.Controls.Add(cardPresets);

            // 2. 视频渲染与硬件加速 (左列大卡片)
            int gap = UITheme.Dpi(14);
            int colW = (w - gap) / 2;
            int colH = UITheme.Dpi(420);
            int topY = cardPresets.Bottom + UITheme.Dpi(10);

            cardVideo = new MintCard { Location = new Point(0, topY), Size = new Size(colW, colH), Radius = 14 };
            Label lblVTitle = new Label { Text = "视频渲染与硬件编码", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(16)), AutoSize = true };
            cardVideo.Controls.Add(lblVTitle);

            int comboW = UITheme.Dpi(220);
            int comboH = UITheme.Dpi(36);

            comboResolution = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(54)), Size = new Size(comboW, comboH) };
            comboResolution.Items.AddRange(new string[] { "跟随原生分辨率 (最高清)", "4K (3840×2160)", "2K (2560×1440)", "1080P (1920×1080 推荐)", "720P (1280×720 流畅)" });
            comboResolution.SelectedIndex = 0;
            AddDisplayRow(cardVideo, UITheme.Dpi(18), UITheme.Dpi(54), "分辨率限制 (Max Size)", "限制长边像素显著降低编码吞吐压力", comboResolution);

            comboFps = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(142)), Size = new Size(comboW, comboH) };
            comboFps.Items.AddRange(new string[] { "原生无限制 (推荐原汁原味高刷)", "144 FPS (电竞极限刷新率)", "120 FPS (旗舰高刷突破)", "90 FPS (平滑高帧模式)", "60 FPS (标准能耗平衡)", "30 FPS (节能低功耗)" });
            comboFps.SelectedIndex = 0;
            AddDisplayRow(cardVideo, UITheme.Dpi(18), UITheme.Dpi(142), "刷新率上限 (Max FPS)", "跟随手机高刷屏或锁定指定帧率", comboFps);

            comboBitrate = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(230)), Size = new Size(comboW, comboH) };
            comboBitrate.Items.AddRange(new string[] { "原生默认 (8 Mbps)", "16 Mbps (超清细腻推荐)", "32 Mbps (发烧极客原画)", "64 Mbps (极限无损细节压制)", "4 Mbps (低网速占用)" });
            comboBitrate.SelectedIndex = 0;
            AddDisplayRow(cardVideo, UITheme.Dpi(18), UITheme.Dpi(230), "传输码率 (Video Bitrate)", "动态细节不足调高，卡顿可回调", comboBitrate);

            comboCodec = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(318)), Size = new Size(comboW, comboH) };
            comboCodec.Items.AddRange(new string[] { "自动协商选择 (Auto)", "H.265 / HEVC (推荐省带宽)", "AV1 (下一代高画质标准)", "H.264 (广泛兼容基线)" });
            comboCodec.SelectedIndex = 0;
            AddDisplayRow(cardVideo, UITheme.Dpi(18), UITheme.Dpi(318), "编码格式 (Video Codec)", "H.265 同画质下可节约近40%带宽", comboCodec);
            pnl.Controls.Add(cardVideo);

            // 3. 音频保真与流控 (右列大卡片)
            cardAudio = new MintCard { Location = new Point(colW + gap, topY), Size = new Size(colW, colH), Radius = 14 };
            Label lblATitle = new Label { Text = "音频保真与流控录制", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(16)), AutoSize = true };
            cardAudio.Controls.Add(lblATitle);

            comboAudioCodec = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(54)), Size = new Size(comboW, comboH) };
            comboAudioCodec.Items.AddRange(new string[] { "Opus (超低延迟推荐)", "AAC (广泛兼容多媒体)", "RAW (未压缩原始音频)" });
            comboAudioCodec.SelectedIndex = 0;
            AddDisplayRow(cardAudio, UITheme.Dpi(18), UITheme.Dpi(54), "音频编码 (Audio Codec)", "Android 11+ 原生抓取内部声音", comboAudioCodec);

            comboAudioBitrate = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(142)), Size = new Size(comboW, comboH) };
            comboAudioBitrate.Items.AddRange(new string[] { "128 kbps (标准CD品质)", "192 kbps (高保真音乐)", "256 kbps (母带级发烧)", "64 kbps (低占用)" });
            comboAudioBitrate.SelectedIndex = 0;
            AddDisplayRow(cardAudio, UITheme.Dpi(18), UITheme.Dpi(142), "采样码率 (Audio Bitrate)", "控制声音回传电脑的压缩品质", comboAudioBitrate);

            comboBuffer = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(230)), Size = new Size(comboW, comboH) };
            comboBuffer.Items.AddRange(new string[] { "0 ms (零缓冲竞技低延迟)", "25 ms (平滑抗抖动)", "50 ms (弱网防花屏)", "100 ms (高容错播放)" });
            comboBuffer.SelectedIndex = 0;
            AddDisplayRow(cardAudio, UITheme.Dpi(18), UITheme.Dpi(230), "显示缓冲区 (Buffer)", "无线网络抖动顿挫可设为 25ms", comboBuffer);

            comboRecord = new MintComboBox { Location = new Point(colW - comboW - UITheme.Dpi(18), UITheme.Dpi(318)), Size = new Size(comboW, comboH) };
            comboRecord.Items.AddRange(new string[] { "仅实时投屏 (不录制)", "同步录制为 MP4 (通用兼容)", "同步录制为 MKV (防断电损坏)" });
            comboRecord.SelectedIndex = 0;
            AddDisplayRow(cardAudio, UITheme.Dpi(18), UITheme.Dpi(318), "同步录制 (Auto Record)", "自动无损保存音视频到工作目录", comboRecord);
            pnl.Controls.Add(cardAudio);

            EventHandler onPresetChanged = (s, e) =>
            {
                if (applyingPreset) return;
                SetPresetSelection(3);
            };
            comboResolution.SelectedIndexChanged += onPresetChanged;
            comboFps.SelectedIndexChanged += onPresetChanged;
            comboBitrate.SelectedIndexChanged += onPresetChanged;
            comboCodec.SelectedIndexChanged += onPresetChanged;
            comboAudioCodec.SelectedIndexChanged += onPresetChanged;
            comboAudioBitrate.SelectedIndexChanged += onPresetChanged;
            comboBuffer.SelectedIndexChanged += onPresetChanged;

            SetPresetSelection(3);
            return pnl;
        }

        private void AddDisplayRow(Control parent, int x, int y, string title, string sub, Control control)
        {
            Label lblT = new Label
            {
                Text = title,
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(x, y + UITheme.Dpi(1)),
                AutoSize = true
            };
            Label lblS = new Label
            {
                Text = sub,
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.Transparent,
                Location = new Point(x, y + UITheme.Dpi(24)),
                AutoSize = true
            };
            parent.Controls.AddRange(new Control[] { lblT, lblS, control });
        }

        private void ApplyPreset(int index)
        {
            applyingPreset = true;
            try
            {
                if (index == 0) // ⚡ 极速性能
                {
                    comboResolution.SelectedIndex = 4; // 720P
                    comboFps.SelectedIndex = 4;        // 60 FPS
                    comboBitrate.SelectedIndex = 4;    // 4 Mbps
                    comboCodec.SelectedIndex = 3;      // H.264
                    comboAudioBitrate.SelectedIndex = 3;
                    comboBuffer.SelectedIndex = 2;     // 50ms
                }
                else if (index == 1) // ⚖️ 均衡推荐
                {
                    comboResolution.SelectedIndex = 3; // 1080P
                    comboFps.SelectedIndex = 3;        // 90 FPS
                    comboBitrate.SelectedIndex = 1;    // 16 Mbps
                    comboCodec.SelectedIndex = 1;      // H.265
                    comboAudioBitrate.SelectedIndex = 0;
                    comboBuffer.SelectedIndex = 1;     // 25ms
                }
                else if (index == 2) // 💎 极致画质
                {
                    comboResolution.SelectedIndex = 0; // 原生分辨率
                    comboFps.SelectedIndex = 2;        // 120 FPS
                    comboBitrate.SelectedIndex = 2;    // 32 Mbps
                    comboCodec.SelectedIndex = 1;      // H.265
                    comboAudioBitrate.SelectedIndex = 1;
                    comboBuffer.SelectedIndex = 0;     // 0ms
                }
            }
            finally
            {
                applyingPreset = false;
            }

            SetPresetSelection(index);
            string[] names = new string[] { "⚡ 性能", "⚖️ 均衡", "💎 画质" };
            ShowToast("已应用预设: " + names[index], ToastKind.Info);
        }

        private void SetPresetSelection(int index)
        {
            if (chipPerf == null) return;
            chipPerf.Selected = (index == 0);
            chipBalanced.Selected = (index == 1);
            chipQuality.Selected = (index == 2);
            chipCustom.Selected = (index == 3);
        }

        #endregion

        #region Page 3: 辅助工具箱 (Tools)

        private Panel CreatePageTools()
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.WindowBg };
            int w = ClientSize.Width - UITheme.Dpi(40);

            // 1. 移动端虚拟按键直通大卡片
            cardKeys = new MintCard { Location = new Point(0, 0), Size = new Size(w, UITheme.Dpi(250)), Radius = 14 };
            Label lblKTitle = new Label { Text = "移动端虚拟按键直通 (Android Key Control)", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(16)), AutoSize = true };
            cardKeys.Controls.Add(lblKTitle);

            // 第一行：常用导航
            AddToolBtn(cardKeys, "🔙 返回 (Back)", 4);
            AddToolBtn(cardKeys, "🏠 主页 (Home)", 3);
            AddToolBtn(cardKeys, "📑 任务 (Recents)", 187);
            AddToolBtn(cardKeys, "🔌 电源 (Power)", 26);
            AddToolBtn(cardKeys, "💡 唤醒点亮", 224, MintButton.ButtonStyle.Accent);

            // 第二行：音频与屏幕
            AddToolBtn(cardKeys, "🔊 音量增加 (+)", 24);
            AddToolBtn(cardKeys, "🔉 音量减小 (-)", 25);
            AddToolBtn(cardKeys, "🔇 静音切换", 164);
            AddToolBtn(cardKeys, "🔄 旋转屏幕 90°", -1);
            AddToolBtn(cardKeys, "📸 高清截屏", -6, MintButton.ButtonStyle.Accent);

            // 第三行：系统管理
            AddToolBtn(cardKeys, "📥 下拉通知栏", -2);
            AddToolBtn(cardKeys, "📤 收起通知栏", -3);
            AddToolBtn(cardKeys, "⚠️ 重启系统", -4, MintButton.ButtonStyle.Danger);
            AddToolBtn(cardKeys, "⚡ 重启 Bootloader", -5, MintButton.ButtonStyle.Danger);
            pnl.Controls.Add(cardKeys);

            // 2. 剪贴板文字推送与常用操作卡片
            int topY = cardKeys.Bottom + UITheme.Dpi(10);
            cardClip = new MintCard { Location = new Point(0, topY), Size = new Size(w, UITheme.Dpi(220)), Radius = 14 };
            Label lblCTitle = new Label { Text = "剪贴板文字直接推送与安装", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(16)), AutoSize = true };
            Label lblCSub = new Label { Text = "输入文字并一键推送到手机当前获得焦点的输入框（解决手机端输入复杂长密码难题）：", Font = UITheme.FontItemSub, ForeColor = UITheme.TextMuted, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(42)), AutoSize = true };
            cardClip.Controls.AddRange(new Control[] { lblCTitle, lblCSub });

            txtClip = new MintTextBox { Location = new Point(UITheme.Dpi(18), UITheme.Dpi(72)), Size = new Size(w - UITheme.Dpi(180), UITheme.Dpi(38)) };
            btnSendText = new MintButton
            {
                Text = "推送文字",
                Location = new Point(w - UITheme.Dpi(150), UITheme.Dpi(72)),
                Size = new Size(UITheme.Dpi(132), UITheme.Dpi(38)),
                Style = MintButton.ButtonStyle.Accent
            };
            btnSendText.Click += (s, e) =>
            {
                string text = txtClip.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    string safe = text.Replace(" ", "%s").Replace("&", "\\&").Replace("\"", "\\\"");
                    RunAdbShell("input text \"" + safe + "\"");
                    ShowToast("文字已成功推送到设备", ToastKind.Success);
                }
            };
            cardClip.Controls.AddRange(new Control[] { txtClip, btnSendText });

            Panel div = new Panel { Location = new Point(UITheme.Dpi(18), UITheme.Dpi(130)), Size = new Size(w - UITheme.Dpi(36), 1), BackColor = UITheme.InnerBorder };
            cardClip.Controls.Add(div);

            btnOpenWorkDir = new MintButton
            {
                Text = "📂 打开工作台目录",
                Location = new Point(UITheme.Dpi(18), UITheme.Dpi(150)),
                Size = new Size(UITheme.Dpi(190), UITheme.Dpi(40)),
                Style = MintButton.ButtonStyle.Secondary
            };
            btnOpenWorkDir.Click += (s, e) => Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);

            btnInstallApk = new MintButton
            {
                Text = "📦 选择 APK 安装到设备",
                Location = new Point(UITheme.Dpi(224), UITheme.Dpi(150)),
                Size = new Size(UITheme.Dpi(210), UITheme.Dpi(40)),
                Style = MintButton.ButtonStyle.Secondary
            };
            btnInstallApk.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Android 安装包 (*.apk)|*.apk" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ShowToast("正在安装 APK...", ToastKind.Info);
                    RunAdbCommand("install -r \"" + ofd.FileName + "\"", (outStr) =>
                    {
                        bool ok = outStr.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0;
                        ShowToast(ok ? "APK 安装完成" : "APK 安装反馈异常", ok ? ToastKind.Success : ToastKind.Error);
                        AppendLog("APK 安装反馈: " + outStr.Trim());
                    });
                }
            };
            cardClip.Controls.AddRange(new Control[] { btnOpenWorkDir, btnInstallApk });
            pnl.Controls.Add(cardClip);

            return pnl;
        }

        private void AddToolBtn(Control parent, string text, int keyCode, MintButton.ButtonStyle style = MintButton.ButtonStyle.Secondary)
        {
            MintButton btn = new MintButton
            {
                Text = text,
                Style = style,
                Radius = 18
            };
            btn.Click += (s, e) =>
            {
                if (keyCode >= 0)
                {
                    SendAdbKeyEvent(keyCode);
                    ShowToast("已触发: " + text, ToastKind.Info);
                }
                else if (keyCode == -1) RotateDeviceScreen();
                else if (keyCode == -2) RunAdbShell("cmd statusbar expand-notifications");
                else if (keyCode == -3) RunAdbShell("cmd statusbar collapse");
                else if (keyCode == -4) RebootDevice();
                else if (keyCode == -5) RebootBootloader();
                else if (keyCode == -6) TakeDeviceScreenshot();
            };
            toolButtons.Add(btn);
            parent.Controls.Add(btn);
        }

        #endregion

        #region Page 4: 引擎设置与日志 (Settings)

        private Panel CreatePageSettings()
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.WindowBg };
            int w = ClientSize.Width - UITheme.Dpi(40);

            // 1. 核心程序路径卡片
            cardPath = new MintCard { Location = new Point(0, 0), Size = new Size(w, UITheme.Dpi(168)), Radius = 14 };
            Label lblPTitle = new Label { Text = "核心路径配置 (Core Engine & ADB Paths)", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(16)), AutoSize = true };
            cardPath.Controls.Add(lblPTitle);

            // scrcpy 核心行
            Label lblSLabel = new Label { Text = "scrcpy 核心 :", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(48)), AutoSize = true };
            lblScrcpyPathVal = new Label { Text = scrcpyExePath, Font = UITheme.FontMono, ForeColor = UITheme.TextSecondary, Location = new Point(UITheme.Dpi(130), UITheme.Dpi(50)), Size = new Size(w - UITheme.Dpi(400), UITheme.Dpi(20)), AutoEllipsis = true };
            lblScrcpyStatus = new Label { Text = "● 检测中", Font = UITheme.FontSmall, ForeColor = Color.FromArgb(217, 119, 6), Location = new Point(w - UITheme.Dpi(250), UITheme.Dpi(50)), AutoSize = true };
            btnBrowseScrcpy = new MintButton { Text = "浏览...", Location = new Point(w - UITheme.Dpi(140), UITheme.Dpi(44)), Size = new Size(UITheme.Dpi(120), UITheme.Dpi(32)), Style = MintButton.ButtonStyle.Secondary };
            btnBrowseScrcpy.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "scrcpy 可执行文件 (scrcpy.exe)|scrcpy.exe" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SetCorePath(ofd.FileName);
                    ShowToast("已更新 scrcpy.exe 路径", ToastKind.Success);
                }
            };

            // adb 调试工具行
            Label lblALabel = new Label { Text = "adb 调试工具 :", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(88)), AutoSize = true };
            lblAdbPathVal = new Label { Text = adbExePath, Font = UITheme.FontMono, ForeColor = UITheme.TextSecondary, Location = new Point(UITheme.Dpi(130), UITheme.Dpi(90)), Size = new Size(w - UITheme.Dpi(400), UITheme.Dpi(20)), AutoEllipsis = true };
            lblAdbStatus = new Label { Text = "● 检测中", Font = UITheme.FontSmall, ForeColor = Color.FromArgb(217, 119, 6), Location = new Point(w - UITheme.Dpi(250), UITheme.Dpi(90)), AutoSize = true };
            btnBrowseAdb = new MintButton { Text = "浏览...", Location = new Point(w - UITheme.Dpi(140), UITheme.Dpi(84)), Size = new Size(UITheme.Dpi(120), UITheme.Dpi(32)), Style = MintButton.ButtonStyle.Secondary };
            btnBrowseAdb.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "ADB 可执行文件 (adb.exe)|adb.exe" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    adbExePath = ofd.FileName;
                    UpdateEngineStatusDisplay();
                    ShowToast("已手动指定 ADB 路径", ToastKind.Success);
                }
            };

            btnAutoDetect = new MintButton { Text = "🔍 自动检测环境", Location = new Point(UITheme.Dpi(18), UITheme.Dpi(124)), Size = new Size(UITheme.Dpi(160), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Secondary };
            btnAutoDetect.Click += (s, e) =>
            {
                DetectScrcpyEngine();
                RefreshDeviceList();
                ShowToast("已完成环境自检", ToastKind.Info);
            };

            btnRestartAdbServer = new MintButton { Text = "🔄 重启 ADB 守护进程", Location = new Point(UITheme.Dpi(188), UITheme.Dpi(124)), Size = new Size(UITheme.Dpi(180), UITheme.Dpi(34)), Style = MintButton.ButtonStyle.Secondary };
            btnRestartAdbServer.Click += (s, e) => RestartAdbServer();

            lblEngineOrigin = new Label
            {
                Text = "● 检测中",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(UITheme.Dpi(366), UITheme.Dpi(130)),
                Size = new Size(w - UITheme.Dpi(390), UITheme.Dpi(20)),
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };

            cardPath.Controls.AddRange(new Control[] {
                lblSLabel, lblScrcpyPathVal, lblScrcpyStatus, btnBrowseScrcpy,
                lblALabel, lblAdbPathVal, lblAdbStatus, btnBrowseAdb,
                btnAutoDetect, btnRestartAdbServer, lblEngineOrigin
            });
            pnl.Controls.Add(cardPath);

            // 2. 终端实时运行日志卡片
            int topY = cardPath.Bottom + UITheme.Dpi(10);
            cardLog = new MintCard { Location = new Point(0, topY), Size = new Size(w, UITheme.Dpi(300)), Radius = 14 };
            Label lblLTitle = new Label { Text = "工作台实时运行控制台日志", Font = UITheme.FontCardHeader, ForeColor = UITheme.AccentGreen, Location = new Point(UITheme.Dpi(18), UITheme.Dpi(14)), AutoSize = true };
            btnClearLog = new MintButton { Text = "清空日志", Location = new Point(w - UITheme.Dpi(110), UITheme.Dpi(10)), Size = new Size(UITheme.Dpi(96), UITheme.Dpi(30)), Style = MintButton.ButtonStyle.Ghost };
            btnClearLog.Click += (s, e) => { txtLogMonitor.Clear(); };
            cardLog.Controls.AddRange(new Control[] { lblLTitle, btnClearLog });

            txtLogMonitor = new TextBox
            {
                Location = new Point(UITheme.Dpi(18), UITheme.Dpi(48)),
                Size = new Size(w - UITheme.Dpi(36), cardLog.Height - UITheme.Dpi(62)),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(242, 248, 245),
                ForeColor = UITheme.TextPrimary,
                Font = UITheme.FontMono,
                BorderStyle = BorderStyle.None
            };
            cardLog.Controls.Add(txtLogMonitor);
            pnl.Controls.Add(cardLog);

            UpdateEngineStatusDisplay();
            AppendLog("Scrcpy 现代化控制台已启动");
            return pnl;
        }

        #endregion

        #region ADB 与设备辅助功能逻辑

        private void RefreshDeviceList()
        {
            comboDevices.Items.Clear();
            if (string.IsNullOrEmpty(adbExePath))
            {
                lblHeroDeviceName.Text = "未找到 ADB 路径";
                lblHeroState.Text = "● ADB MISSING";
                lblHeroState.ForeColor = UITheme.DangerRed;
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(adbExePath, "devices")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                if (File.Exists(scrcpyExePath))
                    psi.WorkingDirectory = Path.GetDirectoryName(scrcpyExePath);

                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("List") || string.IsNullOrWhiteSpace(line))
                            continue;
                        string[] parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                            comboDevices.Items.Add(parts[0]);
                    }
                }

                if (comboDevices.Items.Count > 0)
                {
                    comboDevices.SelectedIndex = 0;
                    lblHeroDeviceName.Text = comboDevices.SelectedItem;
                    lblHeroState.Text = string.Format("● ONLINE · {0} 台设备在线", comboDevices.Items.Count);
                    lblHeroState.ForeColor = UITheme.AccentGreen;
                    ShowToast("已发现在线 Android 设备: " + comboDevices.SelectedItem, ToastKind.Success);
                }
                else
                {
                    lblHeroDeviceName.Text = "未连接设备";
                    lblHeroState.Text = "● STANDBY · 等待设备连接";
                    lblHeroState.ForeColor = UITheme.TextMuted;
                }
            }
            catch (Exception ex)
            {
                lblHeroState.Text = "● ADB ERROR";
                lblHeroState.ForeColor = UITheme.DangerRed;
                AppendLog("ADB 异常: " + ex.Message);
            }
        }

        private void BtnAdbTcpip_Click(object sender, EventArgs e)
        {
            AppendLog("正在下发 adb tcpip 5555...");
            RunAdbCommand("tcpip 5555", (outStr) =>
            {
                string msg = FirstLine(outStr);
                ShowToast(string.IsNullOrEmpty(msg) ? "TCP/IP 5555 端口已开启" : msg,
                          msg.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 ? ToastKind.Error : ToastKind.Success);
            });
        }

        private void BtnWirelessConnect_Click(object sender, EventArgs e)
        {
            string ip = txtWirelessIp.Text.Trim();
            if (string.IsNullOrEmpty(ip)) return;
            if (!ip.Contains(":")) ip += ":5555";

            AppendLog("正在无线连接至: " + ip);
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(adbExePath, "connect " + ip)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                if (File.Exists(scrcpyExePath))
                    psi.WorkingDirectory = Path.GetDirectoryName(scrcpyExePath);

                using (Process p = Process.Start(psi))
                {
                    string outStr = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    string msg = FirstLine(outStr);
                    bool ok = msg.IndexOf("connected", StringComparison.OrdinalIgnoreCase) >= 0
                           && msg.IndexOf("cannot", StringComparison.OrdinalIgnoreCase) < 0;
                    ShowToast(string.IsNullOrEmpty(msg) ? "无线连接完成" : msg, ok ? ToastKind.Success : ToastKind.Error);
                    RefreshDeviceList();
                }
            }
            catch (Exception ex)
            {
                ShowToast("连接失败: " + ex.Message, ToastKind.Error);
            }
        }

        private void SendAdbKeyEvent(int keyCode)
        {
            RunAdbShell("input keyevent " + keyCode);
            AppendLog("模拟按键 KeyCode: " + keyCode);
        }

        private void RotateDeviceScreen()
        {
            RunAdbShell("settings put system user_rotation 1");
            ShowToast("已请求设备旋转屏幕", ToastKind.Info);
        }

        private void RebootDevice()
        {
            if (MessageBox.Show("确定要重启连接的 Android 手机吗？", "确认重启",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                RunAdbCommand("reboot", (outStr) => ShowToast("已发送系统重启指令", ToastKind.Warning));
            }
        }

        private void RebootBootloader()
        {
            if (MessageBox.Show("确定要将手机重启至 Bootloader 模式吗？", "确认重启",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                RunAdbCommand("reboot bootloader", (outStr) => ShowToast("已发送重启至 Bootloader 指令", ToastKind.Warning));
            }
        }

        private void TakeDeviceScreenshot()
        {
            try
            {
                string devArg = (comboDevices.SelectedItem != null) ? string.Format("-s {0} ", comboDevices.SelectedItem) : "";
                string fileName = string.Format("screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                ShowToast("正在截屏...", ToastKind.Info);
                AppendLog("抓取屏幕截图：" + savePath);

                RunAdbCommand(devArg + "shell screencap -p /sdcard/scrcpy_screen.png", (s1) =>
                {
                    RunAdbCommand(devArg + string.Format("pull /sdcard/scrcpy_screen.png \"{0}\"", savePath), (s2) =>
                    {
                        RunAdbShell(devArg + "rm /sdcard/scrcpy_screen.png");
                        if (File.Exists(savePath))
                        {
                            ShowToast("截图保存成功: " + fileName, ToastKind.Success);
                        }
                        else
                        {
                            ShowToast("截图拉取失败", ToastKind.Error);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                ShowToast("截图失败: " + ex.Message, ToastKind.Error);
            }
        }

        private void RestartAdbServer()
        {
            ShowToast("正在重启 ADB 服务...", ToastKind.Info);
            AppendLog("执行 adb kill-server / start-server...");
            RunAdbCommand("kill-server", (s1) =>
            {
                RunAdbCommand("start-server", (s2) =>
                {
                    RefreshDeviceList();
                    ShowToast("ADB 服务已成功重启", ToastKind.Success);
                });
            });
        }

        private void RunAdbShell(string shellCommand)
        {
            string devArg = (comboDevices.SelectedItem != null) ? string.Format("-s {0} ", comboDevices.SelectedItem) : "";
            RunAdbCommand(devArg + "shell " + shellCommand, null);
        }

        private void RunAdbCommand(string arguments, Action<string> onComplete)
        {
            if (string.IsNullOrEmpty(adbExePath)) return;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(adbExePath, arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                if (File.Exists(scrcpyExePath))
                    psi.WorkingDirectory = Path.GetDirectoryName(scrcpyExePath);

                Process p = Process.Start(psi);
                p.EnableRaisingEvents = true;
                p.Exited += (s, e) =>
                {
                    string outStr = p.StandardOutput.ReadToEnd();
                    if (onComplete != null)
                        this.BeginInvoke(new Action(() => onComplete(outStr)));
                };
            }
            catch (Exception ex)
            {
                AppendLog("执行 ADB 异常: " + ex.Message);
            }
        }

        private static string FirstLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Length > 0 ? lines[0].Trim() : "";
        }

        #endregion

        #region 启动与终止投屏

        private void BtnLaunchPrimary_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(scrcpyExePath) || !File.Exists(scrcpyExePath))
            {
                ShowToast("未找到 scrcpy.exe，请在【引擎设置】中指定路径", ToastKind.Error);
                SelectNavTab(3);
                return;
            }

            if (scrcpyProcess != null && !scrcpyProcess.HasExited)
            {
                ShowToast("投屏引擎已在运行中", ToastKind.Info);
                return;
            }

            string args = "";
            if (comboDevices.SelectedItem != null)
                args += string.Format("-s {0} ", comboDevices.SelectedItem);

            // 分辨率
            if (comboResolution.SelectedIndex == 1) args += "--max-size 3840 ";
            else if (comboResolution.SelectedIndex == 2) args += "--max-size 2560 ";
            else if (comboResolution.SelectedIndex == 3) args += "--max-size 1080 ";
            else if (comboResolution.SelectedIndex == 4) args += "--max-size 720 ";

            // 刷新率
            if (comboFps.SelectedIndex == 1) args += "--max-fps 144 ";
            else if (comboFps.SelectedIndex == 2) args += "--max-fps 120 ";
            else if (comboFps.SelectedIndex == 3) args += "--max-fps 90 ";
            else if (comboFps.SelectedIndex == 4) args += "--max-fps 60 ";
            else if (comboFps.SelectedIndex == 5) args += "--max-fps 30 ";

            // 码率
            if (comboBitrate.SelectedIndex == 1) args += "--video-bit-rate 16M ";
            else if (comboBitrate.SelectedIndex == 2) args += "--video-bit-rate 32M ";
            else if (comboBitrate.SelectedIndex == 3) args += "--video-bit-rate 64M ";
            else if (comboBitrate.SelectedIndex == 4) args += "--video-bit-rate 4M ";

            // 编码器
            if (comboCodec.SelectedIndex == 1) args += "--video-codec=h265 ";
            else if (comboCodec.SelectedIndex == 2) args += "--video-codec=av1 ";
            else if (comboCodec.SelectedIndex == 3) args += "--video-codec=h264 ";

            // 音频协议与码率
            if (comboAudioCodec.SelectedIndex == 1) args += "--audio-codec=aac ";
            else if (comboAudioCodec.SelectedIndex == 2) args += "--audio-codec=raw ";

            if (comboAudioBitrate.SelectedIndex == 1) args += "--audio-bit-rate=192K ";
            else if (comboAudioBitrate.SelectedIndex == 2) args += "--audio-bit-rate=256K ";
            else if (comboAudioBitrate.SelectedIndex == 3) args += "--audio-bit-rate=64K ";

            // 缓冲区
            if (comboBuffer.SelectedIndex == 1) args += "--video-buffer=25 ";
            else if (comboBuffer.SelectedIndex == 2) args += "--video-buffer=50 ";
            else if (comboBuffer.SelectedIndex == 3) args += "--video-buffer=100 ";

            // 同步录制
            if (comboRecord.SelectedIndex == 1)
                args += string.Format("--record=\"record_{0}.mp4\" ", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            else if (comboRecord.SelectedIndex == 2)
                args += string.Format("--record=\"record_{0}.mkv\" ", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            // 特性开关
            if (togScreenOff.Checked) args += "--turn-screen-off ";
            if (togAlwaysOnTop.Checked) args += "--always-on-top ";
            if (togStayAwake.Checked) args += "--stay-awake ";
            if (togBorderless.Checked) args += "--window-borderless ";
            if (togTouches.Checked) args += "--show-touches ";
            if (!togAudio.Checked) args += "--no-audio ";

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(scrcpyExePath, args.Trim())
                {
                    WorkingDirectory = Path.GetDirectoryName(scrcpyExePath),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                AppendLog("启动投屏: scrcpy " + args.Trim());

                scrcpyProcess = Process.Start(psi);
                scrcpyProcess.OutputDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) AppendLog("[scrcpy] " + ev.Data); };
                scrcpyProcess.ErrorDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) AppendLog("[scrcpy] " + ev.Data); };
                scrcpyProcess.BeginOutputReadLine();
                scrcpyProcess.BeginErrorReadLine();
                scrcpyProcess.EnableRaisingEvents = true;

                scrcpyProcess.Exited += (s, ev) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        btnLaunchPrimary.Enabled = true;
                        btnStopPrimary.Enabled = false;
                        lblHeroState.Text = "● STANDBY · 投屏已结束";
                        lblHeroState.ForeColor = UITheme.TextMuted;
                        ShowToast("投屏已结束", ToastKind.Info);
                        scrcpyProcess = null;
                        RestoreMainWindow();
                    }));
                };

                btnLaunchPrimary.Enabled = false;
                btnStopPrimary.Enabled = true;
                lblHeroState.Text = "● RUNNING · 投屏渲染中 (零黑框)";
                lblHeroState.ForeColor = UITheme.AccentGreen;

                ShowToast("投屏已成功启动", ToastKind.Success);
                ScheduleAutoHide();
            }
            catch (Exception ex)
            {
                ShowToast("启动失败: " + ex.Message, ToastKind.Error);
            }
        }

        private void BtnStopPrimary_Click(object sender, EventArgs e)
        {
            if (scrcpyProcess != null && !scrcpyProcess.HasExited)
            {
                try
                {
                    scrcpyProcess.Kill();
                    ShowToast("已手动终止投屏", ToastKind.Info);
                }
                catch { }
            }
        }

        #region 投屏时自动隐藏主窗口 / 结束后自动弹回

        private Timer autoHideTimer;

        /// <summary>投屏启动后延迟隐藏主窗口，让投屏窗口拿到焦点</summary>
        private void ScheduleAutoHide()
        {
            if (autoHideTimer == null)
            {
                autoHideTimer = new Timer();
                autoHideTimer.Interval = 900;
                autoHideTimer.Tick += (s, e) =>
                {
                    autoHideTimer.Stop();
                    if (scrcpyProcess != null && !scrcpyProcess.HasExited)
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }
                };
            }
            autoHideTimer.Stop();
            autoHideTimer.Start();
        }

        /// <summary>投屏结束后把主窗口恢复并置于前台</summary>
        private void RestoreMainWindow()
        {
            if (autoHideTimer != null) autoHideTimer.Stop();
            try
            {
                if (!this.Visible) this.Show();
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.Activate();
                this.BringToFront();
            }
            catch { }
        }

        #endregion

        #endregion

        #region 可选悬浮导航栏控制

        private void ToggleNavBar()
        {
            if (navBar == null || navBar.IsDisposed)
                EnsureNavBarVisible();
            else
                CloseNavBar();
        }

        private void EnsureNavBarVisible()
        {
            if (navBar != null && !navBar.IsDisposed)
            {
                if (!navBar.Visible) navBar.Show();
                navBar.BringToFront();
                return;
            }

            navBar = new FloatingNavBar();
            navBar.GetSerial = delegate { return (comboDevices.SelectedItem == null) ? null : comboDevices.SelectedItem.ToString(); };
            navBar.RunShell = delegate(string cmd) { RunAdbShell(cmd); };
            navBar.OnScreenshot = delegate { TakeDeviceScreenshot(); };
            navBar.OnLog = delegate(string msg) { AppendLog(msg); };
            navBar.Show();
            ShowToast("悬浮导航栏已启动", ToastKind.Info);
        }

        private void CloseNavBar()
        {
            if (navBar != null && !navBar.IsDisposed)
            {
                FloatingNavBar nb = navBar;
                navBar = null;
                nb.Close();
                nb.Dispose();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (navBar != null && !navBar.IsDisposed)
            {
                navBar.Close();
                navBar.Dispose();
                navBar = null;
            }
            base.OnFormClosing(e);
        }

        #endregion
    }

    #endregion

    #region 可选独立悬浮导航栏 (FloatingNavBar)

    public class NavIconButton : Control
    {
        public enum NavIcon { Back, Home, Recents, VolumeDown, VolumeUp, Power, Camera, Close }

        private bool _isHovered;
        private bool _isPressed;

        public NavIcon IconType { get; set; }
        public Color IconColor { get; set; }

        public NavIconButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(UITheme.Dpi(40), UITheme.Dpi(40));
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            IconColor = UITheme.TextPrimary;
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; _isPressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _isPressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (_isHovered || _isPressed)
                {
                    Color bg = _isPressed ? Color.FromArgb(200, 222, 214) : Color.FromArgb(220, 236, 228);
                    using (GraphicsPath path = Geometry.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), UITheme.Dpi(8)))
                    using (SolidBrush brush = new SolidBrush(bg))
                        g.FillPath(brush, path);
                }

                int iconSz = UITheme.Dpi(20);
                Rectangle r = new Rectangle((Width - iconSz) / 2, (Height - iconSz) / 2, iconSz, iconSz);
                Color color = _isHovered ? UITheme.AccentGreen : IconColor;
                DrawIcon(g, IconType, r, color);
            }
            catch { }
        }

        private void DrawIcon(Graphics g, NavIcon icon, Rectangle r, Color color)
        {
            int x = r.X;
            int y = r.Y;

            using (Pen pen = new Pen(color, 1.8f))
            using (SolidBrush brush = new SolidBrush(color))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                switch (icon)
                {
                    case NavIcon.Back:
                        g.DrawLine(pen, x + 12, y + 4, x + 6, y + 10);
                        g.DrawLine(pen, x + 6, y + 10, x + 12, y + 16);
                        g.DrawLine(pen, x + 6, y + 10, x + 16, y + 10);
                        break;
                    case NavIcon.Home:
                        g.DrawLines(pen, new Point[] {
                            new Point(x + 2, y + 10), new Point(x + 10, y + 2), new Point(x + 18, y + 10)
                        });
                        g.DrawLines(pen, new Point[] {
                            new Point(x + 4, y + 9), new Point(x + 4, y + 18),
                            new Point(x + 16, y + 18), new Point(x + 16, y + 9)
                        });
                        break;
                    case NavIcon.Recents:
                        g.DrawRectangle(pen, x + 2, y + 6, 10, 11);
                        g.DrawRectangle(pen, x + 7, y + 3, 10, 11);
                        break;
                    case NavIcon.VolumeDown:
                    case NavIcon.VolumeUp:
                        g.FillPolygon(brush, new Point[] {
                            new Point(x + 2, y + 8), new Point(x + 6, y + 8),
                            new Point(x + 10, y + 4), new Point(x + 10, y + 16),
                            new Point(x + 6, y + 12), new Point(x + 2, y + 12)
                        });
                        g.DrawLine(pen, x + 12, y + 10, x + 17, y + 10);
                        if (icon == NavIcon.VolumeUp)
                            g.DrawLine(pen, x + 14.5f, y + 7.5f, x + 14.5f, y + 12.5f);
                        break;
                    case NavIcon.Power:
                        g.DrawArc(pen, x + 4, y + 4, 12, 12, -60, 300);
                        g.DrawLine(pen, x + 10, y + 2, x + 10, y + 9);
                        break;
                    case NavIcon.Camera:
                        using (GraphicsPath body = Geometry.RoundedRect(new Rectangle(x + 1, y + 5, 17, 11), 2))
                            g.DrawPath(pen, body);
                        g.DrawEllipse(pen, x + 7, y + 7, 5, 5);
                        break;
                    case NavIcon.Close:
                        g.DrawLine(pen, x + 5, y + 5, x + 15, y + 15);
                        g.DrawLine(pen, x + 15, y + 5, x + 5, y + 15);
                        break;
                }
            }
        }
    }

    public class FloatingNavBar : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        public Func<string> GetSerial;
        public Action<string> RunShell;
        public Action OnScreenshot;
        public Action<string> OnLog;

        private ToolTip _tip;

        public FloatingNavBar()
        {
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            BackColor = UITheme.CardBg;
            DoubleBuffered = true;
            Size = new Size(UITheme.Dpi(380), UITheme.Dpi(54));
            Text = "Scrcpy 悬浮导航栏";

            _tip = new ToolTip();
            BuildBar();
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            using (GraphicsPath path = Geometry.RoundedRect(new Rectangle(0, 0, Width, Height), UITheme.Dpi(10)))
            {
                Region old = Region;
                Region = new Region(path);
                if (old != null) old.Dispose();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath path = Geometry.RoundedRect(rect, UITheme.Dpi(10)))
                using (SolidBrush brush = new SolidBrush(UITheme.CardBg))
                using (Pen pen = new Pen(UITheme.CardBorder, 1.2f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }
            }
            catch { }
        }

        private void BuildBar()
        {
            int x = UITheme.Dpi(10);
            int step = UITheme.Dpi(44);

            AddButton(NavIconButton.NavIcon.Back, x, "返回", delegate { SendKey(4); });
            x += step;
            AddButton(NavIconButton.NavIcon.Home, x, "主页", delegate { SendKey(3); });
            x += step;
            AddButton(NavIconButton.NavIcon.Recents, x, "最近任务", delegate { SendKey(187); });
            x += step;
            AddButton(NavIconButton.NavIcon.VolumeDown, x, "音量-", delegate { SendKey(25); });
            x += step;
            AddButton(NavIconButton.NavIcon.VolumeUp, x, "音量+", delegate { SendKey(24); });
            x += step;
            AddButton(NavIconButton.NavIcon.Power, x, "电源键", delegate { SendKey(26); });
            x += step;
            AddButton(NavIconButton.NavIcon.Camera, x, "截屏", delegate { if (OnScreenshot != null) OnScreenshot(); });

            Panel sep = new Panel { Location = new Point(x + step - UITheme.Dpi(10), UITheme.Dpi(12)), Size = new Size(1, UITheme.Dpi(30)), BackColor = UITheme.InnerBorder };
            Controls.Add(sep);

            NavIconButton btnClose = new NavIconButton
            {
                IconType = NavIconButton.NavIcon.Close,
                Location = new Point(x + step - UITheme.Dpi(2), UITheme.Dpi(7)),
                Size = new Size(UITheme.Dpi(38), UITheme.Dpi(38)),
                IconColor = UITheme.TextSecondary
            };
            btnClose.Click += (s, e) => Close();
            _tip.SetToolTip(btnClose, "关闭导航栏");
            Controls.Add(btnClose);

            MouseDown += DragWindow;
        }

        private void AddButton(NavIconButton.NavIcon icon, int x, string tip, EventHandler onClick)
        {
            NavIconButton btn = new NavIconButton
            {
                IconType = icon,
                Location = new Point(x, UITheme.Dpi(7))
            };
            btn.Click += onClick;
            _tip.SetToolTip(btn, tip);
            Controls.Add(btn);
        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void SendKey(int keyCode)
        {
            if (RunShell == null) return;
            RunShell("input keyevent " + keyCode);
            if (OnLog != null) OnLog("[导航栏] KeyCode " + keyCode);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(wa.Left + (wa.Width - Width) / 2, wa.Bottom - Height - UITheme.Dpi(50));
        }
    }

    #endregion

    static class Program
    {
        [DllImport("user32.dll")]
        public static extern bool SetProcessDpiAwarenessContext(IntPtr ctx);

        [DllImport("user32.dll")]
        public static extern uint GetDpiForSystem();

        [STAThread]
        static void Main()
        {
            float scale = 1.0f;
            try
            {
                // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4
                SetProcessDpiAwarenessContext((IntPtr)(-4));
                uint sysDpi = GetDpiForSystem();
                if (sysDpi > 0)
                    scale = sysDpi / 96.0f;
            }
            catch
            {
                scale = 1.0f;
            }

            UITheme.InitDpi(scale);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
