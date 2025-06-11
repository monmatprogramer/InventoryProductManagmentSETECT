# InventoryPro - Microservices-based Inventory Management System

A professional Windows Forms application built with .NET 9.0 and microservices architecture for inventory management.

## Architecture Overview

This project uses a microservices architecture with the following components:

- **Windows Forms Client**: Desktop application for users
- **API Gateway**: Ocelot-based gateway for routing and authentication
- **Auth Service**: JWT-based authentication service
- **Product Service**: Product and inventory management
- **Sales Service**: Sales and customer management
- **Report Service**: Reporting and analytics

## Prerequisites

- Visual Studio 2022 or later
- .NET 9.0 SDK
- SQL Server (Express or Developer Edition)
- Git

## Project Structure

# InventoryPro - Microservices-based Inventory Management System

A professional Windows Forms application built with .NET 9.0 and microservices architecture for comprehensive inventory management.

## 🏗️ Architecture Overview

```
┌─────────────────┐
│ Windows Forms   │
│    Client       │
└────────┬────────┘
         │
    ┌────▼─────┐
    │   API    │
    │ Gateway  │ (Port 5000)
    └────┬─────┘
         │
    ┌────┴─────────────┬───────────────┬─────────────────┐
    │                  │               │                 │
┌───▼────┐     ┌──────▼─────┐  ┌─────▼──────┐  ┌──────▼─────┐
│  Auth  │     │  Product   │  │   Sales    │  │  Report    │
│Service │     │  Service   │  │  Service   │  │  Service   │
│(5041)  │     │  (5089)    │  │  (5282)    │  │  (5179)    │
└────────┘     └────────────┘  └────────────┘  └────────────┘
    │                │                │                │
    └────────────────┴────────────────┴────────────────┘
                            │
                     ┌──────▼──────┐
                     │ SQL Server  │
                     │  Databases  │
                     └─────────────┘
```

## 📋 Prerequisites

- **Visual Studio 2022** or later (with .NET desktop development workload)
- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (Express or Developer Edition) - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Git** - [Download](https://git-scm.com/downloads)
- **Postman** (for API testing) - [Download](https://www.postman.com/downloads/)

## 🚀 Quick Start Guide

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/InventoryPro.git
cd InventoryPro
```

### 2. Setup Databases

Run the following SQL script to create all required databases:

```sql
-- Create Auth Service Database
CREATE DATABASE InventoryPro_AuthDB;
GO

-- Create Product Service Database
CREATE DATABASE InventoryPro_ProductDB;
GO

-- Create Sales Service Database
CREATE DATABASE InventoryPro_SalesDB;
GO
```

### 3. Update Connection Strings

Update the connection strings in each service's `appsettings.json` file if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=InventoryPro_[ServiceName]DB;Integrated Security=true;TrustServerCertificate=True;"
}
```

### 4. Run Database Migrations

Open a terminal in the solution directory and run:

```bash
# Auth Service
cd src/Services/InventoryPro.AuthService
dotnet ef database update

# Product Service
cd ../InventoryPro.ProductService
dotnet ef database update

# Sales Service
cd ../InventoryPro.SalesService
dotnet ef database update

cd ../../..
```

### 5. Start All Services

You can start all services using the provided PowerShell script or manually:

#### Option A: Using PowerShell Script (Recommended)

Create a file named `start-all-services.ps1`:

```powershell
Write-Host "Starting InventoryPro Microservices..." -ForegroundColor Green

# Start Auth Service
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.AuthService; dotnet run"
Start-Sleep -Seconds 2

# Start Product Service
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.ProductService; dotnet run"
Start-Sleep -Seconds 2

# Start Sales Service
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.SalesService; dotnet run"
Start-Sleep -Seconds 2

# Start Report Service
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.ReportService; dotnet run"
Start-Sleep -Seconds 2

# Start API Gateway
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Gateway/InventoryPro.Gateway; dotnet run"

Write-Host "All services started!" -ForegroundColor Green
Write-Host "Gateway URL: http://localhost:5000" -ForegroundColor Yellow
```

Run it:

```powershell
.\start-all-services.ps1
```

#### Option B: Manual Start

Open 5 terminal windows and run each service:

```bash
# Terminal 1 - Auth Service
cd src/Services/InventoryPro.AuthService
dotnet run

# Terminal 2 - Product Service
cd src/Services/InventoryPro.ProductService
dotnet run

# Terminal 3 - Sales Service
cd src/Services/InventoryPro.SalesService
dotnet run

# Terminal 4 - Report Service
cd src/Services/InventoryPro.ReportService
dotnet run

# Terminal 5 - API Gateway
cd src/Gateway/InventoryPro.Gateway
dotnet run
```

### 6. Run the Windows Forms Client

Open Visual Studio and:

1. Open the solution file `InventoryPro.sln`
2. Set `InventoryPro.WinForms` as the startup project
3. Press F5 or click Start

## 📱 Using the Application

### Default Login Credentials

- **Username:** admin
- **Password:** admin123
- **Role:** Admin (full access)

### Main Features

1. **Dashboard**

   - Real-time sales statistics
   - Low stock alerts
   - Recent activities
   - Quick access to all modules

2. **Product Management**

   - Add, edit, delete products
   - Track inventory levels
   - Set minimum stock alerts
   - Categorize products

3. **Customer Management**

   - Maintain customer database
   - View purchase history
   - Track customer statistics

4. **Sales (Point of Sale)**

   - Process sales transactions
   - Multiple payment methods
   - Real-time stock updates
   - Generate receipts

5. **Reports**
   - Sales reports
   - Inventory reports
   - Financial reports
   - Export to PDF/Excel/CSV

## 🧪 Testing with Postman

### Import Postman Collection

Create a file named `InventoryPro.postman_collection.json`:

```json
{
  "info": {
    "name": "InventoryPro API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "header": [
              {
                "key": "Content-Type",
                "value": "application/json"
              }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"username\": \"admin\",\n  \"password\": \"admin123\"\n}"
            },
            "url": {
              "raw": "http://localhost:5000/auth/login",
              "protocol": "http",
              "host": ["localhost"],
              "port": "5000",
              "path": ["auth", "login"]
            }
          }
        }
      ]
    },
    {
      "name": "Products",
      "item": [
        {
          "name": "Get Products",
          "request": {
            "method": "GET",
            "header": [
              {
                "key": "Authorization",
                "value": "Bearer {{token}}"
              }
            ],
            "url": {
              "raw": "http://localhost:5000/products",
              "protocol": "http",
              "host": ["localhost"],
              "port": "5000",
              "path": ["products"]
            }
          }
        }
      ]
    }
  ],
  "variable": [
    {
      "key": "token",
      "value": ""
    }
  ]
}
```

### Testing Steps

1. Import the collection into Postman
2. Test login endpoint to get JWT token
3. Copy the token and set it in the collection variables
4. Test other endpoints with authorization

## 🗂️ Project Structure

```
InventoryPro/
├── src/
│   ├── Clients/
│   │   └── InventoryPro.WinForms/     # Windows Forms client application
│   │       ├── Forms/                  # UI Forms
│   │       ├── Services/               # Service interfaces and implementations
│   │       └── Program.cs              # Application entry point
│   ├── Gateway/
│   │   └── InventoryPro.Gateway/       # Ocelot API Gateway
│   ├── Services/
│   │   ├── InventoryPro.AuthService/   # Authentication service
│   │   ├── InventoryPro.ProductService/ # Product management service
│   │   ├── InventoryPro.SalesService/  # Sales management service
│   │   └── InventoryPro.ReportService/ # Reporting service
│   └── Shared/
│       └── InventoryPro.Shared/        # Shared DTOs and models
├── docs/                               # Documentation
├── scripts/                            # Utility scripts
└── README.md
```

## 🔧 Configuration

### Service Ports

| Service         | Port | URL                   |
| --------------- | ---- | --------------------- |
| API Gateway     | 5000 | http://localhost:5000 |
| Auth Service    | 5041 | http://localhost:5041 |
| Product Service | 5089 | http://localhost:5089 |
| Sales Service   | 5282 | http://localhost:5282 |
| Report Service  | 5179 | http://localhost:5179 |

### JWT Configuration

All services use the same JWT settings for token validation:

```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "InventoryPro",
    "Audience": "InventoryProUsers",
    "ExpirationInHours": 24
  }
}
```

## 🚢 Deployment

### Development Environment

The current setup is configured for development. All services run on localhost with different ports.

### Production Deployment

For production deployment:

1. **Update connection strings** to point to production SQL Server
2. **Change JWT secret** to a secure, randomly generated key
3. **Enable HTTPS** in all services
4. **Use environment variables** for sensitive configuration
5. **Implement proper logging** with centralized log management
6. **Add health checks** to all services
7. **Use container orchestration** (Kubernetes/Docker Swarm)

### Docker Support (Coming Soon)

Docker files and docker-compose configuration will be added for easier deployment.

## 🐛 Troubleshooting

### Common Issues

1. **Services won't start**

   - Check if all required ports are available
   - Verify SQL Server is running
   - Ensure all NuGet packages are restored

2. **Database connection errors**

   - Verify SQL Server instance name
   - Check Windows Authentication is enabled
   - Ensure databases are created

3. **Authentication failures**

   - Verify JWT configuration matches across all services
   - Check token expiration time
   - Ensure Authorization header is included

4. **Gateway routing issues**
   - Verify all services are running
   - Check ocelot.json configuration
   - Ensure downstream ports match service ports

## 📚 Additional Resources

- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Windows Forms Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Authors

- Your Name - Initial work

## 🙏 Acknowledgments

- Thanks to the .NET community
- Inspired by modern microservices architecture patterns
- Built with best practices in mind
