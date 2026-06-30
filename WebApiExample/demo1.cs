using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebApplication2
{
    public class demo1
    {
        public static void Main1(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Base de dados simulada em memória
            var produtos = new List<Produto>
                {
                    new(1, "Teclado Mecânico", 89.99m),
                    new(2, "Rato Sem Fios", 49.99m)
                };

            // 1. GET: Listar todos os produtos
            app.MapGet("/produtos", () => Results.Ok(produtos));

            // 2. GET: Procurar um produto por ID
            app.MapGet("/produtos/{id:int}", (int id) =>
            {
                var produto = produtos.Find(p => p.Id == id);
                return produto is not null ? Results.Ok(produto) : Results.NotFound();
            });

            // 3. POST: Criar um novo produto
            app.MapPost("/produtos", ([FromBody] Produto novoProduto) =>
            {
                produtos.Add(novoProduto);
                return Results.Created($"/produtos/{novoProduto.Id}", novoProduto);
            });

            // 4. PUT: Atualizar um produto existente
            app.MapPut("/produtos/{id:int}", (int id, [FromBody] Produto produtoAtualizado) =>
            {
                var index = produtos.FindIndex(p => p.Id == id);
                if (index == -1) return Results.NotFound();

                produtos[index] = produtoAtualizado with { Id = id }; // Garante o ID correto
                return Results.NoContent();
            });

            // 5. DELETE: Remover um produto
            app.MapDelete("/produtos/{id:int}", (int id) =>
            {
                var produto = produtos.Find(p => p.Id == id);
                if (produto is null) return Results.NotFound();

                produtos.Remove(produto);
                return Results.NoContent();
            });

            app.Run();
        }
    }

    // Modelo de dados usando C# Records
    public record Produto(int Id, string Nome, decimal Preco);

}