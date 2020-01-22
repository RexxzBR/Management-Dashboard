using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dashboard
{
    public partial class Dashboard : Form
    {
        private PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private IntPtr thisForm;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(String sClassName, String sAppName);
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public Dashboard()
        {

            InitializeComponent();
            int ramValue = GetRam();
            int cpuValue = GetCPU();
            bunifuCircleProgressbar1.Value = ramValue;
            bunifuProgressBar1.Value = ramValue;
            bunifuCircleProgressbar2.Value = cpuValue;
            bunifuProgressBar2.Value = cpuValue;
            UpdateProcesses();
            updateInfo.Start();
            processUpdate.Start();
            localIp.Text += GetLocalIP();
            externIp.Text += GetNetworkIP();
           
        }

        private void UpdateInfo_Tick(object sender, EventArgs e)
        {
            int ramValue = GetRam();
            int cpuValue = GetCPU();
            bunifuCircleProgressbar1.Value = ramValue;
            bunifuProgressBar1.Value = ramValue;
            bunifuCircleProgressbar2.Value = cpuValue;
            bunifuProgressBar2.Value = cpuValue;
        }
        private int GetRam()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                return Convert.ToInt32(percent);
            }
            return 0;
        }
        private int GetCPU()
        {
            float perfCounterValue = cpuCounter.NextValue();

            System.Threading.Thread.Sleep(450);

            perfCounterValue = cpuCounter.NextValue();

            return Convert.ToInt32(perfCounterValue);

        }
        private void BunifuCustomLabel2_Click(object sender, EventArgs e)
        {

        }
        private string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Erro de provedor!";
        }
        public void UpdateProcesses()
        {
            AutoCompleteStringCollection stringProcess = new AutoCompleteStringCollection();
            Process[] p = Process.GetProcesses();
            foreach (Process process in p)
            {
                stringProcess.Add(process.ProcessName);
            }

            processList.AutoCompleteCustomSource = stringProcess;
        }
        private void ProcessUpdate_Tick(object sender, EventArgs e)
        {
            UpdateProcesses();
        }
        private void KillProcesses(string name)
        {
            List<String> priorities = new List<String>();
            Process[] processes = Process.GetProcessesByName(name);
            foreach (Process p in processes)
            {
                p.Kill();
            }
            
        }

        private void ProcessList_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void BunifuFlatButton4_Click(object sender, EventArgs e)
        {
            KillProcesses(processList.Text);
            Audio a = new Audio();
            a.PlaySystemSound(SystemSounds.Beep);
        }

        private void BunifuFlatButton5_Click(object sender, EventArgs e)
        {
            Audio a = new Audio();
            var processes = Process.GetProcessesByName(processList.Text);
            List<String> paths = new List<String>();
            if (processes.Length > 0 && processes != null)
            {
                foreach (Process p in processes)
                {
                    if (!paths.Contains(p.MainModule.FileName))
                    {
                        try
                        {
                            Process.Start(p.MainModule.FileName);
                            paths.Add(p.MainModule.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            a.PlaySystemSound(SystemSounds.Exclamation);
                            return;
                        }
                    }
                }
                a.PlaySystemSound(SystemSounds.Beep);
            }
            
        }

        private void BunifuCustomLabel5_Click(object sender, EventArgs e)
        {
            Environment.Exit(100);
        }

        private void BunifuCustomLabel6_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        public enum HotKey
        {
            Alt = 0x0001,
            Control = 0x0002,
        }

        private void Dashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnregisterHotKey(thisForm, 1);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                KillProcesses(processList.Text);
                processList.Text = "";
                Audio a = new Audio();
                a.PlaySystemSound(SystemSounds.Beep);

            }
            base.WndProc(ref m);
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            thisForm = FindWindow(null, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            RegisterHotKey(thisForm, 1, (uint)HotKey.Alt, (uint)Keys.K);
        }
        public string GetNetworkIP()
        {
            string tempIp = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                tempIp = stream.ReadToEnd();
            }
            int first = tempIp.IndexOf("Address: ") + 9;
            int last = tempIp.IndexOf("</body>");
            tempIp = tempIp.Substring(first, last - first);
            return tempIp;
        }

        private void BunifuFlatButton2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Programa desenvolvido para o uso diário, é baseado no gerenciador de tarefas do Windows, entretanto possui algumas particularidades.", "Ajuda", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BunifuFlatButton3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Desenvolvido por: Eduardo Odelon Wagner, codigo fonte disponível em github.com/rexxzbr, feito em C# utilizando Bunifu Framework e .Net", "Sobre", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }
    }
}
