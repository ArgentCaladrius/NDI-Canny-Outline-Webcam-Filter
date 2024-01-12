using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CannyFilter
{
    public partial class CannyForm : Form
    {
        public CannyForm()
        {
            InitializeComponent();
            this.OnOpened();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.CannyWebcam();
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            this.threshold1 = scrlLowThreshold.Value;
        }

        private void hScrollBar2_ValueChanged(object sender, EventArgs e)
        {
            this.threshold2 = scrlHighThreshold.Value;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.StopCanny();
        }

        private void CannyForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(webcamThread != null && webcamThread.IsAlive) this.webcamThread.Abort();
            Application.Exit();
        }

        private void numLCR_ValueChanged(object sender, EventArgs e) => this.AdjustChromaThreshold(false, 0, (int) numLCR.Value);
        private void numLCG_ValueChanged(object sender, EventArgs e) => this.AdjustChromaThreshold(false, 1, (int) numLCG.Value);
        private void numLCB_ValueChanged(object sender, EventArgs e) => this.AdjustChromaThreshold(false, 2, (int) numLCB.Value);
        private void numHCR_ValueChanged(object sender, EventArgs e) => this.AdjustChromaThreshold(true, 0, (int) numHCR.Value);
        private void numHCG_ValueChanged(object sender, EventArgs e) => this.AdjustChromaThreshold(true, 1, (int) numHCG.Value);
        private void numHCB_ValueChanged(object sender, EventArgs e) => this.AdjustChromaThreshold(true, 2, (int) numHCB.Value);

        private void CannyForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            this.OpenProperties();
        }
    }
}
