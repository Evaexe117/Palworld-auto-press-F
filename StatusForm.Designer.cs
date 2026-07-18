using System;
using System.Drawing;
using System.Windows.Forms;

namespace PalWorldPressF
{
    partial class StatusForm
    {
        private Label statusLabel;
        private Button closeButton;
        private Label instructionsLabel;
        private TextBox debugTextBox;
        private Label pressKeyLabel;
        private ComboBox pressKeyComboBox;
        private Label toggleKeyLabel;
        private ComboBox toggleKeyComboBox;
        private Label intervalLabel;
        private NumericUpDown intervalNumeric;
        private CheckBox cancelCheckBox;
        private ComboBox cancelKeyComboBox;
        private Label tipLabel;

        private void InitializeComponent()
        {
            statusLabel = new Label();
            closeButton = new Button();
            instructionsLabel = new Label();
            debugTextBox = new TextBox();
            pressKeyLabel = new Label();
            pressKeyComboBox = new ComboBox();
            toggleKeyLabel = new Label();
            toggleKeyComboBox = new ComboBox();
            intervalLabel = new Label();
            intervalNumeric = new NumericUpDown();
            cancelCheckBox = new CheckBox();
            cancelKeyComboBox = new ComboBox();
            tipLabel = new Label();
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
            instructionsLabel.Text = "ALT+J: toggle auto-press F";
            //
            // debugTextBox
            //
            debugTextBox.Location = new Point(14, 78);
            debugTextBox.Multiline = false;
            debugTextBox.Name = "debugTextBox";
            debugTextBox.ScrollBars = ScrollBars.Vertical;
            debugTextBox.Size = new Size(358, 20);
            //
            // pressKeyLabel
            //
            pressKeyLabel.AutoSize = true;
            pressKeyLabel.Location = new Point(14, 112);
            pressKeyLabel.Name = "pressKeyLabel";
            pressKeyLabel.Size = new Size(96, 15);
            pressKeyLabel.Text = "Pressed/held key:";
            //
            // pressKeyComboBox
            //
            pressKeyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            pressKeyComboBox.FormattingEnabled = true;
            pressKeyComboBox.Location = new Point(140, 108);
            pressKeyComboBox.Name = "pressKeyComboBox";
            pressKeyComboBox.Size = new Size(80, 23);
            pressKeyComboBox.SelectedIndexChanged += PressKeyComboBox_SelectedIndexChanged;
            //
            // toggleKeyLabel
            //
            toggleKeyLabel.AutoSize = true;
            toggleKeyLabel.Location = new Point(14, 142);
            toggleKeyLabel.Name = "toggleKeyLabel";
            toggleKeyLabel.Size = new Size(116, 15);
            toggleKeyLabel.Text = "ALT toggle letter:";
            //
            // toggleKeyComboBox
            //
            toggleKeyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            toggleKeyComboBox.FormattingEnabled = true;
            toggleKeyComboBox.Location = new Point(140, 138);
            toggleKeyComboBox.Name = "toggleKeyComboBox";
            toggleKeyComboBox.Size = new Size(80, 23);
            toggleKeyComboBox.SelectedIndexChanged += ToggleKeyComboBox_SelectedIndexChanged;
            //
            // intervalLabel
            //
            intervalLabel.AutoSize = true;
            intervalLabel.Location = new Point(14, 172);
            intervalLabel.Name = "intervalLabel";
            intervalLabel.Size = new Size(120, 15);
            intervalLabel.Text = "Spam interval (ms):";
            //
            // intervalNumeric
            //
            intervalNumeric.Increment = 10;
            intervalNumeric.Location = new Point(140, 168);
            intervalNumeric.Maximum = 5000;
            intervalNumeric.Minimum = 10;
            intervalNumeric.Name = "intervalNumeric";
            intervalNumeric.Size = new Size(80, 23);
            intervalNumeric.Value = 100;
            intervalNumeric.ValueChanged += IntervalNumeric_ValueChanged;
            //
            // cancelCheckBox
            //
            cancelCheckBox.AutoSize = true;
            cancelCheckBox.Location = new Point(14, 200);
            cancelCheckBox.Name = "cancelCheckBox";
            cancelCheckBox.Size = new Size(120, 19);
            cancelCheckBox.Text = "Cancel on key press:";
            cancelCheckBox.CheckedChanged += CancelCheckBox_CheckedChanged;
            //
            // cancelKeyComboBox
            //
            cancelKeyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            cancelKeyComboBox.FormattingEnabled = true;
            cancelKeyComboBox.Location = new Point(158, 198);
            cancelKeyComboBox.Name = "cancelKeyComboBox";
            cancelKeyComboBox.Size = new Size(80, 23);
            cancelKeyComboBox.SelectedIndexChanged += CancelKeyComboBox_SelectedIndexChanged;
            //
            // tipLabel
            //
            tipLabel.AutoSize = false;
            tipLabel.ForeColor = Color.DimGray;
            tipLabel.Location = new Point(14, 228);
            tipLabel.Name = "tipLabel";
            tipLabel.Size = new Size(358, 32);
            tipLabel.Text = "Tip: disable \"Single button press for hold interactions\" in Palworld's options, so the selected auto-press key also covers hold interactions.";
            //
            // StatusForm
            //
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(386, 266);
            Controls.Add(statusLabel);
            Controls.Add(instructionsLabel);
            Controls.Add(closeButton);
            Controls.Add(debugTextBox);
            Controls.Add(pressKeyLabel);
            Controls.Add(pressKeyComboBox);
            Controls.Add(toggleKeyLabel);
            Controls.Add(toggleKeyComboBox);
            Controls.Add(intervalLabel);
            Controls.Add(intervalNumeric);
            Controls.Add(cancelCheckBox);
            Controls.Add(cancelKeyComboBox);
            Controls.Add(tipLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StatusForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PalWorld - Auto Press Key in background";
            ResumeLayout(false);
            PerformLayout();

            PopulateKeyDropdowns();
            UpdateInstructions();
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

        private void IntervalNumeric_ValueChanged(object sender, EventArgs e)
        {
            Program.spamIntervalMs = (int)intervalNumeric.Value;
            Program.SaveSettings();
        }

        private void PressKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.SetPressedKey((Keys)pressKeyComboBox.SelectedItem);
            UpdateInstructions();
            Program.SaveSettings();
        }

        private void ToggleKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.SetToggleKey((Keys)toggleKeyComboBox.SelectedItem);
            UpdateInstructions();
            Program.SaveSettings();
        }

        private void CancelCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Program.cancelOnKeyEnabled = cancelCheckBox.Checked;
            UpdateInstructions();
            Program.SaveSettings();
        }

        private void CancelKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.cancelKeyCode = (int)(Keys)cancelKeyComboBox.SelectedItem;
            UpdateInstructions();
            Program.SaveSettings();
        }

        private void PopulateKeyDropdowns()
        {
            cancelKeyComboBox.Items.Add(Keys.Escape);
            cancelKeyComboBox.Items.Add(Keys.Tab);
            cancelKeyComboBox.Items.Add(Keys.Space);
            cancelKeyComboBox.Items.Add(Keys.Enter);
            for (Keys key = Keys.A; key <= Keys.Z; key++)
            {
                pressKeyComboBox.Items.Add(key);
                toggleKeyComboBox.Items.Add(key);
                cancelKeyComboBox.Items.Add(key);
            }

            pressKeyComboBox.SelectedItem = (Keys)Program.pressedKeyCode;
            toggleKeyComboBox.SelectedItem = (Keys)Program.toggleKeyCode;
            cancelKeyComboBox.SelectedItem = (Keys)Program.cancelKeyCode;
            cancelCheckBox.Checked = Program.cancelOnKeyEnabled;
        }

        public void UpdateInstructions()
        {
            string cancelPart = Program.cancelOnKeyEnabled
                ? $", {(Keys)Program.cancelKeyCode} cancels it"
                : "";
            instructionsLabel.Text = $"ALT+{(Keys)Program.toggleKeyCode}: toggle auto-press {(Keys)Program.pressedKeyCode}{cancelPart}";
        }

        public void UpdateStatus(bool isSpamming)
        {
            // Update the label text and color to reflect the status
            if (isSpamming)
            {
                statusLabel.Text = "Running";
                statusLabel.ForeColor = Color.Green;
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
