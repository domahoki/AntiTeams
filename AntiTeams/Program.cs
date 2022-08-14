using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntiTeams
{
    static class Program
    {
        private static bool enabled = true;
        private static int cursorStopSec = 0;
        private static Point prevCursorPos = Cursor.Position;

        [STAThread]
        static void Main()
        {
            CreateNotifyIcon();
            InitializeTimer();
            Application.Run();
        }

        private static void CreateNotifyIcon()
        {
            // 常駐アプリ（タスクトレイのアイコン）を
            var icon = new NotifyIcon();
            Assembly myAssembly = Assembly.GetExecutingAssembly().ManifestModule.Assembly;
            using (Stream s = myAssembly.GetManifestResourceStream("AntiTeams.Icon.ico"))
            {
                Icon ico = new Icon(s);
                icon.Icon = ico;
                s.Close();
            }
            //icon.Icon = new Icon("Icon.ico");
            icon.ContextMenuStrip = ContextMenu();
            icon.Text = "AntiTeams";
            icon.Visible = true;
        }

        private static void InitializeTimer()
        {
            Timer timer = new Timer();
            timer.Tick += TimerEventHandler;
            timer.Interval = 1000;
            timer.Enabled = true;
        }

        private static void TimerEventHandler(object sender, EventArgs e)
        {
            if (!enabled) { return; }

            Point curCursorPos = Cursor.Position;
            if ((curCursorPos.X == prevCursorPos.X) && 
                (curCursorPos.Y == prevCursorPos.Y))
            {
                cursorStopSec += 1;
            }
            else
            {
                prevCursorPos = curCursorPos;
                cursorStopSec = 0;
            }
            Debug.WriteLine(cursorStopSec);

            if (cursorStopSec > Properties.Settings.Default.TimeInterval)
            {
                MoveMouse();
                cursorStopSec = 0;
                Debug.WriteLine("Called Mouse Move.");
            }
        }

        private static ContextMenuStrip ContextMenu()
        {
            // アイコンを右クリックしたときのメニューを返却
            var menu = new ContextMenuStrip();
            var itemEnable = new ToolStripMenuItem("Enable", null, (s, e) => {
                var sender = (ToolStripMenuItem)s;
                sender.Checked = !sender.Checked;
                enabled = sender.Checked;
            });
            itemEnable.CheckState = CheckState.Checked;
            menu.Items.Add(itemEnable);
            menu.Items.Add("Setting", null, (s, e) => {
                var form = new ConfigForm();
                var rtn = form.ShowDialog();

                if (rtn.Equals(DialogResult.OK))
                {
                    Properties.Settings.Default.TimeInterval = (int)form.numericUpDown1.Value;
                    return;
                }
                // 設定内容を保存する
                Properties.Settings.Default.Save();
            });
            menu.Items.Add("Exit", null, (s, e) => {
                Application.Exit();
            });
            
            return menu;
        }

        private static async void MoveMouse()
        {
            int posX = Cursor.Position.X;
            int posY = Cursor.Position.Y;

            double steps = 100;
            double R = 20;
            for(int i = 0; i < (int)steps; i++)
            {
                double t = (double)i / steps;
                int toX = (int)Math.Round(R * Math.Sin(2 * t * Math.PI) + posX);
                int toY = (int)Math.Round(R * Math.Cos(2 * t * Math.PI) + posY - R);
                Cursor.Position = new Point(toX, toY);
                await Task.Delay((int)(1 / steps * 1000));
            }
        }
    }
}
