using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DawonHistoryMes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public string DPTpath, MESpath;
        public DateTime now;
        string buffer;
        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDawon.ShowDialog();
            DPTpath = folderBrowserDawon.SelectedPath;
            tbDawonHistoryFolder.Text = DPTpath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserMes.ShowDialog();
            MESpath = folderBrowserMes.SelectedPath;
            tbMesHistoryFolder.Text = MESpath;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@"C:\DEV_PJT\Dawon_MES_BRIDE\")) Directory.CreateDirectory(@"C:\DEV_PJT\Dawon_MES_BRIDE\");
            string config = DPTpath + Environment.NewLine + MESpath + Environment.NewLine + nudCycletime.Value.ToString();
            File.WriteAllText(@"C:\DEV_PJT\Dawon_MES_BRIDE\configs.cfg", config);

            if (!File.Exists(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt"))
                File.WriteAllText(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt", buffer);
            else
                buffer = File.ReadAllText(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt");

            notifyIcon.Visible = true;
            now = DateTime.Now;
            string folderResult = now.ToString("yyyyMM");
            tbDawonHistoryFolder.Text = DPTpath;
            tbMesHistoryFolder.Text = MESpath;
            timer1.Interval = (int)nudCycletime.Value * 1000 * 60;
            timer1.Start();
            copyFile();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(@"C:\DEV_PJT\Dawon_MES_BRIDE\")) Directory.CreateDirectory(@"C:\DEV_PJT\Dawon_MES_BRIDE\");
            if (!File.Exists(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt"))
                File.WriteAllText(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt", buffer);
            else
                buffer = File.ReadAllText(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt");

            if (File.Exists(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "configs.cfg"))
            {
                string[] config = File.ReadAllLines(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "configs.cfg");
                DPTpath = config[0];
                MESpath = config[1];
                nudCycletime.Value = Convert.ToInt32(config[2]);
                timer1.Interval = (int)nudCycletime.Value * 1000 * 60;
                tbDawonHistoryFolder.Text = DPTpath;
                tbMesHistoryFolder.Text = MESpath;
            }
        }

        public void copyFile()
        {
            now = DateTime.Now;
            string folderResult = now.ToString("yyyy") + @"\" + now.ToString("MM");
            string fileResult = now.ToString("dd");

            if (!Directory.Exists(MESpath + @"\" + folderResult)) Directory.CreateDirectory(MESpath);

            if (!File.Exists(MESpath + @"\" + fileResult + ".csv"))
            {
                if (Directory.Exists(DPTpath + @"\" + folderResult))
                {
                    Console.WriteLine(DPTpath + @"\" + folderResult);
                    foreach (string f in Directory.GetFiles(DPTpath + @"\" + folderResult, "*.csv"))
                    {
                        if (f.Contains(fileResult))
                        {
                            Console.WriteLine(f);
                            string extension = Path.GetExtension(f);
                            if (extension != null && (extension.Equals(".csv")))
                            {
                                FileInfo fileInfo = new FileInfo(f);
                                if (!IsFileLocked(fileInfo))
                                {
                                    string alldata = File.ReadAllText(f);
                                    buffer = File.ReadAllText(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt");
                                    string newdata = "";
                                    if (buffer.Length < alldata.Length)
                                        newdata = alldata.Remove(0, buffer.Length);
                                    else if (buffer.Length > alldata.Length)
                                        newdata = alldata;
                                    buffer = alldata;

                                    if (newdata.Length > 0)
                                        File.WriteAllText(MESpath + @"\" + fileInfo.Name, newdata);
                                    File.WriteAllText(@"C:\DEV_PJT\Dawon_MES_BRIDE\" + "buffer.txt", alldata);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)nudCycletime.Value * 1000 * 60;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            copyFile();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void notifyIcon_MouseDown(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

    }
}
