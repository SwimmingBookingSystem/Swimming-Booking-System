using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "Staff_Types",
                columns: table => new
                {
                    staff_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff_Types", x => x.staff_type_id);
                });

            migrationBuilder.CreateTable(
                name: "TicketType",
                columns: table => new
                {
                    ticket_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    type_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    base_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_combo = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    discount_percent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketType", x => x.ticket_type_id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    images = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_role_id",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComboDetail",
                columns: table => new
                {
                    combo_type_id = table.Column<int>(type: "int", nullable: false),
                    included_type_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboDetail", x => new { x.combo_type_id, x.included_type_id });
                    table.ForeignKey(
                        name: "FK_ComboDetail_TicketType_combo_type_id",
                        column: x => x.combo_type_id,
                        principalTable: "TicketType",
                        principalColumn: "ticket_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboDetail_TicketType_included_type_id",
                        column: x => x.included_type_id,
                        principalTable: "TicketType",
                        principalColumn: "ticket_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Account_Ban_Log",
                columns: table => new
                {
                    ban_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    banned_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_permanent = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account_Ban_Log", x => x.ban_id);
                    table.ForeignKey(
                        name: "FK_Account_Ban_Log_Users_banned_by",
                        column: x => x.banned_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Account_Ban_Log_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Branchs",
                columns: table => new
                {
                    branch_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    branch_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    manager_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branchs", x => x.branch_id);
                    table.ForeignKey(
                        name: "FK_Branchs_Users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    contact_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    is_resolved = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.contact_id);
                    table.ForeignKey(
                        name: "FK_Contact_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Discount",
                columns: table => new
                {
                    discount_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    discount_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    discount_percent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    valid_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    valid_to = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discount", x => x.discount_id);
                    table.ForeignKey(
                        name: "FK_Discount_Users_created_by",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    target_role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    target_branch_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_Notification_Branchs_target_branch_id",
                        column: x => x.target_branch_id,
                        principalTable: "Branchs",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notification_Roles_target_role_id",
                        column: x => x.target_role_id,
                        principalTable: "Roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notification_Users_created_by",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pools",
                columns: table => new
                {
                    pool_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pool_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    pool_road = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    pool_address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    max_slot = table.Column<int>(type: "int", nullable: false),
                    open_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    close_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    pool_status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    pool_image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    pool_description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    branch_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pools", x => x.pool_id);
                    table.ForeignKey(
                        name: "FK_Pools_Branchs_branch_id",
                        column: x => x.branch_id,
                        principalTable: "Branchs",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactResponse",
                columns: table => new
                {
                    response_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    contact_id = table.Column<int>(type: "int", nullable: false),
                    responder_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    response_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    response_time = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactResponse", x => x.response_id);
                    table.ForeignKey(
                        name: "FK_ContactResponse_Contact_contact_id",
                        column: x => x.contact_id,
                        principalTable: "Contact",
                        principalColumn: "contact_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactResponse_Users_responder_id",
                        column: x => x.responder_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountAuditLog",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    discount_id = table.Column<int>(type: "int", nullable: false),
                    manager_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    action_time = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountAuditLog", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_DiscountAuditLog_Discount_discount_id",
                        column: x => x.discount_id,
                        principalTable: "Discount",
                        principalColumn: "discount_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscountAuditLog_Users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    discount_id = table.Column<int>(type: "int", nullable: true),
                    booking_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    slot_count = table.Column<int>(type: "int", nullable: false),
                    booking_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_Booking_Discount_discount_id",
                        column: x => x.discount_id,
                        principalTable: "Discount",
                        principalColumn: "discount_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Booking_Pools_pool_id",
                        column: x => x.pool_id,
                        principalTable: "Pools",
                        principalColumn: "pool_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Booking_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Pool_Device",
                columns: table => new
                {
                    device_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    device_image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    device_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    device_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pool_Device", x => x.device_id);
                    table.ForeignKey(
                        name: "FK_Pool_Device_Pools_pool_id",
                        column: x => x.pool_id,
                        principalTable: "Pools",
                        principalColumn: "pool_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoolImage",
                columns: table => new
                {
                    image_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    pool_image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolImage", x => x.image_id);
                    table.ForeignKey(
                        name: "FK_PoolImage_Pools_pool_id",
                        column: x => x.pool_id,
                        principalTable: "Pools",
                        principalColumn: "pool_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoolService",
                columns: table => new
                {
                    pool_service_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    service_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    service_image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    service_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolService", x => x.pool_service_id);
                    table.ForeignKey(
                        name: "FK_PoolService_Pools_pool_id",
                        column: x => x.pool_id,
                        principalTable: "Pools",
                        principalColumn: "pool_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoolTicketType",
                columns: table => new
                {
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    ticket_type_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolTicketType", x => new { x.pool_id, x.ticket_type_id });
                    table.ForeignKey(
                        name: "FK_PoolTicketType_Pools_pool_id",
                        column: x => x.pool_id,
                        principalTable: "Pools",
                        principalColumn: "pool_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoolTicketType_TicketType_ticket_type_id",
                        column: x => x.ticket_type_id,
                        principalTable: "TicketType",
                        principalColumn: "ticket_type_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    staff_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    branch_id = table.Column<int>(type: "int", nullable: false),
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    staff_type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.staff_id);
                    table.ForeignKey(
                        name: "FK_Staffs_Branchs_branch_id",
                        column: x => x.branch_id,
                        principalTable: "Branchs",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Staffs_Pools_pool_id",
                        column: x => x.pool_id,
                        principalTable: "Pools",
                        principalColumn: "pool_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Staffs_Staff_Types_staff_type_id",
                        column: x => x.staff_type_id,
                        principalTable: "Staff_Types",
                        principalColumn: "staff_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Staffs_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCheckin",
                columns: table => new
                {
                    checkin_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    checkin_status = table.Column<byte>(type: "tinyint", nullable: true),
                    checkin_time = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCheckin", x => x.checkin_id);
                    table.ForeignKey(
                        name: "FK_CustomerCheckin_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerCheckin_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    feedback_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    replied = table.Column<bool>(type: "bit", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.feedback_id);
                    table.ForeignKey(
                        name: "FK_Feedback_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_Pools_pool_id",
                        column: x => x.pool_id,
                        principalTable: "Pools",
                        principalColumn: "pool_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    payment_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    discount_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    transaction_reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_Payment_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    ticket_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    ticket_type_id = table.Column<int>(type: "int", nullable: false),
                    ticket_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ticket_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    issued_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    issued_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.ticket_id);
                    table.ForeignKey(
                        name: "FK_Ticket_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ticket_TicketType_ticket_type_id",
                        column: x => x.ticket_type_id,
                        principalTable: "TicketType",
                        principalColumn: "ticket_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Users_issued_by",
                        column: x => x.issued_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BookingService",
                columns: table => new
                {
                    booking_service_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    pool_service_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    total_service_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingService", x => x.booking_service_id);
                    table.ForeignKey(
                        name: "FK_BookingService_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingService_PoolService_pool_service_id",
                        column: x => x.pool_service_id,
                        principalTable: "PoolService",
                        principalColumn: "pool_service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceReport",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    device_id = table.Column<int>(type: "int", nullable: true),
                    report_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    suggestion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    report_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    manager_note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    processed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceReport", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_DeviceReport_Pool_Device_device_id",
                        column: x => x.device_id,
                        principalTable: "Pool_Device",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeviceReport_Staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceReport_Users_processed_by",
                        column: x => x.processed_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SaleTicketDirectly",
                columns: table => new
                {
                    sale_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    customer_phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    customer_email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    sale_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleTicketDirectly", x => x.sale_id);
                    table.ForeignKey(
                        name: "FK_SaleTicketDirectly_Booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleTicketDirectly_Staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleTicketDirectly_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ServiceReport",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    service_id = table.Column<int>(type: "int", nullable: false),
                    report_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    suggestion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    report_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    manager_note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    processed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReport", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_ServiceReport_PoolService_service_id",
                        column: x => x.service_id,
                        principalTable: "PoolService",
                        principalColumn: "pool_service_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceReport_Staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceReport_Users_processed_by",
                        column: x => x.processed_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Ban_Log_banned_by",
                table: "Account_Ban_Log",
                column: "banned_by");

            migrationBuilder.CreateIndex(
                name: "IX_Account_Ban_Log_user_id",
                table: "Account_Ban_Log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

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
                name: "IX_Booking_discount_id",
                table: "Booking",
                column: "discount_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_pool_id",
                table: "Booking",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_user_id",
                table: "Booking",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_BookingService_booking_id",
                table: "BookingService",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_BookingService_pool_service_id",
                table: "BookingService",
                column: "pool_service_id");

            migrationBuilder.CreateIndex(
                name: "IX_Branchs_manager_id",
                table: "Branchs",
                column: "manager_id",
                unique: true,
                filter: "[manager_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ComboDetail_included_type_id",
                table: "ComboDetail",
                column: "included_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_user_id",
                table: "Contact",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResponse_contact_id",
                table: "ContactResponse",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResponse_responder_id",
                table: "ContactResponse",
                column: "responder_id");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCheckin_booking_id",
                table: "CustomerCheckin",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCheckin_user_id",
                table: "CustomerCheckin",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReport_device_id",
                table: "DeviceReport",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReport_processed_by",
                table: "DeviceReport",
                column: "processed_by");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReport_staff_id",
                table: "DeviceReport",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Discount_created_by",
                table: "Discount",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountAuditLog_discount_id",
                table: "DiscountAuditLog",
                column: "discount_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountAuditLog_manager_id",
                table: "DiscountAuditLog",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_booking_id",
                table: "Feedback",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_pool_id",
                table: "Feedback",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_user_id",
                table: "Feedback",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_created_by",
                table: "Notification",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_target_branch_id",
                table: "Notification",
                column: "target_branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_target_role_id",
                table: "Notification",
                column: "target_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_booking_id",
                table: "Payment",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_Pool_Device_pool_id",
                table: "Pool_Device",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "IX_PoolImage_pool_id",
                table: "PoolImage",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "IX_Pools_branch_id",
                table: "Pools",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_PoolService_pool_id",
                table: "PoolService",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "IX_PoolTicketType_ticket_type_id",
                table: "PoolTicketType",
                column: "ticket_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_role_name",
                table: "Roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTicketDirectly_booking_id",
                table: "SaleTicketDirectly",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTicketDirectly_staff_id",
                table: "SaleTicketDirectly",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTicketDirectly_user_id",
                table: "SaleTicketDirectly",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReport_processed_by",
                table: "ServiceReport",
                column: "processed_by");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReport_service_id",
                table: "ServiceReport",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReport_staff_id",
                table: "ServiceReport",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_branch_id",
                table: "Staffs",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_pool_id",
                table: "Staffs",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_staff_type_id",
                table: "Staffs",
                column: "staff_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_user_id",
                table: "Staffs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_booking_id",
                table: "Ticket",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_issued_by",
                table: "Ticket",
                column: "issued_by");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ticket_type_id",
                table: "Ticket",
                column: "ticket_type_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_role_id",
                table: "Users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_username",
                table: "Users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Account_Ban_Log");

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
                name: "BookingService");

            migrationBuilder.DropTable(
                name: "ComboDetail");

            migrationBuilder.DropTable(
                name: "ContactResponse");

            migrationBuilder.DropTable(
                name: "CustomerCheckin");

            migrationBuilder.DropTable(
                name: "DeviceReport");

            migrationBuilder.DropTable(
                name: "DiscountAuditLog");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PoolImage");

            migrationBuilder.DropTable(
                name: "PoolTicketType");

            migrationBuilder.DropTable(
                name: "SaleTicketDirectly");

            migrationBuilder.DropTable(
                name: "ServiceReport");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "Pool_Device");

            migrationBuilder.DropTable(
                name: "PoolService");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "TicketType");

            migrationBuilder.DropTable(
                name: "Staff_Types");

            migrationBuilder.DropTable(
                name: "Discount");

            migrationBuilder.DropTable(
                name: "Pools");

            migrationBuilder.DropTable(
                name: "Branchs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
