namespace PalWorldPressF
{
    public partial class StatusForm : Form
    {
        public StatusForm()
        {
            InitializeComponent();
            Text += $" - v{Application.ProductVersion}";
            // Reflect the interval loaded from settings.json (keys are applied in
            // PopulateKeyDropdowns); Program guarantees it is within the control's range
            intervalNumeric.Value = Program.spamIntervalMs;
        }

        private void StatusForm_Load(object sender, EventArgs e)
        {

        }
    }
}
