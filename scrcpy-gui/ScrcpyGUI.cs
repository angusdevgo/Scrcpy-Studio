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
    #region 全局设计规范 (Linear Pro Studio & Cyber Dark Aesthetic)
    public static class UITheme
    {
        // 核心背景色系 (沉浸式深空冷灰)
        public static readonly Color WindowBg       = Color.FromArgb(13, 15, 20);   // #0D0F14 主工作区背景
        public static readonly Color SidebarBg      = Color.FromArgb(9, 10, 14);    // #090A0E 沉浸侧边栏
        public static readonly Color TitleBarBg     = Color.FromArgb(9, 10, 14);    // 标题栏背景
        public static readonly Color CardBg         = Color.FromArgb(19, 22, 29);   // #13161D 大气浮动卡片
        public static readonly Color CardHoverBg    = Color.FromArgb(25, 29, 38);   // 卡片轻悬停
        public static readonly Color CardBorder     = Color.FromArgb(32, 37, 48);   // #202530 极细微光描边
        public static readonly Color InnerBorder    = Color.FromArgb(24, 27, 36);   // 分割线
        
        // 控件输入与小区块
        public static readonly Color InputBg        = Color.FromArgb(14, 16, 22);   // #0E1016 输入框底色
        public static readonly Color InputBorder    = Color.FromArgb(38, 44, 58);   // #262C3A 边框
        public static readonly Color InputFocus     = Color.FromArgb(0, 225, 170);  // 聚焦高光

        // 核心品牌点缀（赛博极光青 / 霓虹绿）
        public static readonly Color Accent         = Color.FromArgb(0, 225, 170);  // #00E1AA 极光青
        public static readonly Color AccentHover    = Color.FromArgb(40, 245, 195);
        public static readonly Color AccentActive   = Color.FromArgb(0, 190, 140);
        public static readonly Color PrimaryText    = Color.FromArgb(8, 16, 12);

        // 状态功能色
        public static readonly Color Success        = Color.FromArgb(16, 217, 137); // #10D989
        public static readonly Color Danger         = Color.FromArgb(244, 63, 94);  // #F43F5E 霓虹玫瑰红
        public static readonly Color DangerHover    = Color.FromArgb(251, 113, 133);
        public static readonly Color Warning        = Color.FromArgb(245, 158, 11);

        // 文字排版
        public static readonly Color TextWhite      = Color.FromArgb(255, 255, 255);
        public static readonly Color TextPrimary    = Color.FromArgb(240, 243, 248);
        public static readonly Color TextSecondary  = Color.FromArgb(150, 160, 175);
        public static readonly Color TextMuted      = Color.FromArgb(100, 110, 128);
        public static readonly Color TextDisabled   = Color.FromArgb(65, 72, 85);

        // 字体层次规范
        public static Font FontPageTitle   = new Font("Segoe UI", 15F, FontStyle.Bold);
        public static Font FontPageSub     = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);
        public static Font FontCardHeader  = new Font("Microsoft YaHei UI", 11.5F, FontStyle.Bold);
        public static Font FontItemTitle   = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        public static Font FontItemSub     = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular);
        public static Font FontBody        = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);
        public static Font FontBodyBold    = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        public static Font FontMono        = new Font("Consolas", 9.5F, FontStyle.Regular);
        public static Font FontSmall       = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular);
    }
    #endregion

    #region 高清自绘组件库

    // 1. 沉浸圆角卡片 StudioCard
    public class StudioCard : Panel
    {
        public int Radius { get; set; }
        public Color BorderColor { get; set; }

        public StudioCard()
        {
            Radius = 10;
            BorderColor = UITheme.CardBorder;
            BackColor = UITheme.CardBg;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath path = Geometry.RoundedRect(rect, Radius))
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

    // 2. 自绘暗黑下拉框 StudioComboBox
    public class StudioComboBox : Control
    {
        private List<string> _items = new List<string>();
        private int _selectedIndex = -1;
        private bool _isHovered = false;
        private bool _isDropped = false;
        private Form _dropForm = null;
        private ListBox _dropListBox = null;

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

        public StudioComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
            Size = new Size(200, 36);
            Cursor = Cursors.Hand;
            Font = UITheme.FontBody;
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_isDropped)
                CloseDropdown();
            else
                ShowDropdown();
        }

        private void ShowDropdown()
        {
            if (_items.Count == 0) return;

            Point screenPt = this.PointToScreen(new Point(0, this.Height + 2));
            int itemHeight = 34;
            int dropHeight = Math.Min(_items.Count * itemHeight + 6, 260);

            _dropForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                TopMost = true,
                BackColor = UITheme.CardBg
            };

            _dropListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.CardBg,
                ForeColor = UITheme.TextPrimary,
                Font = this.Font,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = itemHeight,
                IntegralHeight = false
            };

            foreach (var item in _items)
                _dropListBox.Items.Add(item);

            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                _dropListBox.SelectedIndex = _selectedIndex;

            _dropListBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= _items.Count) return;
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool isSel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color bg = isSel ? Color.FromArgb(28, 34, 46) : UITheme.CardBg;
                Color fg = isSel ? UITheme.Accent : UITheme.TextPrimary;

                using (SolidBrush b = new SolidBrush(bg))
                    g.FillRectangle(b, e.Bounds);

                if (isSel)
                {
                    using (SolidBrush barB = new SolidBrush(UITheme.Accent))
                        g.FillRectangle(barB, e.Bounds.X + 2, e.Bounds.Y + 4, 3, e.Bounds.Height - 8);
                }

                Rectangle textRect = new Rectangle(e.Bounds.X + 14, e.Bounds.Y, e.Bounds.Width - 18, e.Bounds.Height);
                TextRenderer.DrawText(g, _items[e.Index], this.Font, textRect, fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
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

            _dropForm.Deactivate += (s, e) =>
            {
                this.BeginInvoke(new Action(() => CloseDropdown()));
            };

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
                Color border = (_isDropped || _isHovered) ? UITheme.Accent : UITheme.InputBorder;
                Color bg = _isHovered ? Color.FromArgb(20, 24, 32) : UITheme.InputBg;

                using (GraphicsPath path = Geometry.RoundedRect(rect, 6))
                using (SolidBrush brush = new SolidBrush(bg))
                using (Pen pen = new Pen(border, 1.2f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

                string displayText = (_selectedIndex >= 0 && _selectedIndex < _items.Count) ? _items[_selectedIndex] : "请选择...";
                Rectangle textRect = new Rectangle(14, 0, Width - 36, Height);
                TextRenderer.DrawText(g, displayText, Font, textRect, UITheme.TextPrimary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);

                int arrowX = Width - 20;
                int arrowY = Height / 2 - 2;
                using (Pen arrowPen = new Pen(_isHovered ? UITheme.Accent : UITheme.TextSecondary, 1.6f))
                {
                    arrowPen.StartCap = LineCap.Round;
                    arrowPen.EndCap = LineCap.Round;
                    if (_isDropped)
                    {
                        g.DrawLine(arrowPen, arrowX, arrowY + 4, arrowX + 4, arrowY);
                        g.DrawLine(arrowPen, arrowX + 4, arrowY, arrowX + 8, arrowY + 4);
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

    // 3. 现代交互按钮 StudioButton
    public class StudioButton : Control
    {
        public enum ButtonStyle { Accent, Danger, Secondary, Ghost, CloseGhost }

        private bool _isHovered = false;
        private bool _isPressed = false;

        public ButtonStyle Style { get; set; }
        public int Radius { get; set; }

        public StudioButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = UITheme.FontBodyBold;
            ForeColor = UITheme.TextWhite;
            Radius = 6;
            Style = ButtonStyle.Accent;
            Size = new Size(130, 38);
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

                Color bg = UITheme.Accent;
                Color fg = UITheme.PrimaryText;
                Color border = Color.Transparent;

                if (!Enabled)
                {
                    bg = Color.FromArgb(24, 28, 36);
                    fg = UITheme.TextDisabled;
                    border = Color.FromArgb(34, 38, 48);
                }
                else
                {
                    switch (Style)
                    {
                        case ButtonStyle.Accent:
                            bg = _isPressed ? UITheme.AccentActive : (_isHovered ? UITheme.AccentHover : UITheme.Accent);
                            fg = Color.FromArgb(8, 16, 12);
                            break;
                        case ButtonStyle.Danger:
                            bg = _isPressed ? Color.FromArgb(200, 30, 60) : (_isHovered ? UITheme.DangerHover : UITheme.Danger);
                            fg = Color.White;
                            break;
                        case ButtonStyle.Secondary:
                            bg = _isPressed ? Color.FromArgb(28, 32, 42) : (_isHovered ? Color.FromArgb(24, 28, 38) : Color.FromArgb(16, 19, 26));
                            border = _isHovered ? UITheme.Accent : UITheme.InputBorder;
                            fg = _isHovered ? UITheme.TextWhite : UITheme.TextPrimary;
                            break;
                        case ButtonStyle.Ghost:
                            bg = _isPressed ? Color.FromArgb(30, 36, 48) : (_isHovered ? Color.FromArgb(22, 26, 34) : Color.Transparent);
                            fg = _isHovered ? UITheme.TextWhite : UITheme.TextSecondary;
                            break;
                        case ButtonStyle.CloseGhost:
                            bg = _isPressed ? Color.FromArgb(180, 10, 20) : (_isHovered ? Color.FromArgb(232, 17, 35) : Color.Transparent);
                            fg = _isHovered ? Color.White : UITheme.TextSecondary;
                            break;
                    }
                }

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath path = Geometry.RoundedRect(rect, Radius))
                {
                    using (SolidBrush brush = new SolidBrush(bg))
                        g.FillPath(brush, path);

                    if (border != Color.Transparent)
                    {
                        using (Pen pen = new Pen(border, 1.2f))
                            g.DrawPath(pen, path);
                    }
                }

                TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            catch { }
        }
    }

    // 4. 自绘极简胶囊开关 StudioToggle
    public class StudioToggle : Control
    {
        private bool _checked = false;
        public bool Checked
        {
            get { return _checked; }
            set { if (_checked != value) { _checked = value; Invalidate(); if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty); } }
        }

        public event EventHandler CheckedChanged;

        public StudioToggle()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(44, 24);
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
                Color bg = _checked ? UITheme.Accent : Color.FromArgb(28, 33, 44);
                Color border = _checked ? UITheme.AccentHover : Color.FromArgb(46, 54, 70);

                using (GraphicsPath path = Geometry.Pill(rect))
                using (SolidBrush brush = new SolidBrush(bg))
                using (Pen pen = new Pen(border, 1.2f))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

                int thumbSize = Height - 6;
                int thumbX = _checked ? (Width - thumbSize - 3) : 3;
                Rectangle thumbRect = new Rectangle(thumbX, 3, thumbSize, thumbSize);

                Color thumbColor = _checked ? Color.FromArgb(8, 16, 12) : Color.FromArgb(195, 202, 215);
                using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
                {
                    g.FillEllipse(thumbBrush, thumbRect);
                }
            }
            catch { }
        }
    }

    // 5. 现代化自绘输入框 StudioTextBox (解决 WinForms 原生输入框丑陋与高度固定问题)
    public class StudioTextBox : Panel
    {
        private TextBox _inner;
        private bool _isFocused = false;

        public string Text
        {
            get { return _inner.Text; }
            set { _inner.Text = value; }
        }

        public TextBox InnerTextBox { get { return _inner; } }

        public StudioTextBox()
        {
            Size = new Size(200, 36);
            BackColor = UITheme.InputBg;
            DoubleBuffered = true;

            _inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = UITheme.InputBg,
                ForeColor = UITheme.TextWhite,
                Font = UITheme.FontMono,
                Location = new Point(12, 9),
                Width = Width - 24
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
                _inner.Width = Width - 24;
                _inner.Location = new Point(12, (Height - _inner.PreferredHeight) / 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color border = _isFocused ? UITheme.Accent : UITheme.InputBorder;

            using (GraphicsPath path = Geometry.RoundedRect(rect, 6))
            using (SolidBrush brush = new SolidBrush(UITheme.InputBg))
            using (Pen pen = new Pen(border, _isFocused ? 1.5f : 1.2f))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }
    }

    // 6. 侧边栏导航 Tab
    public class StudioNavTab : Control
    {
        public enum TabIconType { Device, Video, Tools, Engine }

        private bool _isSelected = false;
        private bool _isHovered = false;

        public string TabName { get; set; }
        public TabIconType IconType { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; Invalidate(); }
        }

        public StudioNavTab(TabIconType iconType, string name)
        {
            IconType = iconType;
            TabName = name;
            Size = new Size(206, 46);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
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

                Color bg = Color.Transparent;
                Color fg = UITheme.TextSecondary;
                Color iconColor = UITheme.TextSecondary;

                if (_isSelected)
                {
                    bg = Color.FromArgb(20, 24, 34);
                    fg = UITheme.TextWhite;
                    iconColor = UITheme.Accent;
                }
                else if (_isHovered)
                {
                    bg = Color.FromArgb(14, 17, 24);
                    fg = UITheme.TextPrimary;
                    iconColor = UITheme.TextPrimary;
                }

                Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath path = Geometry.RoundedRect(rect, 8))
                using (SolidBrush brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }

                if (_isSelected)
                {
                    using (GraphicsPath barPath = Geometry.RoundedRect(new Rectangle(3, 10, 4, Height - 20), 1))
                    using (SolidBrush barBrush = new SolidBrush(UITheme.Accent))
                    {
                        g.FillPath(barBrush, barPath);
                    }
                }

                DrawVectorIcon(g, IconType, 18, 14, iconColor);

                Rectangle textRect = new Rectangle(52, 0, Width - 56, Height);
                TextRenderer.DrawText(g, TabName, _isSelected ? UITheme.FontBodyBold : UITheme.FontBody, textRect, fg, 
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            catch { }
        }

        private void DrawVectorIcon(Graphics g, TabIconType type, int x, int y, Color color)
        {
            using (Pen pen = new Pen(color, 1.8f))
            using (SolidBrush brush = new SolidBrush(color))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                switch (type)
                {
                    case TabIconType.Device:
                        using (GraphicsPath p = Geometry.RoundedRect(new Rectangle(x + 2, y, 12, 17), 2))
                            g.DrawPath(pen, p);
                        g.FillEllipse(brush, x + 7f, y + 13.5f, 2, 2);
                        break;

                    case TabIconType.Video:
                        using (GraphicsPath p = Geometry.RoundedRect(new Rectangle(x, y + 1, 18, 12), 2))
                            g.DrawPath(pen, p);
                        g.DrawLine(pen, x + 5, y + 17, x + 13, y + 17);
                        g.DrawLine(pen, x + 9, y + 13, x + 9, y + 17);
                        break;

                    case TabIconType.Tools:
                        using (GraphicsPath p = Geometry.RoundedRect(new Rectangle(x + 1, y + 4, 16, 12), 2))
                            g.DrawPath(pen, p);
                        using (GraphicsPath h = Geometry.RoundedRect(new Rectangle(x + 6, y + 1, 6, 4), 1))
                            g.DrawPath(pen, h);
                        break;

                    case TabIconType.Engine:
                        g.DrawEllipse(pen, x + 2, y + 2, 14, 14);
                        g.FillEllipse(brush, x + 7.5f, y + 7.5f, 3, 3);
                        g.DrawLine(pen, x + 9, y, x + 9, y + 3);
                        g.DrawLine(pen, x + 9, y + 15, x + 9, y + 18);
                        g.DrawLine(pen, x, y + 9, x + 3, y + 9);
                        g.DrawLine(pen, x + 15, y + 9, x + 18, y + 9);
                        break;
                }
            }
        }
    }

    public static class Geometry
    {
        public static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            if (radius < 1) radius = 1;
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
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
            path.AddArc(rect.X, rect.Y, d, d, 90, 180);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 180);
            path.CloseFigure();
            return path;
        }
    }

    #endregion

    #region 主窗体 (1180×740 旗舰级宽屏 Studio)

    public class MainForm : Form
    {
        private string scrcpyExePath = "";
        private string adbExePath = "";
        private Process scrcpyProcess = null;

        private Panel customTitleBar;
        private Panel sidebarPanel;
        private Panel contentWrapper;
        private List<StudioNavTab> navTabs = new List<StudioNavTab>();

        // 4 大功能主分页
        private Panel pageDashboard;
        private Panel pageDisplay;
        private Panel pageTools;
        private Panel pageSettings;

        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;

        // Dashboard 控件
        private Label lblDeviceBadge;
        private Label lblDeviceSerial;
        private Label lblDeviceSubInfo;
        private StudioComboBox comboDevices;
        private StudioButton btnRefreshDev;
        private StudioTextBox txtWirelessIp;
        private StudioButton btnWirelessConnect;
        private StudioButton btnAdbTcpip;

        private StudioToggle togScreenOff;
        private StudioToggle togAlwaysOnTop;
        private StudioToggle togStayAwake;
        private StudioToggle togAudio;
        private StudioToggle togBorderless;
        private StudioToggle togTouches;

        private StudioButton btnLaunchPrimary;
        private StudioButton btnStopPrimary;

        // Display 控件
        private StudioComboBox comboResolution;
        private StudioComboBox comboFps;
        private StudioComboBox comboBitrate;
        private StudioComboBox comboCodec;
        private StudioComboBox comboAudioCodec;
        private StudioComboBox comboAudioBitrate;
        private StudioComboBox comboBuffer;
        private StudioComboBox comboRecord;

        // Settings 控件
        private Label lblScrcpyPathVal;
        private Label lblAdbPathVal;
        private Label lblScrcpyStatus;
        private Label lblAdbStatus;
        private TextBox txtLogMonitor;

        // 底部全局状态
        private Label lblStatusDot;
        private Label lblGlobalStatus;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public MainForm()
        {
            InitWindowArchitecture();
            LoadApplicationIcon();
            DetectScrcpyEngine();
            BuildCustomTitleBar();
            BuildSidebar();
            BuildContentPages();
            SelectNavTab(0);
            RefreshDeviceList();
        }

        private void InitWindowArchitecture()
        {
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(1180, 740);
            MinimumSize = new Size(1180, 740);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UITheme.WindowBg;
            Font = UITheme.FontBody;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 绘制 1px 极细深色外边框，保证在深色/浅色壁纸下均有立体感
            using (Pen pen = new Pen(UITheme.CardBorder, 1f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
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

            string[] candidates = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scrcpy.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\scrcpy.exe"),
                @"D:\Data\Scrcpy\scrcpy.exe",
                @"C:\Data\Scrcpy\scrcpy.exe",
                @"C:\scrcpy\scrcpy.exe"
            };

            foreach (string p in candidates)
            {
                if (File.Exists(p)) { SetCorePath(Path.GetFullPath(p)); return; }
            }

            // 找不到则保留默认相对路径
            SetCorePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scrcpy.exe"));
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

            bool scrcpyOk = File.Exists(scrcpyExePath);
            bool adbOk = File.Exists(adbExePath) || adbExePath == "adb";

            if (lblScrcpyStatus != null)
            {
                lblScrcpyStatus.Text = scrcpyOk ? "● 引擎就绪" : "● 核心缺失";
                lblScrcpyStatus.ForeColor = scrcpyOk ? UITheme.Success : UITheme.Danger;
            }

            if (lblAdbStatus != null)
            {
                lblAdbStatus.Text = adbOk ? "● 服务就绪" : "● 未找到 ADB";
                lblAdbStatus.ForeColor = adbOk ? UITheme.Success : UITheme.Warning;
            }
        }

        private void BuildCustomTitleBar()
        {
            customTitleBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1180, 42),
                BackColor = UITheme.TitleBarBg
            };
            customTitleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            Label title = new Label
            {
                Text = "⚡ Scrcpy Studio · 极光现代化控制工作台",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextSecondary,
                BackColor = UITheme.TitleBarBg,
                Location = new Point(16, 12),
                AutoSize = true
            };
            title.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            StudioButton btnMin = new StudioButton
            {
                Text = "—",
                Style = StudioButton.ButtonStyle.Ghost,
                Location = new Point(1088, 4),
                Size = new Size(42, 34),
                Radius = 4
            };
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            StudioButton btnClose = new StudioButton
            {
                Text = "✕",
                Style = StudioButton.ButtonStyle.CloseGhost,
                Location = new Point(1132, 4),
                Size = new Size(42, 34),
                Radius = 4
            };
            btnClose.Click += (s, e) => Application.Exit();

            customTitleBar.Controls.AddRange(new Control[] { title, btnMin, btnClose });
            Controls.Add(customTitleBar);
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel
            {
                Location = new Point(0, 42),
                Size = new Size(230, 698),
                BackColor = UITheme.SidebarBg
            };

            Panel brand = new Panel { Location = new Point(16, 16), Size = new Size(198, 54), BackColor = UITheme.SidebarBg };
            Label lblLogo = new Label
            {
                Text = "SCRCPY PRO",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.SidebarBg,
                Location = new Point(2, 2),
                AutoSize = true
            };
            Label lblVer = new Label
            {
                Text = "STUDIO ENTERPRISE EDITION",
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                BackColor = UITheme.SidebarBg,
                Location = new Point(3, 28),
                AutoSize = true
            };
            brand.Controls.AddRange(new Control[] { lblLogo, lblVer });
            sidebarPanel.Controls.Add(brand);

            Panel line = new Panel { Location = new Point(16, 78), Size = new Size(198, 1), BackColor = UITheme.InnerBorder };
            sidebarPanel.Controls.Add(line);

            int startY = 92;
            StudioNavTab.TabIconType[] icons = new StudioNavTab.TabIconType[] {
                StudioNavTab.TabIconType.Device,
                StudioNavTab.TabIconType.Video,
                StudioNavTab.TabIconType.Tools,
                StudioNavTab.TabIconType.Engine
            };
            string[] names = new string[] { "投屏工作台", "画质与性能", "实用工具箱", "引擎与设置" };

            for (int i = 0; i < 4; i++)
            {
                int index = i;
                StudioNavTab tab = new StudioNavTab(icons[i], names[i])
                {
                    Location = new Point(12, startY),
                    Width = 206,
                    Height = 46
                };
                tab.Click += (s, e) => SelectNavTab(index);
                sidebarPanel.Controls.Add(tab);
                navTabs.Add(tab);
                startY += 52;
            }

            Panel bottomStatus = new Panel { Location = new Point(16, 642), Size = new Size(198, 40), BackColor = UITheme.SidebarBg };
            lblStatusDot = new Label
            {
                Text = "●",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = UITheme.Success,
                BackColor = UITheme.SidebarBg,
                Location = new Point(4, 10),
                AutoSize = true
            };
            lblGlobalStatus = new Label
            {
                Text = "工作台就绪",
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextSecondary,
                BackColor = UITheme.SidebarBg,
                Location = new Point(20, 10),
                AutoSize = true
            };
            bottomStatus.Controls.AddRange(new Control[] { lblStatusDot, lblGlobalStatus });
            sidebarPanel.Controls.Add(bottomStatus);

            Controls.Add(sidebarPanel);
        }

        private void BuildContentPages()
        {
            contentWrapper = new Panel
            {
                Location = new Point(230, 42),
                Size = new Size(950, 698),
                BackColor = UITheme.WindowBg
            };

            Panel headerPnl = new Panel { Location = new Point(32, 14), Size = new Size(886, 54), BackColor = UITheme.WindowBg };
            lblHeaderTitle = new Label
            {
                Text = "投屏控制工作台",
                Font = UITheme.FontPageTitle,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.WindowBg,
                Location = new Point(0, 0),
                AutoSize = true
            };
            lblHeaderSubtitle = new Label
            {
                Text = "实时监控已连接的 Android 调试设备并启动零延迟超清投屏",
                Font = UITheme.FontPageSub,
                ForeColor = UITheme.TextSecondary,
                BackColor = UITheme.WindowBg,
                Location = new Point(2, 30),
                AutoSize = true
            };
            headerPnl.Controls.AddRange(new Control[] { lblHeaderTitle, lblHeaderSubtitle });
            contentWrapper.Controls.Add(headerPnl);

            pageDashboard = CreatePageDashboard();
            pageDisplay = CreatePageDisplay();
            pageTools = CreatePageTools();
            pageSettings = CreatePageSettings();

            contentWrapper.Controls.AddRange(new Control[] { pageDashboard, pageDisplay, pageTools, pageSettings });
            Controls.Add(contentWrapper);
        }

        private void SelectNavTab(int index)
        {
            for (int i = 0; i < navTabs.Count; i++)
                navTabs[i].IsSelected = (i == index);

            pageDashboard.Visible = (index == 0);
            pageDisplay.Visible = (index == 1);
            pageTools.Visible = (index == 2);
            pageSettings.Visible = (index == 3);

            string[] titles = { "投屏控制工作台", "画质与性能调校", "实用辅助工具箱", "引擎与环境配置" };
            string[] subs = {
                "实时监控已连接的 Android 调试设备并启动零延迟超清投屏",
                "自定义最高分辨率、极限 120 FPS 帧率、硬件编解码与多媒体流控",
                "快捷触发手机物理按键、实时截屏、剪贴板文本同步与系统调试",
                "管理底层 scrcpy.exe 核心路径、ADB 服务守护状态与终端实时日志"
            };

            lblHeaderTitle.Text = titles[index];
            lblHeaderSubtitle.Text = subs[index];
        }

        #region Page 1: 投屏工作台 Dashboard

        private Panel CreatePageDashboard()
        {
            Panel pnl = new Panel { Location = new Point(32, 74), Size = new Size(886, 610), BackColor = UITheme.WindowBg };

            // 1. 设备连接状态卡片
            StudioCard cardDev = new StudioCard { Location = new Point(0, 0), Size = new Size(886, 126) };
            
            lblDeviceBadge = new Label
            {
                Text = "● 状态检测",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.Accent,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            lblDeviceSerial = new Label
            {
                Text = "正在扫描设备...",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 40),
                AutoSize = true
            };
            lblDeviceSubInfo = new Label
            {
                Text = "支持 USB 线连多设备切换及 TCP/IP 无线配对",
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextMuted,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 80),
                AutoSize = true
            };

            comboDevices = new StudioComboBox
            {
                Location = new Point(350, 22),
                Width = 360
            };

            btnRefreshDev = new StudioButton
            {
                Text = "刷新设备",
                Location = new Point(724, 21),
                Size = new Size(140, 38),
                Style = StudioButton.ButtonStyle.Secondary
            };
            btnRefreshDev.Click += (s, e) => RefreshDeviceList();

            Label lblIp = new Label
            {
                Text = "无线 IP :",
                Location = new Point(350, 78),
                AutoSize = true,
                ForeColor = UITheme.TextSecondary,
                BackColor = UITheme.CardBg,
                Font = UITheme.FontBody
            };

            txtWirelessIp = new StudioTextBox
            {
                Location = new Point(416, 73),
                Size = new Size(210, 38)
            };
            txtWirelessIp.Text = "192.168.1.";

            btnAdbTcpip = new StudioButton
            {
                Text = "开启 TCP/IP",
                Location = new Point(634, 73),
                Size = new Size(110, 38),
                Style = StudioButton.ButtonStyle.Secondary
            };
            btnAdbTcpip.Click += BtnAdbTcpip_Click;

            btnWirelessConnect = new StudioButton
            {
                Text = "无线连接",
                Location = new Point(752, 73),
                Size = new Size(112, 38),
                Style = StudioButton.ButtonStyle.Accent
            };
            btnWirelessConnect.Click += BtnWirelessConnect_Click;

            cardDev.Controls.AddRange(new Control[] { lblDeviceBadge, lblDeviceSerial, lblDeviceSubInfo, comboDevices, btnRefreshDev, lblIp, txtWirelessIp, btnAdbTcpip, btnWirelessConnect });
            pnl.Controls.Add(cardDev);

            // 2. 快捷特性开关卡片 (双列优雅自排布)
            StudioCard cardSw = new StudioCard { Location = new Point(0, 138), Size = new Size(886, 230) };
            Label lblSwTitle = new Label
            {
                Text = "快捷特性与渲染策略 (Quick Features)",
                Font = UITheme.FontCardHeader,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            cardSw.Controls.Add(lblSwTitle);

            int col1 = 22, col2 = 460;
            int rY1 = 52, rY2 = 110, rY3 = 168;

            AddSwitchItem(cardSw, col1, rY1, "投屏时自动息屏", "关闭手机物理屏幕，极大降低发热并保护隐私", out togScreenOff, false);
            AddSwitchItem(cardSw, col1, rY2, "窗口置顶显示", "保持投屏窗口始终固定于所有桌面应用最前端", out togAlwaysOnTop, false);
            AddSwitchItem(cardSw, col1, rY3, "无边框模式", "隐藏投屏窗口原生标题栏，呈现极简纯净画面", out togBorderless, false);

            AddSwitchItem(cardSw, col2, rY1, "保持手机常亮", "投屏运作期间阻止移动设备自动休眠锁屏", out togStayAwake, true);
            AddSwitchItem(cardSw, col2, rY2, "音频实时转发", "将设备内部声音极低延迟回传至电脑扬声器", out togAudio, true);
            AddSwitchItem(cardSw, col2, rY3, "绘制物理触控点", "在投屏画面上同步呈现手指点击与滑动圆点", out togTouches, false);

            pnl.Controls.Add(cardSw);

            // 3. 核心控制大按钮 (52px 高度极光质感)
            btnLaunchPrimary = new StudioButton
            {
                Text = "🚀 启动超清投屏 (零 CMD 黑框极客模式)",
                Location = new Point(0, 380),
                Size = new Size(626, 52),
                Radius = 8,
                Font = new Font("Microsoft YaHei UI", 11.5F, FontStyle.Bold),
                Style = StudioButton.ButtonStyle.Accent
            };
            btnLaunchPrimary.Click += BtnLaunchPrimary_Click;

            btnStopPrimary = new StudioButton
            {
                Text = "⏹ 终止投屏",
                Location = new Point(640, 380),
                Size = new Size(246, 52),
                Radius = 8,
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
                Style = StudioButton.ButtonStyle.Danger,
                Enabled = false
            };
            btnStopPrimary.Click += BtnStopPrimary_Click;

            pnl.Controls.AddRange(new Control[] { btnLaunchPrimary, btnStopPrimary });

            // 4. 底部快捷键速查条 (Shortcuts Dock - 消除底部死角空间)
            StudioCard cardDock = new StudioCard { Location = new Point(0, 444), Size = new Size(886, 60), BackColor = Color.FromArgb(16, 19, 25) };
            
            Label lblDockTag = new Label
            {
                Text = "⌨️ 快捷键速查 :",
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.Accent,
                BackColor = Color.FromArgb(16, 19, 25),
                Location = new Point(18, 20),
                AutoSize = true
            };
            cardDock.Controls.Add(lblDockTag);

            string[] shortcuts = new string[] {
                "Alt+F 全屏切换",
                "Alt+G 1:1原画比例",
                "Alt+H 主页 (Home)",
                "Alt+B 返回 (Back)",
                "Alt+S 多任务切换",
                "Alt+P 电源键",
                "Alt+O 关闭手机屏幕"
            };

            int badgeX = 148;
            foreach (string sc in shortcuts)
            {
                Label badge = new Label
                {
                    Text = sc,
                    Font = UITheme.FontSmall,
                    ForeColor = UITheme.TextSecondary,
                    BackColor = Color.FromArgb(24, 28, 38),
                    Location = new Point(badgeX, 18),
                    Padding = new Padding(6, 4, 6, 4),
                    AutoSize = true
                };
                cardDock.Controls.Add(badge);
                badgeX += badge.PreferredWidth + 10;
            }

            pnl.Controls.Add(cardDock);

            // 5. 快捷工装条 (截图 / 重启ADB / 终端)
            StudioCard cardQuickTools = new StudioCard { Location = new Point(0, 516), Size = new Size(886, 68), BackColor = Color.FromArgb(14, 17, 23) };

            Label lblToolHint = new Label
            {
                Text = "⚡ 快捷操作 :",
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = Color.FromArgb(14, 17, 23),
                Location = new Point(18, 24),
                AutoSize = true
            };

            StudioButton btnQuickCap = new StudioButton
            {
                Text = "📸 快速截屏",
                Location = new Point(126, 16),
                Size = new Size(130, 36),
                Style = StudioButton.ButtonStyle.Secondary
            };
            btnQuickCap.Click += (s, e) => TakeDeviceScreenshot();

            StudioButton btnQuickWake = new StudioButton
            {
                Text = "💡 唤醒屏幕",
                Location = new Point(268, 16),
                Size = new Size(130, 36),
                Style = StudioButton.ButtonStyle.Secondary
            };
            btnQuickWake.Click += (s, e) => SendAdbKeyEvent(224);

            StudioButton btnQuickTerm = new StudioButton
            {
                Text = "💻 打开 ADB 终端",
                Location = new Point(410, 16),
                Size = new Size(150, 36),
                Style = StudioButton.ButtonStyle.Secondary
            };
            btnQuickTerm.Click += (s, e) => OpenAdbTerminal();

            StudioButton btnQuickRestartAdb = new StudioButton
            {
                Text = "🔄 重启 ADB 服务",
                Location = new Point(572, 16),
                Size = new Size(150, 36),
                Style = StudioButton.ButtonStyle.Secondary
            };
            btnQuickRestartAdb.Click += (s, e) => RestartAdbServer();

            cardQuickTools.Controls.AddRange(new Control[] { lblToolHint, btnQuickCap, btnQuickWake, btnQuickTerm, btnQuickRestartAdb });
            pnl.Controls.Add(cardQuickTools);

            return pnl;
        }

        private void AddSwitchItem(Control parent, int x, int y, string title, string subtitle, out StudioToggle toggle, bool defState)
        {
            Label lblT = new Label
            {
                Text = title,
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = UITheme.CardBg,
                Location = new Point(x, y),
                Size = new Size(320, 20),
                AutoEllipsis = true
            };
            Label lblS = new Label
            {
                Text = subtitle,
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextMuted,
                BackColor = UITheme.CardBg,
                Location = new Point(x, y + 22),
                Size = new Size(320, 18),
                AutoEllipsis = true
            };
            toggle = new StudioToggle
            {
                Location = new Point(x + 350, y + 6),
                Checked = defState
            };
            parent.Controls.AddRange(new Control[] { lblT, lblS, toggle });
        }

        #endregion

        #region Page 2: 画质与性能调校 Display

        private Panel CreatePageDisplay()
        {
            Panel pnl = new Panel { Location = new Point(32, 74), Size = new Size(886, 610), BackColor = UITheme.WindowBg };

            // 1. 视频流与硬件加速卡片
            StudioCard cardVideo = new StudioCard { Location = new Point(0, 0), Size = new Size(886, 280) };
            Label lblVTitle = new Label
            {
                Text = "视频渲染与硬件编码 (Video & Acceleration)",
                Font = UITheme.FontCardHeader,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            cardVideo.Controls.Add(lblVTitle);

            int startY = 48, step = 56;

            comboResolution = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboResolution.Items.AddRange(new string[] { "跟随设备原生分辨率 (最高清画质)", "4K (3840×2160 极限画质)", "2K (2560×1440 超清均衡)", "1080P (1920×1080 标准推荐)", "720P (1280×720 流畅极速)" });
            comboResolution.SelectedIndex = 0;
            AddCompactSettingRow(cardVideo, 22, startY, "最大分辨率限制 (Max Size):", "限制长边像素可显著降低高分屏手机与 Wi-Fi 的编码吞吐压力", comboResolution);

            startY += step;
            comboFps = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboFps.Items.AddRange(new string[] { "原生无限制 (推荐原汁原味高刷)", "144 FPS (电竞极限刷新率)", "120 FPS (旗舰高帧率突破)", "90 FPS (平滑顺畅高帧)", "60 FPS (标准能耗平衡)", "30 FPS (超低功耗模式)" });
            comboFps.SelectedIndex = 0;
            AddCompactSettingRow(cardVideo, 22, startY, "显示刷新率上限 (Max FPS):", "默认不锁帧跟随手机原生高刷屏，亦可强制锁定指定刷新率", comboFps);

            startY += step;
            comboBitrate = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboBitrate.Items.AddRange(new string[] { "原生默认码率 (8 Mbps 均衡)", "16 Mbps (超清细腻推荐)", "32 Mbps (极客级发烧原画)", "64 Mbps (极限无损细节压制)", "4 Mbps (弱网超低占用)" });
            comboBitrate.SelectedIndex = 0;
            AddCompactSettingRow(cardVideo, 22, startY, "视频传输码率 (Video Bitrate):", "若需更细腻动态细节可调高码率，无线投屏若有卡顿可适当回调", comboBitrate);

            startY += step;
            comboCodec = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboCodec.Items.AddRange(new string[] { "自动协商选择 (Auto / 兼容最佳)", "H.265 / HEVC (高压缩比低带宽推荐)", "AV1 (下一代高画质开源标准)", "H.264 (广泛兼容基线标准)" });
            comboCodec.SelectedIndex = 0;
            AddCompactSettingRow(cardVideo, 22, startY, "视频硬件编码器 (Video Codec):", "优先推荐 H.265，在同等清晰度下可节约近 40% 的无线带宽消耗", comboCodec);

            pnl.Controls.Add(cardVideo);

            // 2. 音频转发与高级流控卡片
            StudioCard cardAudio = new StudioCard { Location = new Point(0, 296), Size = new Size(886, 280) };
            Label lblATitle = new Label
            {
                Text = "音频保真、缓冲与录制 (Audio & Stream Control)",
                Font = UITheme.FontCardHeader,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            cardAudio.Controls.Add(lblATitle);

            startY = 48;
            comboAudioCodec = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboAudioCodec.Items.AddRange(new string[] { "Opus (默认超低延迟高保真)", "AAC (广泛兼容多媒体格式)", "RAW (未压缩原始 PCM 音频流)" });
            comboAudioCodec.SelectedIndex = 0;
            AddCompactSettingRow(cardAudio, 22, startY, "音频编码协议 (Audio Codec):", "Android 11+ 支持原生抓取内部音频，Opus 提供极致超低听感延迟", comboAudioCodec);

            startY += step;
            comboAudioBitrate = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboAudioBitrate.Items.AddRange(new string[] { "128 kbps (标准CD音质)", "192 kbps (高品质保真音频)", "256 kbps (母带级发烧音质)", "64 kbps (语音通话极低延迟)" });
            comboAudioBitrate.SelectedIndex = 0;
            AddCompactSettingRow(cardAudio, 22, startY, "音频流码率 (Audio Bitrate):", "控制手机内部声音回传给电脑声卡时的传输采样压缩品质", comboAudioBitrate);

            startY += step;
            comboBuffer = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboBuffer.Items.AddRange(new string[] { "0 ms (极限零缓冲实时操控)", "25 ms (平滑微缓冲抗抖动)", "50 ms (抗丢包弱网防花屏)", "100 ms (高容错播放缓冲)" });
            comboBuffer.SelectedIndex = 0;
            AddCompactSettingRow(cardAudio, 22, startY, "画面显示缓冲区 (Display Buffer):", "游戏竞技建议开启 0ms；无线网络抖动导致轻微顿挫时可设为 25ms", comboBuffer);

            startY += step;
            comboRecord = new StudioComboBox { Location = new Point(500, startY), Width = 350 };
            comboRecord.Items.AddRange(new string[] { "仅实时投屏 (不录制保存本地)", "自动录制为 MP4 (H.264/AAC 通用)", "自动录制为 MKV (断电防损坏容器)" });
            comboRecord.SelectedIndex = 0;
            AddCompactSettingRow(cardAudio, 22, startY, "同步录制投屏流 (Auto Record):", "开启后投屏时将自动录制音视频流并无损持久化到当前工作目录", comboRecord);

            pnl.Controls.Add(cardAudio);
            return pnl;
        }

        private void AddCompactSettingRow(Control parent, int x, int y, string title, string sub, Control control)
        {
            Label lblT = new Label
            {
                Text = title,
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = UITheme.CardBg,
                Location = new Point(x, y + 2),
                AutoSize = true
            };
            Label lblS = new Label
            {
                Text = sub,
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextMuted,
                BackColor = UITheme.CardBg,
                Location = new Point(x, y + 22),
                AutoSize = true
            };

            parent.Controls.AddRange(new Control[] { lblT, lblS, control });
        }

        #endregion

        #region Page 3: 实用辅助工具箱 Tools

        private Panel CreatePageTools()
        {
            Panel pnl = new Panel { Location = new Point(32, 74), Size = new Size(886, 610), BackColor = UITheme.WindowBg };

            // 1. 移动端快捷控制按键
            StudioCard cardKeys = new StudioCard { Location = new Point(0, 0), Size = new Size(886, 260) };
            Label lblKTitle = new Label
            {
                Text = "移动端虚拟按键直通 (Android Key Control)",
                Font = UITheme.FontCardHeader,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            cardKeys.Controls.Add(lblKTitle);

            // 第一行按键：导航键
            StudioButton btnBack = new StudioButton { Text = "🔙 返回 (Back)", Location = new Point(22, 54), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnBack.Click += (s, e) => SendAdbKeyEvent(4);

            StudioButton btnHome = new StudioButton { Text = "🏠 主页 (Home)", Location = new Point(184, 54), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnHome.Click += (s, e) => SendAdbKeyEvent(3);

            StudioButton btnRecents = new StudioButton { Text = "📑 任务 (Recents)", Location = new Point(346, 54), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnRecents.Click += (s, e) => SendAdbKeyEvent(187);

            StudioButton btnPower = new StudioButton { Text = "🔌 电源 (Power)", Location = new Point(508, 54), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnPower.Click += (s, e) => SendAdbKeyEvent(26);

            StudioButton btnWake = new StudioButton { Text = "💡 唤醒点亮屏幕", Location = new Point(670, 54), Size = new Size(190, 42), Style = StudioButton.ButtonStyle.Accent };
            btnWake.Click += (s, e) => SendAdbKeyEvent(224);

            // 第二行按键：多媒体与屏幕
            StudioButton btnVolUp = new StudioButton { Text = "🔊 音量增加 (+)", Location = new Point(22, 110), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnVolUp.Click += (s, e) => SendAdbKeyEvent(24);

            StudioButton btnVolDown = new StudioButton { Text = "🔉 音量减小 (-)", Location = new Point(184, 110), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnVolDown.Click += (s, e) => SendAdbKeyEvent(25);

            StudioButton btnMute = new StudioButton { Text = "🔇 静音切换", Location = new Point(346, 110), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnMute.Click += (s, e) => SendAdbKeyEvent(164);

            StudioButton btnRotate = new StudioButton { Text = "🔄 旋转屏幕 90°", Location = new Point(508, 110), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnRotate.Click += (s, e) => RotateDeviceScreen();

            StudioButton btnShot = new StudioButton { Text = "📸 高清截屏到本地", Location = new Point(670, 110), Size = new Size(190, 42), Style = StudioButton.ButtonStyle.Accent };
            btnShot.Click += (s, e) => TakeDeviceScreenshot();

            // 第三行操作：展开通知栏
            StudioButton btnExpandNotif = new StudioButton { Text = "📥 下拉通知栏", Location = new Point(22, 166), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnExpandNotif.Click += (s, e) => RunAdbShell("cmd statusbar expand-notifications");

            StudioButton btnCollapseNotif = new StudioButton { Text = "📤 收起通知栏", Location = new Point(184, 166), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnCollapseNotif.Click += (s, e) => RunAdbShell("cmd statusbar collapse");

            StudioButton btnReboot = new StudioButton { Text = "⚠️ 重启设备系统", Location = new Point(346, 166), Size = new Size(150, 42), Style = StudioButton.ButtonStyle.Danger };
            btnReboot.Click += (s, e) => RebootDevice();

            StudioButton btnRebootBootloader = new StudioButton { Text = "⚡ 重启至 Bootloader", Location = new Point(508, 166), Size = new Size(190, 42), Style = StudioButton.ButtonStyle.Danger };
            btnRebootBootloader.Click += (s, e) => RebootBootloader();

            cardKeys.Controls.AddRange(new Control[] {
                btnBack, btnHome, btnRecents, btnPower, btnWake,
                btnVolUp, btnVolDown, btnMute, btnRotate, btnShot,
                btnExpandNotif, btnCollapseNotif, btnReboot, btnRebootBootloader
            });

            pnl.Controls.Add(cardKeys);

            // 2. 剪贴板快速同步与高级终端卡片
            StudioCard cardClip = new StudioCard { Location = new Point(0, 276), Size = new Size(886, 300) };
            Label lblCTitle = new Label
            {
                Text = "剪贴板文字推送与调试终端 (Clipboard & Diagnostics)",
                Font = UITheme.FontCardHeader,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            cardClip.Controls.Add(lblCTitle);

            Label lblClipSub = new Label
            {
                Text = "输入文字并一键直接打字推送到手机当前获得焦点的输入框（解决手机端输入复杂长密码难题）：",
                Font = UITheme.FontItemSub,
                ForeColor = UITheme.TextMuted,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 48),
                AutoSize = true
            };
            cardClip.Controls.Add(lblClipSub);

            StudioTextBox txtClip = new StudioTextBox { Location = new Point(22, 74), Size = new Size(680, 40) };
            StudioButton btnSendText = new StudioButton { Text = "推送文字到手机", Location = new Point(712, 74), Size = new Size(150, 40), Style = StudioButton.ButtonStyle.Accent };
            btnSendText.Click += (s, e) =>
            {
                string text = txtClip.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    // 对文本进行基础转义后通过 adb shell input text 发送
                    string safe = text.Replace(" ", "%s").Replace("&", "\\&").Replace("\"", "\\\"");
                    RunAdbShell("input text \"" + safe + "\"");
                    UpdateGlobalStatus(true, "文本已推送到设备");
                }
            };
            cardClip.Controls.AddRange(new Control[] { txtClip, btnSendText });

            Panel div = new Panel { Location = new Point(22, 134), Size = new Size(840, 1), BackColor = UITheme.InnerBorder };
            cardClip.Controls.Add(div);

            Label lblCmdTitle = new Label
            {
                Text = "开发者终端与诊断环境 :",
                Font = UITheme.FontItemTitle,
                ForeColor = UITheme.TextPrimary,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 150),
                AutoSize = true
            };
            cardClip.Controls.Add(lblCmdTitle);

            StudioButton btnTerm = new StudioButton { Text = "💻 打开独立 ADB 命令行", Location = new Point(22, 184), Size = new Size(200, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnTerm.Click += (s, e) => OpenAdbTerminal();

            StudioButton btnOpenWorkDir = new StudioButton { Text = "📂 打开工作台目录", Location = new Point(234, 184), Size = new Size(180, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnOpenWorkDir.Click += (s, e) => Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);

            StudioButton btnInstallApk = new StudioButton { Text = "📦 选择 APK 安装到设备", Location = new Point(426, 184), Size = new Size(200, 42), Style = StudioButton.ButtonStyle.Secondary };
            btnInstallApk.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Android 安装包 (*.apk)|*.apk" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    UpdateGlobalStatus(false, "正在安装 APK...");
                    RunAdbCommand("install -r \"" + ofd.FileName + "\"", (outStr) =>
                    {
                        MessageBox.Show(outStr, "APK 安装反馈", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateGlobalStatus(true, "APK 安装完成");
                    });
                }
            };

            cardClip.Controls.AddRange(new Control[] { btnTerm, btnOpenWorkDir, btnInstallApk });

            pnl.Controls.Add(cardClip);
            return pnl;
        }

        #endregion

        #region Page 4: 引擎与环境配置 Settings

        private Panel CreatePageSettings()
        {
            Panel pnl = new Panel { Location = new Point(32, 74), Size = new Size(886, 610), BackColor = UITheme.WindowBg };

            // 1. 核心程序路径配置
            StudioCard cardPath = new StudioCard { Location = new Point(0, 0), Size = new Size(886, 210) };
            Label lblPTitle = new Label
            {
                Text = "投屏核心与 ADB 服务路径 (Core Engine & ADB Paths)",
                Font = UITheme.FontCardHeader,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            cardPath.Controls.Add(lblPTitle);

            // scrcpy.exe 路径行
            Label lblScrcpyLabel = new Label { Text = "scrcpy.exe 核心路径 :", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, BackColor = UITheme.CardBg, Location = new Point(22, 54), AutoSize = true };
            lblScrcpyPathVal = new Label { Text = scrcpyExePath, Font = UITheme.FontMono, ForeColor = UITheme.TextSecondary, BackColor = UITheme.CardBg, Location = new Point(190, 56), Size = new Size(430, 20), AutoEllipsis = true };
            lblScrcpyStatus = new Label { Text = "● 检测中", Font = UITheme.FontSmall, ForeColor = UITheme.Warning, BackColor = UITheme.CardBg, Location = new Point(630, 56), AutoSize = true };
            StudioButton btnBrowseScrcpy = new StudioButton { Text = "浏览...", Location = new Point(740, 48), Size = new Size(120, 32), Style = StudioButton.ButtonStyle.Secondary };
            btnBrowseScrcpy.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "scrcpy 可执行文件 (scrcpy.exe)|scrcpy.exe" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SetCorePath(ofd.FileName);
                    AppendLog("已更新 scrcpy.exe 路径为: " + ofd.FileName);
                }
            };

            // adb.exe 路径行
            Label lblAdbLabel = new Label { Text = "adb.exe 调试工具路径 :", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, BackColor = UITheme.CardBg, Location = new Point(22, 102), AutoSize = true };
            lblAdbPathVal = new Label { Text = adbExePath, Font = UITheme.FontMono, ForeColor = UITheme.TextSecondary, BackColor = UITheme.CardBg, Location = new Point(190, 104), Size = new Size(430, 20), AutoEllipsis = true };
            lblAdbStatus = new Label { Text = "● 检测中", Font = UITheme.FontSmall, ForeColor = UITheme.Warning, BackColor = UITheme.CardBg, Location = new Point(630, 104), AutoSize = true };
            StudioButton btnBrowseAdb = new StudioButton { Text = "浏览...", Location = new Point(740, 96), Size = new Size(120, 32), Style = StudioButton.ButtonStyle.Secondary };
            btnBrowseAdb.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "ADB 可执行文件 (adb.exe)|adb.exe" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    adbExePath = ofd.FileName;
                    UpdateEngineStatusDisplay();
                    AppendLog("已手动指定 ADB 路径为: " + ofd.FileName);
                }
            };

            StudioButton btnAutoDetect = new StudioButton { Text = "🔍 重新自动检测环境", Location = new Point(22, 150), Size = new Size(180, 38), Style = StudioButton.ButtonStyle.Secondary };
            btnAutoDetect.Click += (s, e) =>
            {
                DetectScrcpyEngine();
                RefreshDeviceList();
                AppendLog("已完成工作台核心与调试环境重新自检");
            };

            StudioButton btnRestartAdbServer = new StudioButton { Text = "🔄 一键重启 ADB 守护进程", Location = new Point(214, 150), Size = new Size(200, 38), Style = StudioButton.ButtonStyle.Secondary };
            btnRestartAdbServer.Click += (s, e) => RestartAdbServer();

            cardPath.Controls.AddRange(new Control[] {
                lblScrcpyLabel, lblScrcpyPathVal, lblScrcpyStatus, btnBrowseScrcpy,
                lblAdbLabel, lblAdbPathVal, lblAdbStatus, btnBrowseAdb,
                btnAutoDetect, btnRestartAdbServer
            });

            pnl.Controls.Add(cardPath);

            // 2. 实时终端日志监视窗口
            StudioCard cardLog = new StudioCard { Location = new Point(0, 226), Size = new Size(886, 350) };
            Label lblLTitle = new Label
            {
                Text = "工作台运行与控制台实时日志 (Console & Live Logs)",
                Font = UITheme.FontCardHeader,
                ForeColor = UITheme.TextWhite,
                BackColor = UITheme.CardBg,
                Location = new Point(22, 16),
                AutoSize = true
            };
            cardLog.Controls.Add(lblLTitle);

            StudioButton btnClearLog = new StudioButton { Text = "清空日志", Location = new Point(760, 12), Size = new Size(100, 30), Style = StudioButton.ButtonStyle.Ghost };
            btnClearLog.Click += (s, e) => { txtLogMonitor.Clear(); };
            cardLog.Controls.Add(btnClearLog);

            txtLogMonitor = new TextBox
            {
                Location = new Point(22, 52),
                Size = new Size(842, 276),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(10, 12, 16),
                ForeColor = Color.FromArgb(170, 185, 200),
                Font = UITheme.FontMono,
                BorderStyle = BorderStyle.FixedSingle
            };
            cardLog.Controls.Add(txtLogMonitor);

            pnl.Controls.Add(cardLog);

            UpdateEngineStatusDisplay();
            AppendLog("Scrcpy Studio 现代化控制台已启动");
            return pnl;
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

        #region ADB 与设备辅助功能逻辑

        private void RefreshDeviceList()
        {
            comboDevices.Items.Clear();
            if (string.IsNullOrEmpty(adbExePath))
            {
                UpdateGlobalStatus(false, "未检测到 ADB 路径");
                AppendLog("警告: 未检测到 ADB 路径");
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
                    lblDeviceSerial.Text = comboDevices.SelectedItem;
                    lblDeviceBadge.Text = string.Format("● 在线设备 ({0})", comboDevices.Items.Count);
                    lblDeviceBadge.ForeColor = UITheme.Accent;
                    UpdateGlobalStatus(true, "设备在线就绪");
                    AppendLog(string.Format("成功发现 {0} 台可用 Android 调试设备: {1}", comboDevices.Items.Count, comboDevices.SelectedItem));
                }
                else
                {
                    lblDeviceSerial.Text = "无设备连接";
                    lblDeviceBadge.Text = "● 未检测到设备";
                    lblDeviceBadge.ForeColor = UITheme.Danger;
                    UpdateGlobalStatus(false, "等待设备插入");
                    AppendLog("未检测到任何 USB/Wi-Fi 调试设备，请确保已打开手机 USB 调试");
                }
            }
            catch (Exception ex)
            {
                UpdateGlobalStatus(false, "ADB 异常: " + ex.Message);
                AppendLog("ADB 异常: " + ex.Message);
            }
        }

        private void BtnAdbTcpip_Click(object sender, EventArgs e)
        {
            UpdateGlobalStatus(false, "开启 TCP/IP 5555 端口中...");
            AppendLog("正在向当前设备下发 'adb tcpip 5555' 开启无线调试守护端口...");
            RunAdbCommand("tcpip 5555", (outStr) =>
            {
                MessageBox.Show(outStr, "TCP/IP 5555 开启结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppendLog("TCP/IP 结果: " + outStr.Trim());
                UpdateGlobalStatus(true, "TCP/IP 端口已开放");
            });
        }

        private void BtnWirelessConnect_Click(object sender, EventArgs e)
        {
            string ip = txtWirelessIp.Text.Trim();
            if (string.IsNullOrEmpty(ip)) return;
            if (!ip.Contains(":")) ip += ":5555";

            UpdateGlobalStatus(false, "无线配对中...");
            AppendLog("正在尝试无线配对连接至: " + ip);
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
                    AppendLog("配对反馈: " + outStr.Trim());
                    MessageBox.Show(outStr, "无线配对结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshDeviceList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog("连接失败: " + ex.Message);
            }
        }

        private void SendAdbKeyEvent(int keyCode)
        {
            RunAdbShell("input keyevent " + keyCode);
            AppendLog("已向设备发送模拟物理按键 KeyCode: " + keyCode);
        }

        private void RotateDeviceScreen()
        {
            RunAdbShell("settings put system user_rotation 1");
            AppendLog("已请求设备旋转屏幕方向");
        }

        private void RebootDevice()
        {
            if (MessageBox.Show("确定要重启连接的 Android 手机吗？", "确认重启", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                RunAdbCommand("reboot", (outStr) => { AppendLog("已向设备发送系统重启指令"); });
            }
        }

        private void RebootBootloader()
        {
            if (MessageBox.Show("确定要将手机重启至 Bootloader (Fastboot) 模式吗？", "确认重启", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                RunAdbCommand("reboot bootloader", (outStr) => { AppendLog("已向设备发送重启至 Bootloader 指令"); });
            }
        }

        private void TakeDeviceScreenshot()
        {
            try
            {
                string devArg = (comboDevices.SelectedItem != null) ? string.Format("-s {0} ", comboDevices.SelectedItem) : "";
                string fileName = string.Format("screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                UpdateGlobalStatus(false, "正在抓取设备屏幕截图...");
                AppendLog("正在抓取屏幕截图并保存至: " + savePath);

                // 先在手机端截图，再拉取到本地
                RunAdbCommand(devArg + "shell screencap -p /sdcard/scrcpy_screen.png", (s1) =>
                {
                    RunAdbCommand(devArg + string.Format("pull /sdcard/scrcpy_screen.png \"{0}\"", savePath), (s2) =>
                    {
                        RunAdbShell(devArg + "rm /sdcard/scrcpy_screen.png");
                        if (File.Exists(savePath))
                        {
                            UpdateGlobalStatus(true, "截图已保存");
                            AppendLog("截图保存成功: " + fileName);
                            if (MessageBox.Show("截图已保存至:\n" + savePath + "\n\n是否立即打开查看？", "截图成功", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                            {
                                Process.Start(savePath);
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("截图失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog("截图失败: " + ex.Message);
            }
        }

        private void OpenAdbTerminal()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
                {
                    WorkingDirectory = File.Exists(scrcpyExePath) ? Path.GetDirectoryName(scrcpyExePath) : AppDomain.CurrentDomain.BaseDirectory
                };
                Process.Start(psi);
                AppendLog("已打开 ADB 独立命令行交互控制台");
            }
            catch { }
        }

        private void RestartAdbServer()
        {
            UpdateGlobalStatus(false, "正在重启 ADB 服务...");
            AppendLog("正在执行 'adb kill-server' 并重启守护进程...");
            RunAdbCommand("kill-server", (s1) =>
            {
                RunAdbCommand("start-server", (s2) =>
                {
                    RefreshDeviceList();
                    UpdateGlobalStatus(true, "ADB 服务已成功重启");
                    AppendLog("ADB 服务已成功重启");
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
                    {
                        this.BeginInvoke(new Action(() => onComplete(outStr)));
                    }
                };
            }
            catch (Exception ex)
            {
                AppendLog("执行 ADB 命令异常: " + ex.Message);
            }
        }

        #endregion

        #region 启动核心引擎与进程管理

        private void BtnLaunchPrimary_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(scrcpyExePath) || !File.Exists(scrcpyExePath))
            {
                MessageBox.Show("未找到核心程序 scrcpy.exe，请在【引擎与设置】中指定有效路径！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SelectNavTab(3);
                return;
            }

            if (scrcpyProcess != null && !scrcpyProcess.HasExited)
            {
                MessageBox.Show("投屏引擎已在运行中。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            // 视频码率
            if (comboBitrate.SelectedIndex == 1) args += "--video-bit-rate 16M ";
            else if (comboBitrate.SelectedIndex == 2) args += "--video-bit-rate 32M ";
            else if (comboBitrate.SelectedIndex == 3) args += "--video-bit-rate 64M ";
            else if (comboBitrate.SelectedIndex == 4) args += "--video-bit-rate 4M ";

            // 视频编码器
            if (comboCodec.SelectedIndex == 1) args += "--video-codec=h265 ";
            else if (comboCodec.SelectedIndex == 2) args += "--video-codec=av1 ";
            else if (comboCodec.SelectedIndex == 3) args += "--video-codec=h264 ";

            // 音频编码器与码率
            if (comboAudioCodec.SelectedIndex == 1) args += "--audio-codec=aac ";
            else if (comboAudioCodec.SelectedIndex == 2) args += "--audio-codec=raw ";

            if (comboAudioBitrate.SelectedIndex == 1) args += "--audio-bit-rate=192K ";
            else if (comboAudioBitrate.SelectedIndex == 2) args += "--audio-bit-rate=256K ";
            else if (comboAudioBitrate.SelectedIndex == 3) args += "--audio-bit-rate=64K ";

            // 缓冲区
            if (comboBuffer.SelectedIndex == 1) args += "--video-buffer=25 ";
            else if (comboBuffer.SelectedIndex == 2) args += "--video-buffer=50 ";
            else if (comboBuffer.SelectedIndex == 3) args += "--video-buffer=100 ";

            // 自动录制
            if (comboRecord.SelectedIndex == 1)
            {
                string recName = string.Format("record_{0}.mp4", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                args += string.Format("--record=\"{0}\" ", recName);
            }
            else if (comboRecord.SelectedIndex == 2)
            {
                string recName = string.Format("record_{0}.mkv", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                args += string.Format("--record=\"{0}\" ", recName);
            }

            // 开关特性
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

                AppendLog("正在以静默无黑框模式启动投屏: scrcpy " + args.Trim());

                scrcpyProcess = Process.Start(psi);
                scrcpyProcess.OutputDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) AppendLog("[scrcpy] " + ev.Data); };
                scrcpyProcess.ErrorDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) AppendLog("[scrcpy err] " + ev.Data); };
                scrcpyProcess.BeginOutputReadLine();
                scrcpyProcess.BeginErrorReadLine();
                scrcpyProcess.EnableRaisingEvents = true;

                scrcpyProcess.Exited += (s, ev) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        btnLaunchPrimary.Enabled = true;
                        btnStopPrimary.Enabled = false;
                        UpdateGlobalStatus(true, "工作台就绪");
                        AppendLog("投屏进程已正常退出");
                        scrcpyProcess = null;
                    }));
                };

                btnLaunchPrimary.Enabled = false;
                btnStopPrimary.Enabled = true;
                UpdateGlobalStatus(true, "投屏渲染中 (零黑框)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog("启动失败: " + ex.Message);
                UpdateGlobalStatus(false, "启动失败");
            }
        }

        private void BtnStopPrimary_Click(object sender, EventArgs e)
        {
            if (scrcpyProcess != null && !scrcpyProcess.HasExited)
            {
                try
                {
                    scrcpyProcess.Kill();
                    UpdateGlobalStatus(true, "已终止投屏");
                    AppendLog("用户手动终止了投屏进程");
                }
                catch { }
            }
        }

        private void UpdateGlobalStatus(bool ok, string text)
        {
            if (lblGlobalStatus == null) return;
            lblGlobalStatus.Text = text;
            lblStatusDot.ForeColor = ok ? UITheme.Success : UITheme.Danger;
        }

        #endregion
    }

    #endregion

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
