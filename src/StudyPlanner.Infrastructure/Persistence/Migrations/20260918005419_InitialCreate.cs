using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "boards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "question_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChosenAnswer = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "integer", nullable: true),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_attempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "question_topics",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_topics", x => new { x.QuestionId, x.TopicId });
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Statement = table.Column<string>(type: "text", nullable: false),
                    AlternativesJson = table.Column<string>(type: "text", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "study_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "study_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyPlanItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "topic_mastery",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mastery = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    AttemptsCount = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_mastery", x => new { x.UserId, x.TopicId });
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subjects_exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_plan_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    PriorityScore = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plan_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_plan_items_study_plans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalTable: "study_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentTopicId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EstimatedIncidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_topics_subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_topics_topics_ParentTopicId",
                        column: x => x.ParentTopicId,
                        principalTable: "topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_boards_Name",
                table: "boards",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exams_UserId_ExamDate",
                table: "exams",
                columns: new[] { "UserId", "ExamDate" });

            migrationBuilder.CreateIndex(
                name: "IX_question_attempts_UserId_AttemptedAt",
                table: "question_attempts",
                columns: new[] { "UserId", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_question_attempts_UserId_QuestionId",
                table: "question_attempts",
                columns: new[] { "UserId", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_question_topics_TopicId",
                table: "question_topics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_questions_BoardId",
                table: "questions",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_questions_Source",
                table: "questions",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_study_plan_items_StudyPlanId_ScheduledDate",
                table: "study_plan_items",
                columns: new[] { "StudyPlanId", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_UserId_ExamId_IsActive",
                table: "study_plans",
                columns: new[] { "UserId", "ExamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_study_sessions_UserId_TopicId_StartedAt",
                table: "study_sessions",
                columns: new[] { "UserId", "TopicId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_subjects_ExamId",
                table: "subjects",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_topics_ParentTopicId",
                table: "topics",
                column: "ParentTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_topics_SubjectId",
                table: "topics",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boards");

            migrationBuilder.DropTable(
                name: "question_attempts");

            migrationBuilder.DropTable(
                name: "question_topics");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "study_plan_items");

            migrationBuilder.DropTable(
                name: "study_sessions");

            migrationBuilder.DropTable(
                name: "topic_mastery");

            migrationBuilder.DropTable(
                name: "topics");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "study_plans");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "exams");
        }
    }
}
