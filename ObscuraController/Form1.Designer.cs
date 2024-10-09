namespace ObscuraController
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStartStop = new Button();
            lblTitle = new Label();
            label1 = new Label();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // btnStartStop
            // 
            btnStartStop.Location = new Point(14, 360);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(183, 78);
            btnStartStop.TabIndex = 0;
            btnStartStop.Text = "Start";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Eco Sans Mono", 24.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(14, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(323, 38);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "ObscureController";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(14, 336);
            label1.Name = "label1";
            label1.Size = new Size(55, 21);
            label1.TabIndex = 2;
            label1.Text = "Status:";
            // 
            // label2
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 12F);
            lblStatus.Location = new Point(75, 336);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 21);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Stopped";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblStatus);
            Controls.Add(label1);
            Controls.Add(lblTitle);
            Controls.Add(btnStartStop);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStartStop;
        private Label lblTitle;
        private Label label1;
        private Label lblStatus;
    }
}