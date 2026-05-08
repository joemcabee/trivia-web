using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TriviaApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "teams",
                schema: "trivia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_teams_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "trivia",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_points",
                schema: "trivia",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    round_id = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_points", x => x.id);
                    table.ForeignKey(
                        name: "FK_team_points_rounds_round_id",
                        column: x => x.round_id,
                        principalSchema: "trivia",
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_points_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "trivia",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_team_points_round_id",
                schema: "trivia",
                table: "team_points",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "ix_team_points_team_id",
                schema: "trivia",
                table: "team_points",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ux_team_points_team_id_round_id",
                schema: "trivia",
                table: "team_points",
                columns: new[] { "team_id", "round_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teams_event_id",
                schema: "trivia",
                table: "teams",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ux_teams_event_id_name",
                schema: "trivia",
                table: "teams",
                columns: new[] { "event_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "team_points",
                schema: "trivia");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "trivia");
        }
    }
}
