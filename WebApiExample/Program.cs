
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication2
{
    
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Serilog Ini

            //1 - Sync
            //Log.Logger = new LoggerConfiguration()
            //.MinimumLevel.Debug()
            //.WriteTo.Console()
            //.WriteTo.File("logs/RespApi.txt", rollingInterval: RollingInterval.Day)
            //.CreateLogger();

            //2 - Asyn
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File("logs/ResrApi.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();

            //Serilog End


            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            /// Endpoints
            
            app.MapGet("/", () => "Hello world!");
            
            //without async 
            //using route
            app.MapGet("/GetRouteSync/{id}", (int id, HttpContext httpContext) =>
            {
                // public IResult GetSync
                
                Counters.IncrementCounter();
                Counters.IncrementActives();
                Log.Information($";GetSync Ini;{httpContext.TraceIdentifier};ID;{id};Counter;{Counters.GetCounter()};0;{Counters.GetActives()};0");

                Task.Delay(5000).GetAwaiter().GetResult(); //resolve async call in a sync method in top function
                // .ConfigureAwait(false)

                //Contador.totalInUse--;
                Log.Information($";GetSync End;{httpContext.TraceIdentifier};ID;{id};Counter;0;{Counters.GetCounter()};0;{Counters.GetActives()}");
                
                return Results.Ok($"GetSync={id}");
            });

            // Endpoints  - INI //

            //async
            //using route (default)
            //app.MapGet("/GetAsync/{id}", async (int id, HttpContext httpContext) =>
            //or
            app.MapGet("/GetRouteAsync/{id}", async ([FromRoute] int id, HttpContext httpContext) =>
            //public async Task<IResult> GetAsync()
            {
                Counters.IncrementCounter();
                Counters.IncrementActives();
                Log.Information($";GetAsync Ini;{httpContext.TraceIdentifier};ID;{id};Counter;{Counters.GetCounter()};0;{Counters.GetActives()};0");

                await Task.Delay(2000); //.ConfigureAwait(false);

                Counters.DecrementActives();
                Log.Information($";GetAsync End;{httpContext.TraceIdentifier};ID;{id};Counter;0;{Counters.GetCounter()};0;{Counters.GetActives()}");
                
                return Results.Ok($"GetAsync={id}");

            });

            //async
            //using query string
            app.MapGet("/GetQueryStringAsync", async ([FromQuery] int id, HttpContext httpContext) =>
            //public async Task<IResult> GetAsync()
            {
                Counters.IncrementCounter();
                Counters.IncrementActives();
                Log.Information($";GetAsync Ini;{httpContext.TraceIdentifier};ID;{id};Counter;{Counters.GetCounter()};0;{Counters.GetActives()};0");

                await Task.Delay(2000); //.ConfigureAwait(false);

                Counters.DecrementActives();
                Log.Information($";GetAsync End;{httpContext.TraceIdentifier};ID;{id};Counter;0;{Counters.GetCounter()};0;{Counters.GetActives()}");

                return Results.Ok($"GetAsync={id}");

            });

            //async
            //using body get with body is bad idea
            //https://dzone.com/articles/using-a-body-with-an-http-get-method-is-still-a-bad-idea
            app.MapGet("/GetBodyAsync", async ([FromBody] int id, HttpContext httpContext) =>
            //public async Task<IResult> GetAsync()
            {
                Counters.IncrementCounter();
                Counters.IncrementActives();
                Log.Information($";GetAsync Ini;{httpContext.TraceIdentifier};ID;{id};Counter;{Counters.GetCounter()};0;{Counters.GetActives()};0");

                await Task.Delay(2000); //.ConfigureAwait(false);

                Counters.DecrementActives();
                Log.Information($";GetAsync End;{httpContext.TraceIdentifier};ID;{id};Counter;0;{Counters.GetCounter()};0;{Counters.GetActives()}");

                var resp = new { Message = $"GetAsync={id}" };
                var strJson = Results.Json(resp);

                return Results.Ok(strJson);

            });

            app.MapGet("/GetSimpleAsync/{id}", async ([FromRoute] int id, HttpContext httpContext) =>
            //public async Task<IResult> GetAsync()
            {
                Log.Information($";GetAsync Ini;{httpContext.TraceIdentifier};ID;{id}");

                await Task.Delay(2000); //.ConfigureAwait(false);

                Log.Information($";GetAsync End;{httpContext.TraceIdentifier};ID;{id}");

                return Results.Ok($"GetAsync={id}");

            });

            // Endpoints  - END //

            app.Run();
        }
    }
}

//FromHeader
//FromQuery    como mapear para object 
//FromRoute
//FromServices
//FromForm
//AsParameters
//FromBody

//app.MapGet("/{id}", (int id,
//                     int page,
//                     [FromHeader(Name = "X-CUSTOM-HEADER")] string customHeader,
//                     Service service) => { });

//app.MapPost("/", (Person person) => { });

//app.MapGet("/{id}", (HttpRequest request) =>
//{
//    var id = request.RouteValues["id"];
//    var page = request.Query["page"];
//    var customHeader = request.Headers["X-CUSTOM-HEADER"];

//    // ...
//});

//app.MapGet("/{id}", ([FromRoute] int id,
//                     [FromQuery(Name = "p")] int page,
//                     [FromServices] Service service,
//                     [FromHeader(Name = "Content-Type")] string contentType)
//                     => { });


//app.MapPost("/todos", async ([FromForm] string name,
//    [FromForm] Visibility visibility, IFormFile? attachment, TodoDb db) =>
//{
//}
