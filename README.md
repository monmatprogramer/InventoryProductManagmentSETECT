# 🏪 InventoryPro - Complete Inventory Management System

A professional **Windows Forms** application built with **.NET 9.0** using **microservices architecture** for comprehensive inventory management. This project demonstrates modern software development practices including microservices, API gateways, JWT authentication, and clean architecture principles.

---

## 📋 Table of Contents

1. [🎯 Project Overview](#-project-overview)
2. [🏗️ System Architecture](#️-system-architecture)
3. [🚀 Getting Started](#-getting-started)
4. [💡 Core Features](#-core-features)
5. [🔧 Technical Implementation](#-technical-implementation)
6. [📊 Code Examples](#-code-examples)
7. [🧪 Testing Guide](#-testing-guide)
8. [📈 Development Process](#-development-process)
9. [🎓 Key Learning Outcomes](#-key-learning-outcomes)
10. [🔮 Future Enhancements](#-future-enhancements)

---

## 🎯 Project Overview

### What is InventoryPro?

InventoryPro is a **complete business solution** for managing inventory, sales, and customers. It's designed to show how modern applications are built using:

- **Microservices Architecture** - Breaking down complex systems into smaller, manageable services
- **API Gateway Pattern** - Single entry point for all client requests
- **JWT Authentication** - Secure token-based authentication
- **Windows Forms Desktop UI** - Rich user interface for desktop users
- **Entity Framework Core** - Modern database access with migrations
- **Dependency Injection** - Loose coupling and testable code

### Why This Architecture?

This project demonstrates **real-world enterprise patterns** that are used in large-scale applications:

```
🏢 Enterprise Benefits:
├── Scalability: Each service can be scaled independently
├── Maintainability: Small, focused services are easier to maintain
├── Team Collaboration: Different teams can work on different services
├── Technology Diversity: Each service can use different technologies
└── Fault Isolation: If one service fails, others continue working
```

---

## 🏗️ System Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    InventoryPro System                          │
└─────────────────────────────────────────────────────────────────┘
                                │
                    ┌───────────▼───────────┐
                    │   Windows Forms       │
                    │   Desktop Client      │
                    │   (User Interface)    │
                    └───────────┬───────────┘
                                │ HTTP/REST API Calls
                    ┌───────────▼───────────┐
                    │     API Gateway       │
                    │    (Port 5000)        │
                    │   - Authentication    │
                    │   - Rate Limiting     │
                    │   - Request Routing   │
                    └───────────┬───────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
        │                       │                       │
┌───────▼──────┐    ┌──────────▼──────┐    ┌──────────▼──────┐
│ Auth Service │    │ Product Service │    │ Sales Service   │
│ (Port 5041)  │    │ (Port 5089)     │    │ (Port 5282)     │
│              │    │                 │    │                 │
│ - User Login │    │ - Product CRUD  │    │ - Sales Orders  │
│ - JWT Tokens │    │ - Inventory     │    │ - Customers     │
│ - User Mgmt  │    │ - Categories    │    │ - Invoices      │
└───────┬──────┘    └──────────┬──────┘    └──────────┬──────┘
        │                      │                      │
        │           ┌──────────▼──────┐               │
        │           │ Report Service  │               │
        │           │ (Port 5179)     │               │
        │           │                 │               │
        │           │ - Sales Reports │               │
        │           │ - Analytics     │               │
        │           │ - Data Export   │               │
        │           └──────────┬──────┘               │
        │                      │                      │
        └──────────────────────┼──────────────────────┘
                               │
                    ┌──────────▼──────┐
                    │   SQL Server    │
                    │   Databases     │
                    │                 │
                    │ - AuthDB        │
                    │ - ProductDB     │
                    │ - SalesDB       │
                    └─────────────────┘
```

### Service Communication Flow

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     Client      │    │   API Gateway   │    │    Services     │
│  (WinForms)     │    │   (Ocelot)      │    │ (Auth/Product)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │ 1. Login Request      │                       │
         ├──────────────────────▶│                       │
         │                       │ 2. Route to Auth      │
         │                       ├──────────────────────▶│
         │                       │                       │ 3. Validate User
         │                       │                       │    Generate JWT
         │                       │ 4. Return JWT Token   │
         │                       │◀──────────────────────┤
         │ 5. JWT Token          │                       │
         │◀──────────────────────┤                       │
         │                       │                       │
         │ 6. API Request        │                       │
         │    (with JWT)         │                       │
         ├──────────────────────▶│                       │
         │                       │ 7. Validate JWT       │
         │                       │    Route Request      │
         │                       ├──────────────────────▶│
         │                       │                       │ 8. Process Request
         │                       │                       │    Return Data
         │                       │ 9. Return Response    │
         │                       │◀──────────────────────┤
         │ 10. Display Data      │                       │
         │◀──────────────────────┤                       │
```

---

## 🚀 Getting Started

### Prerequisites

Make sure you have these installed:

```bash
✅ Visual Studio 2022 (with .NET desktop development)
✅ .NET 9.0 SDK
✅ SQL Server (Express or Developer Edition)
✅ Git
✅ Postman (for API testing)
```

### Quick Setup (5 Minutes)

#### Step 1: Clone and Setup Database

```bash
# Clone the repository
git clone https://github.com/yourusername/InventoryPro.git
cd InventoryPro

# Create databases (run in SQL Server Management Studio)
CREATE DATABASE InventoryPro_AuthDB;
CREATE DATABASE InventoryPro_ProductDB;
CREATE DATABASE InventoryPro_SalesDB;
```

#### Step 2: Run Database Migrations

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
```

#### Step 3: Start All Services

**Easy Way - PowerShell Script:**

```powershell
# Create start-all-services.ps1
Write-Host "🚀 Starting InventoryPro Services..." -ForegroundColor Green

# Start each service in a new terminal
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.AuthService; dotnet run"
Start-Sleep 2
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.ProductService; dotnet run"
Start-Sleep 2
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.SalesService; dotnet run"
Start-Sleep 2
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Services/InventoryPro.ReportService; dotnet run"
Start-Sleep 2
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/Gateway/InventoryPro.Gateway; dotnet run"

Write-Host "✅ All services started!" -ForegroundColor Green
Write-Host "🌐 Gateway URL: http://localhost:5000" -ForegroundColor Yellow
```

#### Step 4: Run the Desktop Application

1. Open `InventoryPro.sln` in Visual Studio
2. Set `InventoryPro.WinForms` as startup project
3. Press **F5** to run

**Default Login:**

- Username: `admin`
- Password: `admin123`

---

## 💡 Core Features

### 1. 🔐 Authentication System

```
📱 Login Screen
├── Username/Password validation
├── JWT token generation
├── Role-based access control
└── Session management
```

### 2. 📊 Dashboard

```
🎯 Real-time Analytics
├── Sales statistics (today, month, year)
├── Inventory levels and alerts
├── Low stock notifications
├── Recent activity feed
└── Quick navigation to all modules
```

### 3. 📦 Product Management

```
🛍️ Complete Product Lifecycle
├── Add/Edit/Delete products
├── SKU and barcode management
├── Category organization
├── Stock level tracking
├── Minimum stock alerts
└── Price management
```

### 4. 👥 Customer Management

```
🤝 Customer Database
├── Customer profiles
├── Contact information
├── Purchase history
├── Customer statistics
└── Loyalty tracking
```

### 5. 💰 Sales & Point of Sale

```
🛒 Sales Processing
├── Create sales orders
├── Real-time inventory updates
├── Multiple payment methods
├── Receipt generation
├── Invoice printing
└── Sales history
```

### 6. 📈 Reports & Analytics

```
📊 Business Intelligence
├── Sales reports (daily, monthly, yearly)
├── Inventory reports
├── Customer analysis
├── Financial summaries
└── Export to PDF/Excel/CSV
```

---

## 🔧 Technical Implementation

### Architecture Patterns Used

#### 1. **Microservices Architecture**

- **Single Responsibility**: Each service handles one business domain
- **Independent Deployment**: Services can be updated independently
- **Database per Service**: Each service has its own database
- **Communication via HTTP**: Services communicate through REST APIs

#### 2. **API Gateway Pattern**

- **Single Entry Point**: All client requests go through the gateway
- **Cross-cutting Concerns**: Authentication, logging, rate limiting
- **Service Discovery**: Routes requests to appropriate services
- **Load Balancing**: Distributes load across service instances

#### 3. **Repository Pattern**

- **Data Access Abstraction**: Separates business logic from data access
- **Testability**: Easy to mock for unit testing
- **Maintainability**: Changes to data access don't affect business logic

#### 4. **Dependency Injection**

- **Loose Coupling**: Classes don't create their dependencies
- **Testability**: Easy to inject mock objects for testing
- **Configuration**: Dependencies configured in one place

### Technology Stack

```
🏗️ Backend Technologies:
├── .NET 9.0 (Latest LTS)
├── ASP.NET Core Web API
├── Entity Framework Core
├── Ocelot API Gateway
├── JWT Bearer Authentication
├── Serilog Logging
├── Polly (Resilience patterns)
└── SQL Server

🎨 Frontend Technologies:
├── Windows Forms (.NET 9.0)
├── Dependency Injection
├── HttpClient with Polly
├── Async/Await patterns
└── Structured logging
```

---

## 📊 Code Examples

### 1. Authentication Service - JWT Token Generation

```csharp
// src/Services/InventoryPro.AuthService/Controllers/AuthController.cs

[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Authenticate user credentials
    var response = await _authService.AuthenticateAsync(request);

    if (response == null)
        return Unauthorized(new { message = "Invalid username or password" });

    // Return JWT token for authenticated user
    return Ok(response);
}
```

**What this code does:**

- Receives login credentials from the client
- Validates the input data
- Authenticates the user against the database
- Returns a JWT token for successful authentication
- Provides error messages for invalid credentials

### 2. API Gateway Configuration - Request Routing

```json
// src/Gateway/InventoryPro.Gateway/ocelot.json

{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5041
        }
      ],
      "UpstreamPathTemplate": "/auth/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "Limit": 100
      }
    }
  ]
}
```

**What this configuration does:**

- Routes all `/auth/*` requests to the Auth Service (port 5041)
- Applies rate limiting (100 requests per minute)
- Supports all HTTP methods (GET, POST, PUT, DELETE)
- Transforms the URL path for the downstream service

### 3. Product Service - CRUD Operations

```csharp
// src/Services/InventoryPro.ProductService/Controllers/ProductController.cs

[HttpGet]
public async Task<IActionResult> GetProducts([FromQuery] PaginationParameters parameters)
{
    try
    {
        var products = await _productService.GetAllProductsAsync();

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            products = products.Where(p =>
                p.Name.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.SKU.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Apply pagination
        var totalCount = products.Count();
        var items = products
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Description = p.Description,
                Price = p.Price,
                Stock = p.StockQuantity,
                MinStock = p.MinimumStock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt ?? p.CreatedAt
            })
            .ToList();

        var response = new PagedResponse<ProductDto>
        {
            Items = items,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting products");
        return StatusCode(500, "Internal server error");
    }
}
```

**What this code does:**

- Retrieves all products from the database
- Applies search filtering if a search term is provided
- Implements pagination to handle large datasets
- Converts database entities to DTOs for API response
- Includes error handling and logging
- Returns structured response with pagination metadata

### 4. Windows Forms Client - Dependency Injection Setup

```csharp
// src/Clients/InventoryPro.WinForms/Program.cs

static IHostBuilder CreateHostBuilder()
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            var config = GetConfiguration();
            services.AddSingleton<IConfiguration>(config);

            // HTTP Client with retry policies
            services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Register services
            services.AddSingleton<IAuthService, AuthService>();
            services.AddScoped<IApiService, ApiService>();

            // Register forms
            services.AddTransient<LoginForm>();
            services.AddTransient<MainForm>();
        })
        .UseSerilog();
}
```

**What this code does:**

- Sets up dependency injection container for the Windows Forms app
- Configures HttpClient with base URL and timeout
- Adds retry and circuit breaker policies for resilience
- Registers all services and forms for dependency injection
- Configures structured logging with Serilog

### 5. Data Transfer Objects (DTOs) - Type Safety

```csharp
// src/Shared/InventoryPro.Shared/DTOs/ProductDTOs.cs

public class ProductDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }

    public int MinStock { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**What this code does:**

- Defines the structure for product data transfer
- Includes validation attributes for data integrity
- Provides clear error messages for validation failures
- Ensures type safety across all services
- Separates data transfer from business logic

---

## 🧪 Testing Guide

### Manual Testing Workflow

#### 1. **Authentication Testing**

```
🔐 Test Login Process:
1. Start all services
2. Open the WinForms application
3. Try invalid credentials → Should show error
4. Use valid credentials (admin/admin123) → Should login successfully
5. Check if JWT token is stored and used for API calls
```

#### 2. **Product Management Testing**

```
📦 Test Product Operations:
1. Navigate to Products section
2. Add a new product → Check if it appears in the list
3. Edit an existing product → Verify changes are saved
4. Delete a product → Confirm it's removed
5. Test search functionality → Verify filtering works
6. Test pagination → Check if large datasets are handled properly
```

#### 3. **API Testing with Postman**

Create a Postman collection to test the APIs:

```json
{
  "info": {
    "name": "InventoryPro API Tests"
  },
  "item": [
    {
      "name": "1. Login",
      "request": {
        "method": "POST",
        "url": "http://localhost:5000/auth/login",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"username\": \"admin\",\n  \"password\": \"admin123\"\n}"
        }
      }
    },
    {
      "name": "2. Get Products",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/products",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ]
      }
    }
  ]
}
```

#### 4. **Service Integration Testing**

```
🔄 Test Service Communication:
1. Stop one service (e.g., Product Service)
2. Try to access products → Should show appropriate error
3. Restart the service → Should work normally again
4. Test with invalid JWT token → Should return 401 Unauthorized
5. Test rate limiting → Make many requests quickly
```

### Service Health Monitoring

Each service provides health endpoints:

```
✅ Health Check URLs:
├── Auth Service: http://localhost:5041/health
├── Product Service: http://localhost:5089/health
├── Sales Service: http://localhost:5282/health
├── Report Service: http://localhost:5179/health
└── API Gateway: http://localhost:5000/health
```

---

## 📈 Development Process

### Project Development Timeline

```
📅 Development Phases:

Phase 1: Foundation (Week 1-2)
├── Project structure setup
├── Database design and migrations
├── Basic authentication service
└── API Gateway configuration

Phase 2: Core Services (Week 3-4)
├── Product Service implementation
├── Sales Service development
├── Report Service creation
└── Service integration testing

Phase 3: Client Application (Week 5-6)
├── Windows Forms UI design
├── API integration
├── User experience optimization
└── Error handling implementation

Phase 4: Testing & Polish (Week 7-8)
├── Comprehensive testing
├── Performance optimization
├── Documentation completion
└── Deployment preparation
```

### Design Decisions and Trade-offs

#### **Why Microservices?**

- ✅ **Pros**: Scalability, maintainability, team autonomy
- ❌ **Cons**: Complexity, network latency, data consistency challenges
- **Decision**: Benefits outweigh complexity for learning purposes

#### **Why Windows Forms?**

- ✅ **Pros**: Rich desktop experience, full .NET ecosystem access
- ❌ **Cons**: Windows-only, traditional UI framework
- **Decision**: Perfect for business applications and .NET demonstration

#### **Why JWT Authentication?**

- ✅ **Pros**: Stateless, scalable, standard-based
- ❌ **Cons**: Token size, token revocation complexity
- **Decision**: Industry standard for API authentication

### Code Quality Practices

```
🛡️ Quality Assurance:
├── Dependency Injection for testability
├── Async/await for non-blocking operations
├── Structured logging with Serilog
├── Input validation and sanitization
├── Error handling and user feedback
├── Configuration management
└── Separation of concerns
```

---

## 🎓 Key Learning Outcomes

### 1. **Microservices Architecture**

**What you learn:**

- How to break down monolithic applications into smaller services
- Service-to-service communication patterns
- Database per service pattern
- Handling distributed system challenges

**Real-world application:**

- Large enterprise applications (Netflix, Amazon, Uber)
- Cloud-native applications
- Scalable web services

### 2. **API Gateway Pattern**

**What you learn:**

- Centralized request routing
- Cross-cutting concerns (auth, logging, rate limiting)
- Service discovery and load balancing
- API versioning and management

**Real-world application:**

- Cloud platforms (AWS API Gateway, Azure API Management)
- Enterprise integration patterns
- Mobile app backends

### 3. **JWT Authentication**

**What you learn:**

- Token-based authentication
- Stateless authentication systems
- Role-based access control
- Security best practices

**Real-world application:**

- Single Sign-On (SSO) systems
- Mobile app authentication
- API security

### 4. **Modern .NET Development**

**What you learn:**

- .NET 9.0 features and capabilities
- Entity Framework Core for data access
- Dependency injection patterns
- Async programming patterns

**Real-world application:**

- Enterprise web applications
- Cloud-native applications
- High-performance APIs

### 5. **Desktop Application Development**

**What you learn:**

- Windows Forms in modern .NET
- Desktop-to-API communication
- User experience design
- Error handling and user feedback

**Real-world application:**

- Business desktop applications
- Point-of-sale systems
- Administrative tools

### 6. **Software Architecture Principles**

**What you learn:**

- Separation of concerns
- Single responsibility principle
- Dependency inversion
- Clean architecture patterns

**Real-world application:**

- Maintainable codebases
- Scalable system design
- Team collaboration patterns

---

## 🔮 Future Enhancements

### Short-term Improvements (Next 3 months)

```
🚀 Immediate Enhancements:
├── Docker containerization for easy deployment
├── Unit tests for all services
├── API documentation with Swagger/OpenAPI
├── Configuration management improvements
├── Enhanced error handling and user feedback
└── Performance monitoring and metrics
```

### Medium-term Features (Next 6 months)

```
📈 Feature Additions:
├── Real-time notifications with SignalR
├── Barcode scanning integration
├── Advanced reporting with charts
├── Data export/import functionality
├── Multi-tenant support
├── Mobile app companion
└── Audit logging and compliance
```

### Long-term Vision (Next 12 months)

```
🌟 Advanced Features:
├── Machine learning for demand forecasting
├── Integration with external systems (ERP, CRM)
├── Advanced analytics and business intelligence
├── Multi-currency and multi-language support
├── Cloud deployment (Azure, AWS)
├── Microservices orchestration with Kubernetes
└── Event-driven architecture with message queues
```

### Potential Technology Upgrades

```
🔧 Technology Evolution:
├── gRPC for inter-service communication
├── Event sourcing for data consistency
├── CQRS pattern for read/write separation
├── Redis for caching and session management
├── Elasticsearch for advanced search
├── Apache Kafka for event streaming
└── Blazor for modern web UI
```

---

## 🏆 Project Summary

### What Makes This Project Special?

1. **Enterprise-Grade Architecture**: Uses patterns found in real enterprise applications
2. **Modern Technology Stack**: Latest .NET 9.0 with current best practices
3. **Complete Business Solution**: Not just a demo, but a functional inventory system
4. **Educational Value**: Demonstrates multiple architectural patterns and practices
5. **Scalable Design**: Can be extended and deployed in production environments

### Key Technical Achievements

```
✨ Technical Highlights:
├── 🏗️ Microservices with proper separation of concerns
├── 🛡️ JWT-based authentication and authorization
├── 🚪 API Gateway with routing and rate limiting
├── 💾 Database-first approach with EF Core migrations
├── 🖥️ Rich desktop UI with Windows Forms
├── 🔄 Resilient HTTP communication with Polly
├── 📝 Structured logging throughout the application
└── 🧪 Comprehensive error handling and validation
```

### Business Value Delivered

```
💼 Business Benefits:
├── Complete inventory management capability
├── Real-time stock tracking and alerts
├── Customer relationship management
├── Sales order processing and invoicing
├── Comprehensive reporting and analytics
├── User management and role-based access
└── Scalable architecture for business growth
```

---

## 📚 Additional Resources

### Learning Materials

- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [Microservices Architecture Guide](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Ocelot API Gateway Documentation](https://ocelot.readthedocs.io/)
- [JWT Authentication Guide](https://jwt.io/introduction)

### Related Projects and Examples

- [Microsoft eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [ASP.NET Core Samples](https://github.com/aspnet/samples)

---

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines

- Follow existing code style and patterns
- Add unit tests for new features
- Update documentation for significant changes
- Test your changes thoroughly before submitting

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**MONMAT** - _Initial work and architecture_

## 🙏 Acknowledgments

- Thanks to the **.NET community** for excellent documentation and samples
- Inspired by **microservices architecture patterns** from industry leaders
- Built following **clean architecture principles** and best practices
- Special thanks to **instructors and peers** for guidance and feedback

---

**🚀 Ready to explore the world of microservices and modern .NET development? Clone this repository and start your journey!**
