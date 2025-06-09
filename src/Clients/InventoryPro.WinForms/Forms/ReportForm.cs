using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Reports form
    /// </summary>
    public partial class ReportForm : Form
        {
        private readonly ILogger<ReportForm> _logger;
        private readonly IApiService _apiService;

        public ReportForm(ILogger<ReportForm> logger, IApiService apiService)
            {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            InitializeComponent();
            }

        private void InitializeComponent()
            {
            this.Text = "Reports";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // TODO: Implement reports UI
            var label = new Label
                {
                Text = "Reports - Coming Soon",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F)
                };

            this.Controls.Add(label);
            }
        }
    }