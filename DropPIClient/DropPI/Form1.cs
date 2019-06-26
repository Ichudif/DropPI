using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace DropPI
{
    public partial class Form1 : Form
    {
        Form Mainform = null;
        UtilityLibrary.UtilityLibrary ul = new UtilityLibrary.UtilityLibrary();
        const int SERVER_PORT = 12345;

        private List<SynchronizingTask> Tasks = new List<SynchronizingTask>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Mainform.Deactivate -= Form1_Deactivate;
            folderBrowserDialog1.ShowDialog();
            int h = listBox1.Items.Add(folderBrowserDialog1.SelectedPath);

            DateTime dt = new DateTime();
            dt = DateTime.Now;
            dt.AddMinutes(3);
            SynchronizingTask st = new SynchronizingTask(folderBrowserDialog1.SelectedPath, dt, h);
            Tasks.Add(st);

            Mainform.Deactivate += Form1_Deactivate;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Mainform.Visible = false;
            Mainform.SendToBack();
        }

        private void Ul_KeyPressedEvent()
        {
            Mainform.Invoke((MethodInvoker)(() =>
            {
                Mainform.Visible = true;
                Mainform.TopMost = true;
                Mainform.BringToFront();
                Mainform.Activate();
            }));
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (Mainform == null)
            {
                Mainform = Form.ActiveForm;
                Mainform.ShowInTaskbar = false;
                ul.startlooking('d');
                ul.KeyPressedEvent += Ul_KeyPressedEvent;

                LogClass.listBox2 = listBox2;
            }
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            new Thread((() =>
            {
                Tasks.ForEach(task => task.CheckAndSyncFolders());
            })).Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox2.Visible == false)
            {
                listBox2.Visible = true;
                button5.Text = "Back";
            }
            else
            {
                listBox2.Visible = false;
                button5.Text = "Show Logs";
            }
        }
    }
}
