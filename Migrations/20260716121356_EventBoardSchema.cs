using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventApi.Migrations
{
    /// <inheritdoc />
    public partial class EventBoardSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Events");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BookedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventBookings_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventFavorites",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    FavoritedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventFavorites", x => new { x.UserId, x.EventId });
                    table.ForeignKey(
                        name: "FK_EventFavorites_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Large industry conferences and summits.", "Conference" },
                    { 2, "Hands-on training workshops.", "Workshop" },
                    { 3, "Live music and performances.", "Concert" },
                    { 4, "Sporting events and tournaments.", "Sports" },
                    { 5, "Community and networking meetups.", "Meetup" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "admin@eventboard.com", "Admin User", "$2a$11$ExjLvV2ie52b756HbndsOOLDoKEexPRtc9Sz7oxFigjZSGtiAaMA2", "Admin" },
                    { 2, "sara@eventboard.com", "Sara Khan", "$2a$11$B6Ctfp3GjbtOj8ZfvmiLtuQwnBvys/603hpg4OA36745T.AYNx.Qa", "User" },
                    { 3, "bilal@eventboard.com", "Bilal Ahmed", "$2a$11$mWlv.Rm30KrmZJR88vNNiew809Api5AXDNWBgRlooYEL6nnASB1m.", "User" }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "EventDate", "Location", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 7, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), "Annual .NET developer conference.", new DateTime(2026, 9, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), "Lahore", ".NET Conf 2026", 1 },
                    { 2, 2, new DateTime(2026, 7, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), "Hands-on EF Core workshop.", new DateTime(2026, 9, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), "Karachi", "EF Core Deep Dive", 1 },
                    { 3, 3, new DateTime(2026, 7, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), "Live in concert.", new DateTime(2026, 10, 5, 19, 0, 0, 0, DateTimeKind.Unspecified), "Islamabad", "Coldplay Live", 2 },
                    { 4, 4, new DateTime(2026, 7, 2, 12, 0, 0, 0, DateTimeKind.Unspecified), "Annual city marathon.", new DateTime(2026, 11, 1, 6, 30, 0, 0, DateTimeKind.Unspecified), "Lahore", "City Marathon", 2 },
                    { 5, 5, new DateTime(2026, 7, 3, 12, 0, 0, 0, DateTimeKind.Unspecified), "Cloud community meetup.", new DateTime(2026, 8, 20, 18, 0, 0, 0, DateTimeKind.Unspecified), "Karachi", "Azure Meetup", 3 },
                    { 6, 1, new DateTime(2026, 7, 3, 12, 0, 0, 0, DateTimeKind.Unspecified), "Artificial intelligence summit.", new DateTime(2026, 9, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), "Lahore", "AI Summit", 1 },
                    { 7, 2, new DateTime(2026, 7, 4, 12, 0, 0, 0, DateTimeKind.Unspecified), "Frontend workshop.", new DateTime(2026, 10, 12, 10, 0, 0, 0, DateTimeKind.Unspecified), "Islamabad", "React Workshop", 3 },
                    { 8, 3, new DateTime(2026, 7, 4, 12, 0, 0, 0, DateTimeKind.Unspecified), "An evening of jazz.", new DateTime(2026, 11, 8, 20, 0, 0, 0, DateTimeKind.Unspecified), "Lahore", "Jazz Night", 2 },
                    { 9, 4, new DateTime(2026, 7, 5, 12, 0, 0, 0, DateTimeKind.Unspecified), "National cricket finals.", new DateTime(2026, 12, 1, 15, 0, 0, 0, DateTimeKind.Unspecified), "Karachi", "Cricket Finals", 1 },
                    { 10, 5, new DateTime(2026, 7, 5, 12, 0, 0, 0, DateTimeKind.Unspecified), "Founders networking meetup.", new DateTime(2026, 8, 30, 17, 0, 0, 0, DateTimeKind.Unspecified), "Islamabad", "Startup Meetup", 3 }
                });

            migrationBuilder.InsertData(
                table: "EventBookings",
                columns: new[] { "Id", "BookedAt", "EventId", "Status", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), 1, "Confirmed", 2 },
                    { 2, new DateTime(2026, 7, 6, 11, 0, 0, 0, DateTimeKind.Unspecified), 3, "Pending", 2 },
                    { 3, new DateTime(2026, 7, 7, 9, 0, 0, 0, DateTimeKind.Unspecified), 1, "Confirmed", 3 },
                    { 4, new DateTime(2026, 7, 7, 12, 0, 0, 0, DateTimeKind.Unspecified), 5, "Cancelled", 3 },
                    { 5, new DateTime(2026, 7, 8, 14, 0, 0, 0, DateTimeKind.Unspecified), 9, "Confirmed", 1 }
                });

            migrationBuilder.InsertData(
                table: "EventFavorites",
                columns: new[] { "EventId", "UserId", "FavoritedAt" },
                values: new object[,]
                {
                    { 9, 1, new DateTime(2026, 7, 8, 14, 30, 0, 0, DateTimeKind.Unspecified) },
                    { 1, 2, new DateTime(2026, 7, 6, 10, 30, 0, 0, DateTimeKind.Unspecified) },
                    { 6, 2, new DateTime(2026, 7, 6, 10, 35, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 3, new DateTime(2026, 7, 7, 9, 15, 0, 0, DateTimeKind.Unspecified) },
                    { 7, 3, new DateTime(2026, 7, 7, 9, 20, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CategoryId",
                table: "Events",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventDate",
                table: "Events",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventBookings_EventId",
                table: "EventBookings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBookings_UserId",
                table: "EventBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventFavorites_EventId",
                table: "EventFavorites",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventFavorites_UserId",
                table: "EventFavorites",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "EventBookings");

            migrationBuilder.DropTable(
                name: "EventFavorites");

            migrationBuilder.DropIndex(
                name: "IX_Events_CategoryId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventDate",
                table: "Events");

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Events",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
