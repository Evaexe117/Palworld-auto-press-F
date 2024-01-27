using System;
using System.Drawing;
using System.Windows.Forms;

namespace PalWorldPressF
{
    partial class StatusForm
    {
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button closeButton;
        private Label instructionsLabel; // Nouvelle Label pour les instructions

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
            statusLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            statusLabel.Location = new Point(14, 10);
            statusLabel.Margin = new Padding(4, 0, 4, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(85, 20);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "En pause";
            // 
            // closeButton
            // 
            closeButton.Location = new Point(285, 43);
            closeButton.Margin = new Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(88, 27);
            closeButton.TabIndex = 1;
            closeButton.Text = "Fermer";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += CloseButton_Click;
            // 
            // instructionsLabel
            // 
            instructionsLabel.AutoSize = true;
            instructionsLabel.Location = new Point(14, 46);
            instructionsLabel.Margin = new Padding(4, 0, 4, 0);
            instructionsLabel.Name = "instructionsLabel";
            instructionsLabel.Size = new Size(325, 15);
            instructionsLabel.TabIndex = 1;
            instructionsLabel.Text = "Appuyez sur SHIFT+P";
            // 
            // StatusForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(386, 92);
            Controls.Add(statusLabel);
            Controls.Add(instructionsLabel);
            Controls.Add(closeButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 3, 4, 3);
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
            Program.isApplicationRunning = false; // Indique au thread de s'arrêter
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void UpdateStatus(bool isRunning)
        {
            // Mettez à jour le texte et la couleur de la Label pour indiquer le statut
            if (isRunning)
            {
                // Mettre à jour pour l'état "Marche"
                statusLabel.Text = "Marche";
                statusLabel.ForeColor = Color.Green; // Utilisez la couleur de votre choix
            }
            else
            {
                // Mettre à jour pour l'état "En pause"
                statusLabel.Text = "En pause";
                statusLabel.ForeColor = Color.Orange; // Utilisez la couleur de votre choix
            }
        }
    }
}
