using Microsoft.Extensions.Logging;
using InventoryPro.WinForms.Services;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Forms
    {
    /// <summary>
    /// Customer management form
    /// </summary>
    public partial class CustomerForm : Form
        {
        private readonly ILogger<CustomerForm> _logger;
        private readonly IApiService _apiService;

        // Add similar implementation as ProductForm but for customers

        public CustomerForm(ILogger<CustomerForm> logger, IApiService apiService)
            {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            //InitializeComponent();
            }

        //private void InitializeComponent()
        //    {
        //    this.Text = "Customer Management";
        //    this.Size = new Size(800, 600);
        //    this.StartPosition = FormStartPosition.CenterScreen;

        //    // TODO: Implement customer management UI
        //    var label = new Label
        //        {
        //        Text = "Customer Management - Coming Soon",
        //        Dock = DockStyle.Fill,
        //        TextAlign = ContentAlignment.MiddleCenter,
        //        Font = new Font("Segoe UI", 16F)
        //        };

        //    this.Controls.Add(label);
        //    }
        }
    }