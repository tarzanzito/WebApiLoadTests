using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2
{
    public class Demo2
    {
        public static void Main2(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Local simulation of an async repository/database 
            var todoService = new TodoService();


            //public static RouteHandlerBuilder MapGet( Delegate handler)
            //{
            //    return endpoints.MapMethods(pattern, GetVerb, handler);
            //}

            // GET: Fetch data asynchronously
            //app.MapGet("/todos", async delegate ()
            app.MapGet("/todos", async () =>
            {
                var todos = await todoService.GetAllAsync();
                return Results.Ok(todos);
            });

            // GET by ID: Fetch specific item asynchronously
            app.MapGet("/todos/{id:int}", async (int id) =>
            {
                var todo = await todoService.GetByIdAsync(id);
                return todo is not null ? Results.Ok(todo) : Results.NotFound();
            });

            // POST: Accept payload and persist asynchronously

            //public delegate Task RequestDelegate(HttpContext context);
            //public static IEndpointConventionBuilder MapPost(
            //    this IEndpointRouteBuilder endpoints,
            //    [StringSyntax("Route")] string pattern,
            //    RequestDelegate requestDelegate)
            //{ 
            //return MapMethods(endpoints, pattern, PostVerb, requestDelegate);
            //}




        app.MapPost("/todos", async ([FromBody] TodoItem item) =>
            {
                var createdItem = await todoService.CreateAsync(item);
                return Results.Created($"/todos/{createdItem.Id}", createdItem);
            });

            app.Run();
        }
        // Data & Logic Infrastructure
        
    }
    public record TodoItem(int Id, string Title, bool IsCompleted);

    public class TodoService
    {
        private readonly List<TodoItem> _store = [
            new TodoItem(1, "Learn Minimal APIs", true),
             new TodoItem(2, "Master Async/Await", false)
        ];

        public async Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            // Simulates unblocking I/O network delay (e.g., DB query)
            await Task.Delay(50);
            return _store;
        }

        public async Task<TodoItem?> GetByIdAsync(int id)
        {
            await Task.Delay(50);
            return _store.FirstOrDefault(t => t.Id == id);
        }

        public async Task<TodoItem> CreateAsync(TodoItem item)
        {
            await Task.Delay(50);
            var newItem = item with { Id = _store.Count + 1 };
            _store.Add(newItem);
            return newItem;
        }
    }
}