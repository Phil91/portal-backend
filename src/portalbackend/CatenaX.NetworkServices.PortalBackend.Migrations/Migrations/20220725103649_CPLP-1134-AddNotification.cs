﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class CPLP1134AddNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_type",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    notification_type_id = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_company_users_creator_id",
                        column: x => x.creator_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_company_users_receiver_id",
                        column: x => x.receiver_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_notification_type_notification_type_id",
                        column: x => x.notification_type_id,
                        principalSchema: "portal",
                        principalTable: "notification_type",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INFO" },
                    { 2, "ACTION" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_creator_user_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_notification_type_id",
                schema: "portal",
                table: "notifications",
                column: "notification_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_receiver_user_id",
                schema: "portal",
                table: "notifications",
                column: "receiver_user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "notification_type",
                schema: "portal");
        }
    }
}