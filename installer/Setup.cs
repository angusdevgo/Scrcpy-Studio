using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ScrcpyStudioSetup
{
    #region 设计系统（与主程序一致的 Satelite 薄荷主题）

    internal static class Th
    {
        public static readonly Color WindowBg   = Color.FromArgb(227, 239, 233);
        public static readonly Color CardBg     = Color.FromArgb(248, 252, 250);
        public static readonly Color CardBorder = Color.FromArgb(208, 226, 218);
        public static readonly Color Accent     = Color.FromArgb(34, 163, 107);
        public static readonly Color AccentHover= Color.FromArgb(40, 185, 122);
        public static readonly Color AccentDown = Color.FromArgb(28, 140, 92);
        public static readonly Color Secondary  = Color.FromArgb(215, 232, 224);
        public static readonly Color SecondaryHover = Color.FromArgb(205, 225, 215);
        public static readonly Color TextPrimary= Color.FromArgb(26, 44, 36);
        public static readonly Color TextSecond = Color.FromArgb(94, 118, 106);
        public static readonly Color TextMuted  = Color.FromArgb(140, 160, 150);
        public static readonly Color Danger     = Color.FromArgb(239, 68, 68);
        public static readonly Color Track      = Color.FromArgb(210, 228, 220);

        public static float Scale = 1f;

        public static void InitDpi()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                Scale = g.DpiX / 96f;
        }

        public static int Dpi(int v) { return (int)Math.Round(v * Scale); }

        public static Font Brand    = new Font("Microsoft YaHei UI", 13f, FontStyle.Bold);
        public static Font Hero     = new Font("Microsoft YaHei UI", 15f, FontStyle.Bold);
        public static Font Title    = new Font("Microsoft YaHei UI", 11f, FontStyle.Bold);
        public static Font Body     = new Font("Microsoft YaHei UI", 9f);
        public static Font BodyBold = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold);
        public static Font Small    = new Font("Microsoft YaHei UI", 8f);
        public static Font Mono     = new Font("Consolas", 9f);
    }

    internal static class Geo
    {
        public static GraphicsPath Round(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            if (radius <= 0) { p.AddRectangle(r); return p; }
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    #endregion

    #region 自绘按钮

    internal class FlatButton : Control
    {
        public enum Style { Accent, Secondary, Ghost }

        private bool _hover, _press;
        public Style Kind = Style.Secondary;

        public FlatButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(Th.Dpi(110), Th.Dpi(38));
            Cursor = Cursors.Hand;
            Font = Th.BodyBold;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _press = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _press = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _press = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color bg, fg = Color.White;
            if (!Enabled)
            {
                bg = Color.FromArgb(224, 234, 229);
                fg = Th.TextMuted;
            }
            else if (Kind == Style.Accent)
            {
                bg = _press ? Th.AccentDown : (_hover ? Th.AccentHover : Th.Accent);
            }
            else if (Kind == Style.Secondary)
            {
                bg = _press ? Color.FromArgb(195, 218, 208) : (_hover ? Th.SecondaryHover : Th.Secondary);
                fg = Th.TextPrimary;
            }
            else
            {
                bg = _press ? Color.FromArgb(210, 226, 218) : (_hover ? Color.FromArgb(220, 235, 228) : Color.Transparent);
                fg = Th.TextPrimary;
            }

            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath p = Geo.Round(r, Th.Dpi(10)))
            using (SolidBrush b = new SolidBrush(bg))
                g.FillPath(b, p);

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    #endregion

    #region 安装向导主窗体

    internal class SetupForm : Form
    {
        // ---- 默认值 ----
        private const string AppName = "Scrcpy Studio";
        private const string AppVersion = "5.0.1";
        private const string Publisher = "angusdevgo";
        private const string PayloadExe = "ScrcpyStudio.exe";
        private const string UninstExe = "Uninstall.exe";
        private const string UninstKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Scrcpy Studio";

        private int _page = 0;
        private string _installDir;

        private Panel header, body, footer;
        private Label lblHeadTitle, lblHeadSub;
        private FlatButton btnBack, btnNext, btnCancel;

        // Page 0
        private Panel p0;
        private TextBox txtDir;
        private FlatButton btnBrowse;
        private CheckBox chkDesktop, chkStartMenu, chkRun;

        // Page 1
        private Panel p1;
        private Label lblStatus;
        private ProgressBar bar;
        private TextBox txtLog;

        // Page 2
        private Panel p2;
        private Label lblDone;
        private CheckBox chkRunNow;

        private bool _installing = false;
        private bool _installed = false;

        public SetupForm()
        {
            Th.InitDpi();

            Text = AppName + " 安装向导";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Th.WindowBg;
            Font = Th.Body;
            ClientSize = new Size(Th.Dpi(760), Th.Dpi(520));
            DoubleBuffered = true;
            Icon = LoadOwnIcon();

            _installDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Scrcpy Studio");

            BuildChrome();
            BuildPage0();
            BuildPage1();
            BuildPage2();
            ShowPage(0);
        }

        private Icon LoadOwnIcon()
        {
            try
            {
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Setup.icon.ico");
                if (s != null) return new Icon(s);
            }
            catch { }
            return null;
        }

        // ---------------- 外框 ----------------

        private void BuildChrome()
        {
            header = new Panel { BackColor = Th.WindowBg };
            header.Paint += PaintHeader;
            Controls.Add(header);

            lblHeadTitle = new Label
            {
                Text = "欢迎使用 " + AppName + " 安装向导",
                Font = Th.Hero,
                ForeColor = Th.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            header.Controls.Add(lblHeadTitle);

            lblHeadSub = new Label
            {
                Text = "安卓免安装投屏工具 · 内置投屏引擎 · 无需命令行",
                Font = Th.Body,
                ForeColor = Th.TextSecond,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            header.Controls.Add(lblHeadSub);

            body = new Panel { BackColor = Th.WindowBg };
            Controls.Add(body);

            footer = new Panel { BackColor = Th.WindowBg };
            Controls.Add(footer);

            btnBack = new FlatButton { Text = "上一步", Kind = FlatButton.Style.Secondary };
            btnBack.Click += (s, e) => { if (_page > 0) ShowPage(_page - 1); };
            footer.Controls.Add(btnBack);

            btnNext = new FlatButton { Text = "下一步", Kind = FlatButton.Style.Accent };
            btnNext.Click += (s, e) => OnNext();
            footer.Controls.Add(btnNext);

            btnCancel = new FlatButton { Text = "取消", Kind = FlatButton.Style.Ghost };
            btnCancel.Click += (s, e) => Close();
            footer.Controls.Add(btnCancel);

            LayoutChrome();
        }

        private void PaintHeader(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle r = new Rectangle(Th.Dpi(24), Th.Dpi(18), header.Width - Th.Dpi(48), header.Height - Th.Dpi(28));
            using (GraphicsPath p = Geo.Round(r, Th.Dpi(16)))
            using (LinearGradientBrush b = new LinearGradientBrush(r,
                Color.FromArgb(232, 245, 239), Color.FromArgb(214, 235, 226), 25f))
            using (Pen pen = new Pen(Color.FromArgb(205, 228, 218), 1.2f))
            {
                g.FillPath(b, p);
                g.DrawPath(pen, p);
            }

            // 品牌徽标
            Rectangle logo = new Rectangle(r.X + Th.Dpi(24), r.Y + Th.Dpi(20), Th.Dpi(44), Th.Dpi(44));
            using (GraphicsPath lp = Geo.Round(logo, Th.Dpi(12)))
            using (SolidBrush lb = new SolidBrush(Th.Accent))
                g.FillPath(lb, lp);
            using (Font f = new Font("Segoe UI Symbol", 15f, FontStyle.Bold))
            using (SolidBrush wb = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("◈", f, wb, logo, sf);
            }
        }

        private void LayoutChrome()
        {
            if (header == null || body == null || footer == null) return;
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            header.SetBounds(0, 0, w, Th.Dpi(108));
            footer.SetBounds(0, h - Th.Dpi(72), w, Th.Dpi(72));
            body.SetBounds(0, Th.Dpi(108), w, h - Th.Dpi(108) - Th.Dpi(72));

            lblHeadTitle.Location = new Point(Th.Dpi(88), Th.Dpi(30));
            lblHeadSub.Location = new Point(Th.Dpi(89), Th.Dpi(62));

            int btnY = (footer.Height - btnNext.Height) / 2;
            btnCancel.SetBounds(w - Th.Dpi(24) - btnCancel.Width, btnY, btnCancel.Width, btnNext.Height);
            btnNext.SetBounds(btnCancel.Left - Th.Dpi(10) - btnNext.Width, btnY, btnNext.Width, btnNext.Height);
            btnBack.SetBounds(btnNext.Left - Th.Dpi(10) - btnBack.Width, btnY, btnBack.Width, btnNext.Height);

            if (p0 != null) p0.SetBounds(0, 0, body.Width, body.Height);
            if (p1 != null) p1.SetBounds(0, 0, body.Width, body.Height);
            if (p2 != null) p2.SetBounds(0, 0, body.Width, body.Height);
            LayoutPage0();
            LayoutPage1();
            LayoutPage2();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutChrome();
        }

        // ---------------- 第 1 页：选择目录 ----------------

        private void BuildPage0()
        {
            p0 = new Panel { BackColor = Th.WindowBg };

            Label lbl = new Label
            {
                Text = "选择安装位置",
                Font = Th.Title,
                ForeColor = Th.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Th.Dpi(32), Th.Dpi(14))
            };
            p0.Controls.Add(lbl);

            Label hint = new Label
            {
                Text = "安装程序会把 " + AppName + " 释放到下面的文件夹，并创建快捷方式。约需 60 MB 磁盘空间。",
                Font = Th.Body,
                ForeColor = Th.TextSecond,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Th.Dpi(32), Th.Dpi(46))
            };
            p0.Controls.Add(hint);

            txtDir = new TextBox
            {
                Text = _installDir,
                Font = Th.Mono,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Th.TextPrimary
            };
            p0.Controls.Add(txtDir);

            btnBrowse = new FlatButton { Text = "浏览...", Kind = FlatButton.Style.Secondary };
            btnBrowse.Click += (s, e) =>
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "选择 " + AppName + " 的安装目录";
                fbd.SelectedPath = SafeDir(txtDir.Text);
                if (fbd.ShowDialog(this) == DialogResult.OK)
                    txtDir.Text = Path.Combine(fbd.SelectedPath, "Scrcpy Studio");
            };
            p0.Controls.Add(btnBrowse);

            Label opt = new Label
            {
                Text = "安装选项",
                Font = Th.Title,
                ForeColor = Th.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Th.Dpi(32), Th.Dpi(132))
            };
            p0.Controls.Add(opt);

            chkDesktop = MakeCheck("创建桌面快捷方式（推荐）", true);
            chkStartMenu = MakeCheck("添加到开始菜单", true);
            chkRun = MakeCheck("安装完成后立即启动 " + AppName, true);
            p0.Controls.Add(chkDesktop);
            p0.Controls.Add(chkStartMenu);
            p0.Controls.Add(chkRun);

            body.Controls.Add(p0);
        }

        private CheckBox MakeCheck(string text, bool check)
        {
            return new CheckBox
            {
                Text = text,
                Checked = check,
                Font = Th.Body,
                ForeColor = Th.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true
            };
        }

        private void LayoutPage0()
        {
            if (p0 == null) return;
            int w = p0.Width;
            txtDir.SetBounds(Th.Dpi(32), Th.Dpi(76), w - Th.Dpi(32) - Th.Dpi(24) - Th.Dpi(100) - Th.Dpi(10), Th.Dpi(30));
            btnBrowse.SetBounds(txtDir.Right + Th.Dpi(10), Th.Dpi(74), Th.Dpi(100), Th.Dpi(32));
            chkDesktop.Location = new Point(Th.Dpi(40), Th.Dpi(170));
            chkStartMenu.Location = new Point(Th.Dpi(40), Th.Dpi(202));
            chkRun.Location = new Point(Th.Dpi(40), Th.Dpi(234));
        }

        // ---------------- 第 2 页：安装进度 ----------------

        private void BuildPage1()
        {
            p1 = new Panel { BackColor = Th.WindowBg };

            Label lbl = new Label
            {
                Text = "正在安装",
                Font = Th.Title,
                ForeColor = Th.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Th.Dpi(32), Th.Dpi(14))
            };
            p1.Controls.Add(lbl);

            lblStatus = new Label
            {
                Text = "准备中…",
                Font = Th.Body,
                ForeColor = Th.TextSecond,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Th.Dpi(32), Th.Dpi(50))
            };
            p1.Controls.Add(lblStatus);

            bar = new ProgressBar { Minimum = 0, Maximum = 100, Value = 0, Style = ProgressBarStyle.Continuous };
            p1.Controls.Add(bar);

            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(242, 248, 245),
                ForeColor = Th.TextPrimary,
                Font = Th.Mono
            };
            p1.Controls.Add(txtLog);

            body.Controls.Add(p1);
        }

        private void LayoutPage1()
        {
            if (p1 == null) return;
            int w = p1.Width, h = p1.Height;
            bar.SetBounds(Th.Dpi(32), Th.Dpi(76), w - Th.Dpi(64), Th.Dpi(18));
            txtLog.SetBounds(Th.Dpi(32), Th.Dpi(112), w - Th.Dpi(64), h - Th.Dpi(112) - Th.Dpi(16));
        }

        // ---------------- 第 3 页：完成 ----------------

        private void BuildPage2()
        {
            p2 = new Panel { BackColor = Th.WindowBg };

            lblDone = new Label
            {
                Text = "安装完成",
                Font = Th.Hero,
                ForeColor = Th.Accent,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Th.Dpi(32), Th.Dpi(20))
            };
            p2.Controls.Add(lblDone);

            Label desc = new Label
            {
                Text = AppName + " 已成功安装到你的电脑。\r\n\r\n"
                     + "· 桌面已生成快捷方式，双击即可启动\r\n"
                     + "· 投屏引擎（scrcpy / adb / 运行库）已内置，无需额外文件\r\n"
                     + "· 可在「设置 → 应用」或控制面板中卸载",
                Font = Th.Body,
                ForeColor = Th.TextSecond,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Th.Dpi(32), Th.Dpi(66))
            };
            p2.Controls.Add(desc);

            chkRunNow = MakeCheck("立即启动 " + AppName, true);
            chkRunNow.Location = new Point(Th.Dpi(40), Th.Dpi(196));
            p2.Controls.Add(chkRunNow);

            body.Controls.Add(p2);
        }

        private void LayoutPage2()
        {
            if (p2 == null) return;
        }

        // ---------------- 页面切换 ----------------

        private void ShowPage(int i)
        {
            _page = i;
            p0.Visible = (i == 0);
            p1.Visible = (i == 1);
            p2.Visible = (i == 2);

            if (i == 0)
            {
                lblHeadTitle.Text = "欢迎使用 " + AppName + " 安装向导";
                lblHeadSub.Text = "安卓免安装投屏工具 · 内置投屏引擎 · 无需命令行";
                btnBack.Enabled = false;
                btnNext.Text = "开始安装";
                btnCancel.Visible = true;
            }
            else if (i == 1)
            {
                lblHeadTitle.Text = "正在安装 " + AppName;
                lblHeadSub.Text = "请稍候，正在释放文件并创建快捷方式…";
                btnBack.Enabled = false;
                btnNext.Text = "安装中…";
                btnNext.Enabled = false;
                btnCancel.Visible = false;
            }
            else
            {
                lblHeadTitle.Text = "安装已完成";
                lblHeadSub.Text = "感谢使用 " + AppName;
                btnBack.Enabled = false;
                btnNext.Text = "完成";
                btnNext.Enabled = true;
                btnCancel.Visible = false;
            }
        }

        private void OnNext()
        {
            if (_page == 0) { StartInstall(); return; }
            if (_page == 2) { Finish(); return; }
        }

        // ---------------- 安装流程 ----------------

        private static string SafeDir(string s)
        {
            try
            {
                s = s.Trim().Trim('"');
                if (s.Length == 0) return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                return Path.GetFullPath(s);
            }
            catch
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
        }

        private void StartInstall()
        {
            string dir = SafeDir(txtDir.Text);
            if (dir.Length < 4)
            {
                MessageBox.Show(this, "安装路径无效，请重新选择。", AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _installDir = dir;
            ShowPage(1);

            Thread t = new Thread(DoInstall);
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void Ui(Action a)
        {
            if (IsDisposed) return;
            try { BeginInvoke(a); } catch { }
        }

        private void Log(string s)
        {
            Ui(delegate
            {
                if (txtLog != null && !txtLog.IsDisposed)
                {
                    txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s + "\r\n");
                }
            });
        }

        private void SetStatus(string s, int pct)
        {
            Ui(delegate
            {
                if (lblStatus != null && !lblStatus.IsDisposed) lblStatus.Text = s;
                if (bar != null && !bar.IsDisposed)
                {
                    if (pct < bar.Minimum) pct = bar.Minimum;
                    if (pct > bar.Maximum) pct = bar.Maximum;
                    bar.Value = pct;
                }
            });
        }

        private void DoInstall()
        {
            _installing = true;
            try
            {
                SetStatus("检查安装目录…", 3);
                if (Directory.Exists(_installDir) && Directory.GetFileSystemEntries(_installDir).Length > 0)
                {
                    // 目录已存在且非空：提示覆盖
                    DialogResult dr = DialogResult.OK;
                    Ui(delegate
                    {
                        dr = MessageBox.Show(this,
                            "目录已存在且不为空：\r\n" + _installDir + "\r\n\r\n继续安装将覆盖同名文件，是否继续？",
                            AppName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    });
                    Thread.Sleep(180);
                    if (dr != DialogResult.OK)
                    {
                        Ui(delegate { ShowPage(0); btnNext.Enabled = true; btnNext.Text = "开始安装"; });
                        _installing = false;
                        return;
                    }
                }

                if (!Directory.Exists(_installDir)) Directory.CreateDirectory(_installDir);
                Log("安装目录：" + _installDir);

                Assembly asm = Assembly.GetExecutingAssembly();
                string[] names = asm.GetManifestResourceNames();

                List<string> payload = new List<string>();
                foreach (string n in names)
                {
                    if (n.StartsWith("Payload.", StringComparison.OrdinalIgnoreCase))
                        payload.Add(n);
                }

                int idx = 0;
                foreach (string res in payload)
                {
                    string fileName = res.Substring("Payload.".Length);
                    string target = Path.Combine(_installDir, fileName);
                    SetStatus("释放 " + fileName + " …", 10 + (int)(60.0 * idx / Math.Max(1, payload.Count)));
                    Log("释放 " + fileName);
                    using (Stream src = asm.GetManifestResourceStream(res))
                    {
                        if (src == null) { idx++; continue; }
                        using (FileStream dst = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None))
                            src.CopyTo(dst, 81920);
                    }
                    idx++;
                }

                SetStatus("创建快捷方式…", 78);
                string exePath = Path.Combine(_installDir, PayloadExe);
                if (chkDesktop.Checked)
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    if (CreateShortcut(Path.Combine(desktop, AppName + ".lnk"), exePath, _installDir))
                        Log("已创建桌面快捷方式");
                }
                if (chkStartMenu.Checked)
                {
                    string sm = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
                    if (!Directory.Exists(sm))
                        sm = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    if (CreateShortcut(Path.Combine(sm, AppName + ".lnk"), exePath, _installDir))
                        Log("已添加到开始菜单");
                }

                SetStatus("写入卸载信息…", 88);
                WriteUninstallEntry(exePath);
                Log("已注册卸载信息");

                SetStatus("完成", 100);
                _installed = true;
                _installing = false;

                Ui(delegate { ShowPage(2); });
            }
            catch (Exception ex)
            {
                _installing = false;
                Log("错误：" + ex.Message);
                SetStatus("安装失败：" + ex.Message, 0);
                Ui(delegate
                {
                    MessageBox.Show(this, "安装失败：\r\n" + ex.Message, AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ShowPage(0);
                    btnNext.Enabled = true;
                    btnNext.Text = "开始安装";
                });
            }
        }

        private void WriteUninstallEntry(string exePath)
        {
            RegistryKey k = Registry.LocalMachine.CreateSubKey(UninstKey);
            if (k == null) return;
            try
            {
                string uninst = Path.Combine(_installDir, UninstExe);
                k.SetValue("DisplayName", AppName);
                k.SetValue("DisplayVersion", AppVersion);
                k.SetValue("Publisher", Publisher);
                k.SetValue("DisplayIcon", exePath);
                k.SetValue("InstallLocation", _installDir);
                k.SetValue("UninstallString", "\"" + uninst + "\"");
                k.SetValue("QuietUninstallString", "\"" + uninst + "\" /quiet");
                k.SetValue("NoModify", 1, RegistryValueKind.DWord);
                k.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                k.SetValue("EstimatedSize", EstimateKb(_installDir), RegistryValueKind.DWord);
            }
            finally { k.Close(); }
        }

        private static int EstimateKb(string dir)
        {
            try
            {
                long total = 0;
                foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    try { total += new FileInfo(f).Length; } catch { }
                }
                return (int)Math.Min(int.MaxValue, total / 1024);
            }
            catch { return 0; }
        }

        /// <summary>用 WScript.Shell 晚绑定创建 .lnk（无需任何引用）</summary>
        private static bool CreateShortcut(string lnkPath, string target, string workDir)
        {
            try
            {
                Type t = Type.GetTypeFromProgID("WScript.Shell");
                if (t == null) return false;
                object shell = Activator.CreateInstance(t);
                object lnk = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { lnkPath });
                Type lt = lnk.GetType();
                lt.InvokeMember("TargetPath", BindingFlags.SetProperty, null, lnk, new object[] { target });
                lt.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, lnk, new object[] { workDir });
                lt.InvokeMember("IconLocation", BindingFlags.SetProperty, null, lnk, new object[] { target + ",0" });
                lt.InvokeMember("Description", BindingFlags.SetProperty, null, lnk, new object[] { AppName + " - 安卓免安装投屏工具" });
                lt.InvokeMember("WindowStyle", BindingFlags.SetProperty, null, lnk, new object[] { 1 });
                lt.InvokeMember("Save", BindingFlags.InvokeMethod, null, lnk, null);
                return true;
            }
            catch { return false; }
        }

        private void Finish()
        {
            bool run = chkRunNow != null && chkRunNow.Checked;
            string exePath = Path.Combine(_installDir, PayloadExe);
            Close();
            if (run && File.Exists(exePath))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo(exePath);
                    psi.WorkingDirectory = _installDir;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                catch { }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_installing)
            {
                e.Cancel = true;
                return;
            }
            if (_installed && _page < 2 && !_installing)
            {
                // 安装已完成但用户提前关闭 → 询问是否启动
                DialogResult dr = MessageBox.Show(this, "安装已完成，是否立即启动 " + AppName + "？",
                    AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    string exePath = Path.Combine(_installDir, PayloadExe);
                    try
                    {
                        if (File.Exists(exePath))
                        {
                            ProcessStartInfo psi = new ProcessStartInfo(exePath);
                            psi.WorkingDirectory = _installDir;
                            psi.UseShellExecute = true;
                            Process.Start(psi);
                        }
                    }
                    catch { }
                }
            }
            base.OnFormClosing(e);
        }
    }

    #endregion

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new SetupForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("安装程序异常：\r\n" + ex.Message, "Scrcpy Studio 安装向导",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
