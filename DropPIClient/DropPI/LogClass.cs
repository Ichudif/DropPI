using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropPI
{
    static class LogClass
    {
        public static ListBox listBox2 { get; set; }

        public static void Log(string text)
        {
            string FormattedText = "[" + DateTime.Now.ToString("hh:mm:ss") + "]";
            FormattedText += " " + text;

            listBox2.Invoke((MethodInvoker)(() =>
            {
                listBox2.Items.Add(FormattedText);
                listBox2.TopIndex = listBox2.Items.Count - 1;
            }));
        }
    }
}
