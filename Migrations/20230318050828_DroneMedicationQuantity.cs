using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusalaDrones.Migrations
{
    /// <inheritdoc />
    public partial class DroneMedicationQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DroneMedication");

            migrationBuilder.CreateTable(
                name: "DronesMedications",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DroneID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DronesMedications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DronesMedications_Drones_DroneID",
                        column: x => x.DroneID,
                        principalTable: "Drones",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DronesMedications_Medications_MedicationID",
                        column: x => x.MedicationID,
                        principalTable: "Medications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DronesMedications_DroneID",
                table: "DronesMedications",
                column: "DroneID");

            migrationBuilder.CreateIndex(
                name: "IX_DronesMedications_MedicationID",
                table: "DronesMedications",
                column: "MedicationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DronesMedications");

            migrationBuilder.CreateTable(
                name: "DroneMedication",
                columns: table => new
                {
                    DronesID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationsID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DroneMedication", x => new { x.DronesID, x.MedicationsID });
                    table.ForeignKey(
                        name: "FK_DroneMedication_Drones_DronesID",
                        column: x => x.DronesID,
                        principalTable: "Drones",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DroneMedication_Medications_MedicationsID",
                        column: x => x.MedicationsID,
                        principalTable: "Medications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DroneMedication_MedicationsID",
                table: "DroneMedication",
                column: "MedicationsID");
        }
    }
}
