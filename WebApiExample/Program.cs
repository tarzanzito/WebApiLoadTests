
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication2
{
    
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Serilog ini
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
            //Serilog end


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
            
            app.MapGet("/GetSync/{id}", (int id, HttpContext httpContext) =>
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


            app.MapGet("/GetAsync/{id}", async (int id, HttpContext httpContext) =>
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

            app.Run();
        }
    }
}
