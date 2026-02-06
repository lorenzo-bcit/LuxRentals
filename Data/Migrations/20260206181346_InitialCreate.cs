using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxRentals.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingStatus",
                columns: table => new
                {
                    pkBookingStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bookingStatus = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BookingS__9B31AD4290225E69", x => x.pkBookingStatusId);
                });

            migrationBuilder.CreateTable(
                name: "CarStatus",
                columns: table => new
                {
                    pkCarStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    statusFlag = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CarStatu__18420CBD73A9C723", x => x.pkCarStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    pkCustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    firstName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    lastName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    phoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    driverLicenceNo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    licenceVerified = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__1FD9D5A2DFB315FE", x => x.pkCustomerId);
                });

            migrationBuilder.CreateTable(
                name: "FuelType",
                columns: table => new
                {
                    pkFuelTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fuelType = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FuelType__8BECB883C1BFCF4F", x => x.pkFuelTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Make",
                columns: table => new
                {
                    pkMakeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    makeName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Make__DCFC4DDA57C669DE", x => x.pkMakeId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleClass",
                columns: table => new
                {
                    pkVehicleClassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicleClass = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VehicleC__0B88CBFE1D5AB50C", x => x.pkVehicleClassId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Model",
                columns: table => new
                {
                    pkModelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    modelName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    fkMakeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Model__C59EF50749A793B2", x => x.pkModelId);
                    table.ForeignKey(
                        name: "FK_Model_Make",
                        column: x => x.fkMakeId,
                        principalTable: "Make",
                        principalColumn: "pkMakeId");
                });

            migrationBuilder.CreateTable(
                name: "Car",
                columns: table => new
                {
                    pkCarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    colour = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    transmissionType = table.Column<byte>(type: "tinyint", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    carThumbnail = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    vinNumber = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    licencePlate = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    personCap = table.Column<int>(type: "int", nullable: false),
                    luggageCap = table.Column<int>(type: "int", nullable: false),
                    dailyRate = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    fkVehicleClassId = table.Column<int>(type: "int", nullable: false),
                    fkCarStatusId = table.Column<int>(type: "int", nullable: false),
                    fkModelId = table.Column<int>(type: "int", nullable: false),
                    fkFuelTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Car__042F2AAC6BD16B16", x => x.pkCarId);
                    table.ForeignKey(
                        name: "FK_Car_CarStatus",
                        column: x => x.fkCarStatusId,
                        principalTable: "CarStatus",
                        principalColumn: "pkCarStatusId");
                    table.ForeignKey(
                        name: "FK_Car_FuelType",
                        column: x => x.fkFuelTypeId,
                        principalTable: "FuelType",
                        principalColumn: "pkFuelTypeId");
                    table.ForeignKey(
                        name: "FK_Car_Model",
                        column: x => x.fkModelId,
                        principalTable: "Model",
                        principalColumn: "pkModelId");
                    table.ForeignKey(
                        name: "FK_Car_VehicleClass",
                        column: x => x.fkVehicleClassId,
                        principalTable: "VehicleClass",
                        principalColumn: "pkVehicleClassId");
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    pkBookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    startDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fkBookingStatusId = table.Column<int>(type: "int", nullable: false),
                    fkCarId = table.Column<int>(type: "int", nullable: false),
                    fkCustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Booking__8A399DE0B0FB731A", x => x.pkBookingId);
                    table.ForeignKey(
                        name: "FK_Booking_Car",
                        column: x => x.fkCarId,
                        principalTable: "Car",
                        principalColumn: "pkCarId");
                    table.ForeignKey(
                        name: "FK_Booking_Customer",
                        column: x => x.fkCustomerId,
                        principalTable: "Customer",
                        principalColumn: "pkCustomerId");
                    table.ForeignKey(
                        name: "FK_Booking_Status",
                        column: x => x.fkBookingStatusId,
                        principalTable: "BookingStatus",
                        principalColumn: "pkBookingStatusId");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    pkTransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    amountPaid = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    paymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fkBookingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transact__D335440639A81962", x => x.pkTransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_Booking",
                        column: x => x.fkBookingId,
                        principalTable: "Booking",
                        principalColumn: "pkBookingId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_fkBookingStatusId",
                table: "Booking",
                column: "fkBookingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_fkCarId",
                table: "Booking",
                column: "fkCarId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_fkCustomerId",
                table: "Booking",
                column: "fkCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Car_fkCarStatusId",
                table: "Car",
                column: "fkCarStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Car_fkFuelTypeId",
                table: "Car",
                column: "fkFuelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Car_fkModelId",
                table: "Car",
                column: "fkModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Car_fkVehicleClassId",
                table: "Car",
                column: "fkVehicleClassId");

            migrationBuilder.CreateIndex(
                name: "UQ__Car__1771C83D56CF7E7E",
                table: "Car",
                column: "licencePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Car__96F3ADF410EA4174",
                table: "Car",
                column: "vinNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Customer__021FFF575E2F467D",
                table: "Customer",
                column: "driverLicenceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Customer__AB6E6164F79866D8",
                table: "Customer",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Model_fkMakeId",
                table: "Model",
                column: "fkMakeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_fkBookingId",
                table: "Transaction",
                column: "fkBookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Car");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "BookingStatus");

            migrationBuilder.DropTable(
                name: "CarStatus");

            migrationBuilder.DropTable(
                name: "FuelType");

            migrationBuilder.DropTable(
                name: "Model");

            migrationBuilder.DropTable(
                name: "VehicleClass");

            migrationBuilder.DropTable(
                name: "Make");
        }
    }
}
