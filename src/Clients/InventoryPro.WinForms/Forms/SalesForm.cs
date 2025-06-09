using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Sales management form
    /// </summary>
    public partial class SalesForm : Form
        {
        private readonly ILogger<SalesForm> _logger;
        private readonly IApiService _apiService;

        public SalesForm(ILogger<SalesForm> logger, IApiService apiService)
            {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            InitializeComponent();
            }

        private void InitializeComponent()
            {
            this.Text = "Sales Management";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // TODO: Implement sales management UI
            var label = new Label
                {
                Text = "Sales Management - Coming Soon",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F)
                };

            this.Controls.Add(label);
            }
        }
    }