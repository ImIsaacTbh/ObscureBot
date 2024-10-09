using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ObscuraController
{
    public partial class Form1 : Form
    {
        public static bool isRunning = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            isRunning = !isRunning;
            if(isRunning)
            {
                btnStartStop.Text = "Stop";
                lblStatus.Text = "Running";
            }
            else
            {
                btnStartStop.Text = "Start";
                lblStatus.Text = "Stopped";
            }
        }
    }
}
