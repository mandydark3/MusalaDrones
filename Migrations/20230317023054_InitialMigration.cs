using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusalaDrones.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drones",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<int>(type: "int", nullable: false),
                    WeightLimit = table.Column<float>(type: "real", nullable: false),
                    BatteryCapacity = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drones", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DroneID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatterLevel = table.Column<int>(type: "int", nullable: false),
                    Checked = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Logs_Drones_DroneID",
                        column: x => x.DroneID,
                        principalTable: "Drones",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DroneID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Medications_Drones_DroneID",
                        column: x => x.DroneID,
                        principalTable: "Drones",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_DroneID",
                table: "Logs",
                column: "DroneID");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_DroneID",
                table: "Medications",
                column: "DroneID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Drones");
        }
    }
}
