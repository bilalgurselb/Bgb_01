﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace SiparisApi.Migrations
{
    [DbContext(typeof(SiparisApi.Data.AppDbContext))]
    [Migration("20251103_InitialFullSchema")]
    public partial class InitialFullSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ SintanCari (CASABIT yapısı)
            migrationBuilder.CreateTable(
                name: "SintanCari",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CARI_ISIM = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    CARI_KOD = table.Column<string>(type: "varchar(15)", nullable: false),
                    GRUP_KODU = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    GRUP_ISMI = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    KOD_1 = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    KOD_2 = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    ADRES = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    IL = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    ILCE = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    TELEFON = table.Column<string>(type: "varchar(20)", nullable: true),
                    FAKS = table.Column<string>(type: "varchar(20)", nullable: true),
                    VERGI_DAIRESI = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    VERGI_NUMARASI = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    POSTAKODU = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    VADE_GUNU = table.Column<short>(type: "smallint", nullable: true),
                    PLASIYER_KODU = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    ULKE_KODU = table.Column<string>(type: "varchar(4)", nullable: true),
                    EMAIL = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    WEB = table.Column<string>(type: "varchar(60)", nullable: true),
                    TC_NO = table.Column<string>(type: "varchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SintanCari", x => x.Id);
                });

            // 2️⃣ SintanStok (STOKSABIT yapısı)
            migrationBuilder.CreateTable(
                name: "SintanStok",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    STOK_ADI = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    STOK_KODU = table.Column<string>(type: "varchar(35)", nullable: false),
                    KOD_1 = table.Column<string>(type: "nvarchar(8000)", nullable: true),
                    PAY1 = table.Column<decimal>(type: "decimal(13,8)", nullable: true),
                    AMBALAJ_AGIRLIGI = table.Column<decimal>(type: "decimal(13,8)", nullable: true),
                    OLCU_BR1 = table.Column<string>(type: "varchar(2)", nullable: true),
                    PALET_AMBALAJ_ADEDI = table.Column<decimal>(type: "decimal(17,10)", nullable: true),
                    OLCU_BR2 = table.Column<string>(type: "varchar(2)", nullable: true),
                    PALET_NET_AGIRLIGI = table.Column<decimal>(type: "decimal(13,8)", nullable: true),
                    A = table.Column<string>(type: "varchar(1)", nullable: true),
                    OLCU_BR3 = table.Column<string>(type: "varchar(2)", nullable: true),
                    PAY2 = table.Column<decimal>(type: "decimal(13,8)", nullable: true),
                    CEVRIM_DEGERI_1 = table.Column<decimal>(type: "decimal(17,10)", nullable: true),
                    ASGARI_STOK = table.Column<decimal>(type: "decimal(13,8)", nullable: true),
                    BIRIM_AGIRLIGI = table.Column<decimal>(type: "decimal(13,8)", nullable: true),
                    NAKLIYET_TUT = table.Column<decimal>(type: "decimal(13,8)", nullable: true),
                    OZEL_SAHA = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SintanStok", x => x.Id);
                });

            // 3️⃣ AllowedEmails
            migrationBuilder.CreateTable(
                name: "AllowedEmails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: false),
                    Role = table.Column<string>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    NameSurname = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedEmails", x => x.Id);
                });

            // 4️⃣ Users (Role kaldırıldı, AllowedId eklendi)
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    AllowedId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_AllowedEmails_AllowedId",
                        column: x => x.AllowedId,
                        principalTable: "AllowedEmails",
                        principalColumn: "Id");
                });

            // 5️⃣ Logs
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            // 6️⃣ OrderHeaders
            migrationBuilder.CreateTable(
                name: "OrderHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    CustomerId = table.Column<int>(nullable: true),
                    SalesRepId = table.Column<int>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    PortOfDelivery = table.Column<string>(maxLength: 100, nullable: true),
                    PlaceOfDelivery = table.Column<string>(maxLength: 100, nullable: true),
                    Status = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHeaders", x => x.Id);
                    table.ForeignKey("FK_OrderHeaders_SintanCari_CustomerId", x => x.CustomerId, "SintanCari", "Id");
                    table.ForeignKey("FK_OrderHeaders_Users_SalesRepId", x => x.SalesRepId, "Users", "Id");
                    table.ForeignKey("FK_OrderHeaders_Users_CreatedById", x => x.CreatedById, "Users", "Id");
                });

            // 7️⃣ OrderItems
            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderHeaderId = table.Column<int>(nullable: false),
                    ProductId = table.Column<int>(nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsApprovedByFactory = table.Column<bool>(nullable: true),
                    IsApprovedBySales = table.Column<bool>(nullable: true),
                    RowNumber = table.Column<int>(nullable: true),
                    PackingInfo = table.Column<string>(maxLength: 100, nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey("FK_OrderItems_OrderHeaders_OrderHeaderId", x => x.OrderHeaderId, "OrderHeaders", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_OrderItems_SintanStok_ProductId", x => x.ProductId, "SintanStok", "Id");
                });

            // 8️⃣ OrderStatusHistories
            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderHeaderId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    ChangedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangedById = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                    table.ForeignKey("FK_OrderStatusHistories_OrderHeaders_OrderHeaderId", x => x.OrderHeaderId, "OrderHeaders", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_OrderStatusHistories_Users_ChangedById", x => x.ChangedById, "Users", "Id");
                });

            // --- Indexler ve Unique Keyler ---

            // AllowedEmails ve Users için Email unique
            migrationBuilder.CreateIndex(
                name: "IX_AllowedEmails_Email",
                table: "AllowedEmails",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AllowedId",
                table: "Users",
                column: "AllowedId");

            // Logs
            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId",
                table: "Logs",
                column: "UserId");

            // OrderHeaders performans indexleri
            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_CreatedAt",
                table: "OrderHeaders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_CreatedById",
                table: "OrderHeaders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_Status",
                table: "OrderHeaders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_CustomerId",
                table: "OrderHeaders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_SalesRepId",
                table: "OrderHeaders",
                column: "SalesRepId");

            // OrderItems
            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderHeaderId",
                table: "OrderItems",
                column: "OrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            // OrderStatusHistories
            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderHeaderId",
                table: "OrderStatusHistories",
                column: "OrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_ChangedById",
                table: "OrderStatusHistories",
                column: "ChangedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OrderStatusHistories");
            migrationBuilder.DropTable(name: "OrderItems");
            migrationBuilder.DropTable(name: "Logs");
            migrationBuilder.DropTable(name: "OrderHeaders");
            migrationBuilder.DropTable(name: "SintanCari");
            migrationBuilder.DropTable(name: "SintanStok");
            migrationBuilder.DropTable(name: "AllowedEmails");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
