using System;
using System.Drawing;
using System.Windows.Forms;

namespace PalWorldPressF
{
    partial class StatusForm
    {
        private Label statusLabel;
        private Button closeButton;
        private Label instructionsLabel; // New label for instructions

        private void InitializeComponent()
        {
            statusLabel = new Label();
            closeButton = new Button();
            instructionsLabel = new Label();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point);
            statusLabel.Location = new Point(14, 10);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(76, 20);
            statusLabel.Text = "Paused";
            // 
            // closeButton
            // 
            closeButton.Location = new Point(285, 43);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(88, 27);
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += CloseButton_Click;
            // 
            // instructionsLabel
            // 
            instructionsLabel.AutoSize = true;
            instructionsLabel.Location = new Point(14, 46);
            instructionsLabel.Name = "instructionsLabel";
            instructionsLabel.Size = new Size(180, 15);
            instructionsLabel.Text = "Press SHIFT+P to toggle";
            // 
            // StatusForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(386, 82);
            Controls.Add(statusLabel);
            Controls.Add(instructionsLabel);
            Controls.Add(closeButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StatusForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PalWorld - Press F - By Eva";
            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Program.isApplicationRunning = false; // Signal the thread to stop
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void UpdateStatus(bool isRunning)
        {
            // Update the label text and color to reflect the status
            if (isRunning)
            {
                // Update for "Running" state
                statusLabel.Text = "Running";
                statusLabel.ForeColor = Color.Green; // Choose your color
            }
            else
            {
                // Update for "Paused" state
                statusLabel.Text = "Paused";
                statusLabel.ForeColor = Color.Orange; // Choose your color
            }
        }
    }
}
