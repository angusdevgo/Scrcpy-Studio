using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ScrcpyStudioUninstall
{
    internal static class Program
    {
        private const string AppName = "Scrcpy Studio";
        private const string UninstKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Scrcpy Studio";

        [STAThread]
        private static void Main(string[] args)
        {
            bool quiet = false;
            foreach (string a in args)
            {
                if (string.Equals(a, "/quiet", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(a, "/s", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(a, "/silent", StringComparison.OrdinalIgnoreCase))
                    quiet = true;
            }

            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!quiet)
            {
                DialogResult dr = MessageBox.Show(
                    "确定要卸载 " + AppName + " 吗？\r\n\r\n"
                    + "卸载位置：" + dir + "\r\n\r\n"
                    + "将删除程序文件、桌面与开始菜单快捷方式。",
                    AppName + " 卸载",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
            }

            // 1. 结束正在运行的程序
            KillByName("ScrcpyStudio");
            KillByName("scrcpy");
            KillByName("adb");
            Thread.Sleep(400);

            // 2. 删除快捷方式
            TryDelete(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                AppName + ".lnk"));
            TryDelete(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                AppName + ".lnk"));
            TryDelete(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                AppName + ".lnk"));

            // 3. 删除注册表卸载项
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(UninstKey, true))
                    if (k != null) Registry.LocalMachine.DeleteSubKeyTree(UninstKey, false);
            }
            catch { }

            // 4. 删除运行缓存（内嵌引擎释放目录）
            try
            {
                string cache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ScrcpyStudio");
                if (Directory.Exists(cache)) Directory.Delete(cache, true);
            }
            catch { }

            // 5. 复制自身到临时目录，由临时副本删除安装目录
            string tmp = Path.Combine(Path.GetTempPath(), "scrcpy_studio_uninst_" + Guid.NewGuid().ToString("N") + ".exe");
            try
            {
                File.Copy(Assembly.GetExecutingAssembly().Location, tmp, true);

                string script = "@echo off\r\n"
                    + "ping 127.0.0.1 -n 3 >nul\r\n"
                    + "rmdir /s /q \"" + dir + "\"\r\n"
                    + "del /f /q \"" + tmp + "\"\r\n";
                string bat = Path.Combine(Path.GetTempPath(), "scrcpy_studio_uninst_" + Guid.NewGuid().ToString("N") + ".bat");
                File.WriteAllText(bat, script);

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c \"" + bat + "\"");
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi);
            }
            catch { }

            if (!quiet)
            {
                MessageBox.Show(AppName + " 已卸载完成。", AppName + " 卸载",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void KillByName(string name)
        {
            try
            {
                foreach (Process p in Process.GetProcessesByName(name))
                {
                    try { p.Kill(); p.WaitForExit(2000); } catch { }
                }
            }
            catch { }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }
    }
}
