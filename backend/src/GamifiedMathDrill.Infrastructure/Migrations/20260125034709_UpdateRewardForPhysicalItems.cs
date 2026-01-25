using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamifiedMathDrill.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRewardForPhysicalItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Rewards",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Rewards",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Rewards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhysical",
                table: "Rewards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Rewards",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Rewards",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Rewards",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "IsPhysical",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Rewards");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Rewards");
        }
    }
}
