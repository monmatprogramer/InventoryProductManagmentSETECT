using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryPro.AuthService.Migrations
    {
    /// <inheritdoc />
    public partial class InitialCreate : Migration
        {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
            {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                    {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            // Seed initial admin user
            // Password: admin123 (hashed with BCrypt)
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Username", "PasswordHash", "Email", "FirstName", "LastName", "Role", "IsActive", "CreatedAt" },
                values: new object[] {
                    1,
                    "admin",
                    "$2a$11$rBNrVkNdKi8qVz7QWXJ9q.6L6U7LMX8J8V8YH5L5V9QXjZ6jVUq2a",
                    "admin@inventorypro.com",
                    "System",
                    "Administrator",
                    "Admin",
                    true,
                    new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });

            // Add a regular user for testing
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Username", "PasswordHash", "Email", "FirstName", "LastName", "Role", "IsActive", "CreatedAt" },
                values: new object[] {
                    2,
                    "user",
                    "$2a$11$mQqQ1tGOmXmGvUQJM3H3cOYxjCH7g8QXiHfHhWXcPvJ2JjKkKkKkK", // user123
                    "user@inventorypro.com",
                    "Test",
                    "User",
                    "User",
                    true,
                    new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                });
            }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            {
            migrationBuilder.DropTable(
                name: "Users");
            }
        }
    }