using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinimalAPIPeliculas.Migrations
{
    /// <inheritdoc />
    public partial class Comentarios2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Peliculas_PeliculasId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_PeliculasId",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "PeliculasId",
                table: "Comentarios");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_PeliculaId",
                table: "Comentarios",
                column: "PeliculaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Peliculas_PeliculaId",
                table: "Comentarios",
                column: "PeliculaId",
                principalTable: "Peliculas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Peliculas_PeliculaId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_PeliculaId",
                table: "Comentarios");

            migrationBuilder.AddColumn<int>(
                name: "PeliculasId",
                table: "Comentarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_PeliculasId",
                table: "Comentarios",
                column: "PeliculasId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Peliculas_PeliculasId",
                table: "Comentarios",
                column: "PeliculasId",
                principalTable: "Peliculas",
                principalColumn: "Id");
        }
    }
}
