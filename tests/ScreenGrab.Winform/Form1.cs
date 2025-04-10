using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenGrab.Winform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool isAuxiliary = true; // Open auxiliary lines
            ScreenGrabber.OnCaptured = bitmap => _ = bitmap;
            ScreenGrabber.Capture(isAuxiliary);   // 这里报错，未将对象引用设置到对象实例
        }
    }
}
