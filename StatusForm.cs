namespace PalWorldPressF
{
    public partial class StatusForm : Form
    {
        public StatusForm()
        {
            InitializeComponent();
            Text += $" - v{Application.ProductVersion}";
        }

        private void StatusForm_Load(object sender, EventArgs e)
        {

        }
    }
}
