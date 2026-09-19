using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace StudyPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEmbeddingDimensionsTo768 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Embeddings existentes foram gerados pelo modelo antigo (384 dimensões) — não têm
            // conversão válida pro novo modelo (768 dimensões, arquitetura diferente). Limpa os
            // chunks e vínculos pra reprocessar do zero; os materiais em si (StudyMaterial) ficam,
            // só perdem o status "Processed" até serem reprocessados.
            migrationBuilder.Sql("DELETE FROM material_topics;");
            migrationBuilder.Sql("DELETE FROM material_chunks;");
            migrationBuilder.Sql("UPDATE study_materials SET \"Status\" = 'Uploaded' WHERE \"Status\" = 'Processed';");

            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "material_chunks",
                type: "vector(768)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(384)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "material_chunks",
                type: "vector(384)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(768)");
        }
    }
}
