using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiparisApi.Migrations
{
    /// <inheritdoc />
    public partial class FinalCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NameSurname = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SintanCari",
                columns: table => new
                {
                    CARI_KOD = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CARI_ISIM = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GRUP_KODU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GRUP_ISMI = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    KOD_1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    KOD_2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ADRES = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ILCE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TELEFON = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FAKS = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VERGI_DAIRESI = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VERGI_NUMARASI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    POSTAKODU = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VADE_GUNU = table.Column<short>(type: "smallint", nullable: true),
                    PLASIYER_KODU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ULKE_KODU = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    EMAIL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WEB = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TC_NO = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SintanCari", x => x.CARI_KOD);
                });

            migrationBuilder.CreateTable(
                name: "SintanStok",
                columns: table => new
                {
                    STOK_KODU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    STOK_ADI = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    KOD_1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PAY1 = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    AMBALAJ_AGIRLIGI = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    OLCU_BR1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PALET_AMBALAJ_ADEDI = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    OLCU_BR2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PALET_NET_AGIRLIGI = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    A = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OLCU_BR3 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PAY2 = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    CEVRIM_DEGERI_1 = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    ASGARI_STOK = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    BIRIM_AGIRLIK = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    NAKLIYET_TUT = table.Column<decimal>(type: "decimal(13,2)", nullable: true),
                    OZEL_SAHA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SintanStok", x => x.STOK_KODU);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "OrderHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesRepId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentTerm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryTerm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDays = table.Column<int>(type: "int", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PortOfDelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOfDelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsNew = table.Column<bool>(type: "bit", nullable: false),
                    IsUpdated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderHeaders_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderHeaders_Users_SalesRepId",
                        column: x => x.SalesRepId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(13,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(13,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApprovedByFactory = table.Column<bool>(type: "bit", nullable: true),
                    IsApprovedBySales = table.Column<bool>(type: "bit", nullable: true),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    PackingInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(13,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_OrderHeaders_OrderHeaderId",
                        column: x => x.OrderHeaderId,
                        principalTable: "OrderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_OrderHeaders_OrderHeaderId",
                        column: x => x.OrderHeaderId,
                        principalTable: "OrderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedEmails_Email",
                table: "AllowedEmails",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId",
                table: "Logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_CreatedAt",
                table: "OrderHeaders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_CreatedById",
                table: "OrderHeaders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_SalesRepId",
                table: "OrderHeaders",
                column: "SalesRepId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_Status",
                table: "OrderHeaders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderHeaderId",
                table: "OrderItems",
                column: "OrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_ChangedById",
                table: "OrderStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderHeaderId",
                table: "OrderStatusHistories",
                column: "OrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AllowedId",
                table: "Users",
                column: "AllowedId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "SintanCari");

            migrationBuilder.DropTable(
                name: "SintanStok");

            migrationBuilder.DropTable(
                name: "OrderHeaders");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AllowedEmails");
        }
    }
}
