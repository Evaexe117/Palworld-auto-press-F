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
        private TextBox debugTextBox;
        private Label intervalLabel;
        private NumericUpDown intervalNumeric;

        private void InitializeComponent()
        {
            statusLabel = new Label();
            closeButton = new Button();
            instructionsLabel = new Label();
            debugTextBox = new TextBox();
            intervalLabel = new Label();
            intervalNumeric = new NumericUpDown();
            debugTextBox.Location = new Point(14, 70);
            debugTextBox.Size = new Size(358, 20);
            debugTextBox.Multiline = false;
            debugTextBox.ScrollBars = ScrollBars.Vertical;

            Controls.Add(debugTextBox);
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
            instructionsLabel.Text = "ALT+J: spam F  |  ALT+H: hold F";
            //
            // intervalLabel
            //
            intervalLabel.AutoSize = true;
            intervalLabel.Location = new Point(14, 100);
            intervalLabel.Name = "intervalLabel";
            intervalLabel.Size = new Size(120, 15);
            intervalLabel.Text = "Spam interval (ms):";
            //
            // intervalNumeric
            //
            intervalNumeric.Location = new Point(140, 96);
            intervalNumeric.Name = "intervalNumeric";
            intervalNumeric.Size = new Size(80, 23);
            intervalNumeric.Minimum = 10;
            intervalNumeric.Maximum = 5000;
            intervalNumeric.Increment = 10;
            intervalNumeric.Value = 100;
            intervalNumeric.ValueChanged += IntervalNumeric_ValueChanged;
            //
            // StatusForm
            //
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(386, 132);
            Controls.Add(statusLabel);
            Controls.Add(instructionsLabel);
            Controls.Add(closeButton);
            Controls.Add(intervalLabel);
            Controls.Add(intervalNumeric);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StatusForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PalWorld - Auto Press F in background ";
            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Program.ReleaseHoldIfActive(); // Don't leave 'F' logically held down in the game
            Program.isApplicationRunning = false; // Signal the thread to stop
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void IntervalNumeric_ValueChanged(object sender, EventArgs e)
        {
            Program.spamIntervalMs = (int)intervalNumeric.Value;
        }

        public void UpdateStatus(bool isSpamming, bool isHolding)
        {
            // Update the label text and color to reflect the status
            if (isSpamming)
            {
                statusLabel.Text = "Running (spam)";
                statusLabel.ForeColor = Color.Green;
            }
            else if (isHolding)
            {
                statusLabel.Text = "Running (hold)";
                statusLabel.ForeColor = Color.Blue;
            }
            else
            {
                // Update for "Paused" state
                statusLabel.Text = "Paused";
                statusLabel.ForeColor = Color.Orange;
            }
        }

        public void AppendDebugMessage(string message)
        {

            if (debugTextBox.InvokeRequired)
            {
                debugTextBox.Invoke(new Action<string>(AppendDebugMessage), new object[] { message });
            }
            else
            {
                debugTextBox.AppendText(message + Environment.NewLine);
            }
        }
    }
}
