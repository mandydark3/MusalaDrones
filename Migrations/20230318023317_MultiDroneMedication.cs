using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusalaDrones.Migrations
{
    /// <inheritdoc />
    public partial class MultiDroneMedication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medications_Drones_DroneID",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_Medications_DroneID",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "DroneID",
                table: "Medications");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DroneMedication");

            migrationBuilder.AddColumn<Guid>(
                name: "DroneID",
                table: "Medications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medications_DroneID",
                table: "Medications",
                column: "DroneID");

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_Drones_DroneID",
                table: "Medications",
                column: "DroneID",
                principalTable: "Drones",
                principalColumn: "ID");
        }
    }
}
