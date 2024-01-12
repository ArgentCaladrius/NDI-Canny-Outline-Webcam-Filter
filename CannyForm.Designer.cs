using System;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NewTek.NDI;
using AForge.Video.DirectShow;

namespace CannyFilter
{
    partial class CannyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        
        String failoverName = String.Format("{0} (NDIlib Send Example)", System.Net.Dns.GetHostName());

        // OpenCV 
        VideoCaptureDevice videoCaptureDevice = new VideoCaptureDevice(new FilterInfoCollection(FilterCategory.VideoInputDevice)[0].MonikerString);
        VideoCapture videoCapture;
        Thread webcamThread;
        bool webcamEnabled = false;
        int threshold1 = 0;
        int threshold2 = 0;
        int frames = 0;
        int[] lowChroma = new int[] { 0, 0, 0 };
        int[] highChroma = new int[] { 0, 0, 0 };
        
        public void OnOpened()
        {
            videoCapture = new VideoCapture(0, VideoCapture.API.DShow);
        }

        public void OpenProperties()
        {
            videoCaptureDevice.DisplayPropertyPage(IntPtr.Zero);
        }

        public void CannyWebcam()
        {
            btnStart.Enabled = false;
            webcamThread = new Thread(new ThreadStart(WebcamThread));
            webcamThread.Start();
            btnStop.Enabled = true;
        }

        public void AdjustChromaThreshold(bool high, int index, int newValue)
        {
            if (high) highChroma[index] = newValue;
            else lowChroma[index] = newValue;

            Console.WriteLine($"{(high ? "High" : "Low")} chroma threshold: {string.Join(", ", (high ? highChroma : lowChroma))}");
        }

        public Mat GetMask(Mat frame)
        {
            // Get mask
            Mat mask = new Mat();
            ScalarArray low = new ScalarArray(new MCvScalar(lowChroma[0], lowChroma[1], lowChroma[2]));
            ScalarArray high = new ScalarArray(new MCvScalar(highChroma[0], highChroma[1], highChroma[2]));
            CvInvoke.InRange(frame.ToImage<Hsv, Byte>(), low, high, mask);
            return mask;
        }

        public void WebcamThread()
        {
            webcamEnabled = true;
            using (Sender sender = new Sender("CannyFeed", true, false, null, failoverName))
            {
                using (VideoFrame videoFrame = new VideoFrame(750, 650, (750 / 650), 30000, 1001))
                {
                    using (Bitmap bmp = new Bitmap(videoFrame.Width, videoFrame.Height, videoFrame.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, videoFrame.BufferPtr))
                    using (Graphics graphics = Graphics.FromImage(bmp))
                    {
                        while (webcamEnabled)
                        {
                            Mat frame = frame = videoCapture.QueryFrame();
                            Mat mask = GetMask(frame);
                            Mat cannyOutput = new Mat(frame.Size, DepthType.Cv8U, 3);

                            // Perform canny outline
                            CvInvoke.Canny(frame, cannyOutput, threshold1, threshold2);
                            cannyOutput = cannyOutput.ToImage<Bgr, byte>().Mat;
                            // Mask out background with green
                            cannyOutput.SetTo(new ScalarArray(new MCvScalar(0, 255, 0)), mask);

                            this.WebcamDisplay.Image = cannyOutput;
                            
                            if (sender.GetConnections(100) < 1)
                            {
                                Console.WriteLine("No connection");
                                Thread.Sleep(1);
                                continue;
                            }
                            else
                            {
                                graphics.Clear(Color.Black);
                                graphics.DrawImage(cannyOutput.ToBitmap(), new System.Drawing.Point(0, 0));
                                sender.Send(videoFrame);
                                Console.WriteLine("Sent frame " + ++frames);
                            }

                            Thread.Sleep(16);
                        }
                    }
                }
            }
            Thread.Sleep(10);
        }

        void StopCanny()
        {
            webcamEnabled = false;
            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CannyForm));
            this.WebcamDisplay = new Emgu.CV.UI.ImageBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.scrlLowThreshold = new System.Windows.Forms.HScrollBar();
            this.scrlHighThreshold = new System.Windows.Forms.HScrollBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.numLCR = new System.Windows.Forms.NumericUpDown();
            this.numLCG = new System.Windows.Forms.NumericUpDown();
            this.numLCB = new System.Windows.Forms.NumericUpDown();
            this.numHCB = new System.Windows.Forms.NumericUpDown();
            this.numHCG = new System.Windows.Forms.NumericUpDown();
            this.numHCR = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.WebcamDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLCR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLCG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLCB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHCB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHCG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHCR)).BeginInit();
            this.SuspendLayout();
            // 
            // WebcamDisplay
            // 
            this.WebcamDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WebcamDisplay.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            this.WebcamDisplay.Location = new System.Drawing.Point(12, 12);
            this.WebcamDisplay.Name = "WebcamDisplay";
            this.WebcamDisplay.Size = new System.Drawing.Size(534, 476);
            this.WebcamDisplay.TabIndex = 2;
            this.WebcamDisplay.TabStop = false;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(471, 508);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // scrlLowThreshold
            // 
            this.scrlLowThreshold.Location = new System.Drawing.Point(12, 508);
            this.scrlLowThreshold.Maximum = 255;
            this.scrlLowThreshold.Name = "scrlLowThreshold";
            this.scrlLowThreshold.Size = new System.Drawing.Size(253, 22);
            this.scrlLowThreshold.TabIndex = 5;
            this.scrlLowThreshold.ValueChanged += new System.EventHandler(this.hScrollBar1_ValueChanged);
            // 
            // scrlHighThreshold
            // 
            this.scrlHighThreshold.Location = new System.Drawing.Point(12, 546);
            this.scrlHighThreshold.Maximum = 255;
            this.scrlHighThreshold.Name = "scrlHighThreshold";
            this.scrlHighThreshold.Size = new System.Drawing.Size(253, 22);
            this.scrlHighThreshold.TabIndex = 6;
            this.scrlHighThreshold.ValueChanged += new System.EventHandler(this.hScrollBar2_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 495);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Low Canny Threshold";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 533);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "High Canny Threshold";
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(471, 547);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 9;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // numLCR
            // 
            this.numLCR.Location = new System.Drawing.Point(270, 510);
            this.numLCR.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numLCR.Name = "numLCR";
            this.numLCR.Size = new System.Drawing.Size(61, 20);
            this.numLCR.TabIndex = 10;
            this.numLCR.ValueChanged += new System.EventHandler(this.numLCR_ValueChanged);
            // 
            // numLCG
            // 
            this.numLCG.Location = new System.Drawing.Point(337, 510);
            this.numLCG.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numLCG.Name = "numLCG";
            this.numLCG.Size = new System.Drawing.Size(61, 20);
            this.numLCG.TabIndex = 11;
            this.numLCG.ValueChanged += new System.EventHandler(this.numLCG_ValueChanged);
            // 
            // numLCB
            // 
            this.numLCB.Location = new System.Drawing.Point(404, 510);
            this.numLCB.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numLCB.Name = "numLCB";
            this.numLCB.Size = new System.Drawing.Size(61, 20);
            this.numLCB.TabIndex = 12;
            this.numLCB.ValueChanged += new System.EventHandler(this.numLCB_ValueChanged);
            // 
            // numHCB
            // 
            this.numHCB.Location = new System.Drawing.Point(404, 549);
            this.numHCB.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numHCB.Name = "numHCB";
            this.numHCB.Size = new System.Drawing.Size(61, 20);
            this.numHCB.TabIndex = 15;
            this.numHCB.ValueChanged += new System.EventHandler(this.numHCB_ValueChanged);
            // 
            // numHCG
            // 
            this.numHCG.Location = new System.Drawing.Point(337, 549);
            this.numHCG.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numHCG.Name = "numHCG";
            this.numHCG.Size = new System.Drawing.Size(61, 20);
            this.numHCG.TabIndex = 14;
            this.numHCG.ValueChanged += new System.EventHandler(this.numHCG_ValueChanged);
            // 
            // numHCR
            // 
            this.numHCR.Location = new System.Drawing.Point(270, 549);
            this.numHCR.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numHCR.Name = "numHCR";
            this.numHCR.Size = new System.Drawing.Size(61, 20);
            this.numHCR.TabIndex = 13;
            this.numHCR.ValueChanged += new System.EventHandler(this.numHCR_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(267, 533);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(147, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "High Chroma Threshold (RGB";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(267, 495);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(148, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Low Chroma Threshold (RGB)";
            // 
            // CannyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(561, 572);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numHCB);
            this.Controls.Add(this.numHCG);
            this.Controls.Add(this.numHCR);
            this.Controls.Add(this.numLCB);
            this.Controls.Add(this.numLCG);
            this.Controls.Add(this.numLCR);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.scrlHighThreshold);
            this.Controls.Add(this.scrlLowThreshold);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.WebcamDisplay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CannyForm";
            this.Text = "Canny Webcam Filter";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.CannyForm_HelpButtonClicked);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CannyForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.WebcamDisplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLCR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLCG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLCB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHCB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHCG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHCR)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Emgu.CV.UI.ImageBox WebcamDisplay;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.HScrollBar scrlLowThreshold;
        private System.Windows.Forms.HScrollBar scrlHighThreshold;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.NumericUpDown numLCR;
        private System.Windows.Forms.NumericUpDown numLCG;
        private System.Windows.Forms.NumericUpDown numLCB;
        private System.Windows.Forms.NumericUpDown numHCB;
        private System.Windows.Forms.NumericUpDown numHCG;
        private System.Windows.Forms.NumericUpDown numHCR;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}

