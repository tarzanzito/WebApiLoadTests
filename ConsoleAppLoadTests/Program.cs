using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        private static HttpClient? _httpClient;

        internal static async Task Main(string[] args)
        {
            //Serilog Ini
            
            //1 - Sync
            //Log.Logger = new LoggerConfiguration()
            //.MinimumLevel.Debug()
            //.WriteTo.Console()
            //.WriteTo.File("../../../logs/Console1.txt", rollingInterval: RollingInterval.Day)
            //.CreateLogger();

            //2 - Asyn
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File("../../../logs/Console1.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();

            //Serilog End

            Console.WriteLine("Wait until Server API Rest started...");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            await LoadTestAsync();
        }

        //using async
        internal static async Task LoadTestAsync()
        {
            Stopwatch watch = new Stopwatch();

            int totalCalls = 5000;
            int limitCallsAtSameTime = 5;

            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5), // Reutiliza conexões
                MaxConnectionsPerServer = limitCallsAtSameTime
            };
            using (_httpClient = new HttpClient(handler));

            var results = new ConcurrentBag<string>();
            var tasks1 = new Task[totalCalls];


            var resp1 = await TaskSimpleAsync(20); //desta forma a task inicia de imediato

            var xtasks = TaskSimpleAsync(20); //desta forma a task inicia de imediato
            var resp = await xtasks;



            for (int i = 0; i < totalCalls; i++)
            {
                int callId = i + 1;
                Log.Information($";SO;Call Task - Id;{callId}");

                tasks1[i] = TaskExampleAsync(callId, results); //desta forma a task inicia de imediato
            }

            Log.Information($";SO;Wait for all tasks ;0");

            await Task.WhenAll(tasks1);

            watch.Stop();

            Log.Information($";SO;All task terminated;0");
            
            Log.Information($";SO;Tempo decorrido (ms);{watch.ElapsedMilliseconds}");
            Log.Information($";SO;Total de respostas processadas;{results.Count}");
        }

        internal static async Task LoadTestLambdaAsync()
        {
            Stopwatch watch = new Stopwatch();

            int totalCalls = 5000;
            int limitCallsAtSameTime = 5;

            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5), // Reuse connections
                MaxConnectionsPerServer = limitCallsAtSameTime
            };
            _httpClient = new HttpClient(handler);


            var results = new ConcurrentBag<string>();
            var tasks1 = new Task[totalCalls];

            Log.Information($";SO;Initialize load tests with total requests =;{totalCalls}");

            for (int i = 0; i < totalCalls; i++)
            {
                int callId = i + 1;
                Log.Information($";SO;Call Task - Id;{callId}");

                //Sim, na grande maioria das vezes, Task.Run(async () => ...) é MAU código e um antipadrão (bad practice) no C#.
                //A comunidade sénior de .NET (incluindo os criadores do padrão async/await) condena este uso na maioria
                //dos cenários devido ao chamado "Async-over-Sync" ou ao desperdício de threads.
                //Aqui está a explicação mecânica de por que isso é considerado mau código:
                //1. O Desperdício de Recursos (Thread Starvation)O propósito do Task.Run é pegar num bloco de código
                //e executá-lo numa thread dedicada do ThreadPool.Quando fazes Task.Run(async () => { await ... }),
                //estás a gastar uma thread inteira apenas para... esperar por outra tarefa assíncrona.
                //A thread do ThreadPool fica bloqueada/ocupada a gerir o estado da máquina de estados do async,
                //em vez de estar livre para processar pedidos reais (como requisições HTTP ou cliques na interface).
                //2. Violação do Princípio AssíncronoO verdadeiro código assíncrono (I/O-bound,
                //como ler uma base de dados ou chamar uma API) não precisa de threads.
                //Ele funciona através de notificações do sistema operativo (gargalo de hardware).
                //Ao usar Task.Run, estás a forçar o uso de CPU onde ele não é necessário.
                tasks1[i] = Task.Run(async () =>
                {
                    Log.Information($";Started Task - startCount;{i}");

                    try
                    {
                        var response = await _httpClient.GetAsync("https://localhost:7195/GetAsync?id={chamadaId}");
                        results.Add($"ID: {callId} | Status: {response.StatusCode}");
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        results.Add($"ID: {callId} | Erro: {ex.Message}");
                    }
                    finally
                    {
                        Log.Information($";Finished Task - startCount;{i}");
                    }
                });
            }

            Log.Information($";SO;Wait for all tasks ;0");

             await Task.WhenAll(tasks1);

            watch.Stop();

            Log.Information($";SO;All task terminated;0");

            Log.Information($";SO;Tempo decorrido (ms);{watch.ElapsedMilliseconds}");
            Log.Information($";SO;Total de respostas processadas;{results.Count}");
        }

        internal static async Task LoadTestSemaphoreAsync()
        {
            Stopwatch watch = new Stopwatch();

            int totalCalls = 5000;
            int limitCallsAtSameTime = 5;

            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5), // Reutiliza conexões
                MaxConnectionsPerServer = limitCallsAtSameTime
            };
            _httpClient = new HttpClient(handler);


            var semaphore = new SemaphoreSlim(limitCallsAtSameTime);
            //var semaphore = new SemaphoreSlim(0, limitCallsAtSameTime); //deu-me erro com o primeiro parametro a zero

            var results = new ConcurrentBag<string>();
            var tasks1 = new Task[totalCalls];


            for (int i = 0; i < totalCalls; i++)
            {
                int callId = i + 1;
                Log.Information($";SO;Call Task - Id;{callId}");

                tasks1[i] = TaskExampleSemaphoreAsync(callId, semaphore, results); //desta forma a task inicia de imediato
            }

            Log.Information($";SO;Wait for all tasks ;0");

            //semaphore.Release(limitCallsAtSameTime); //para quê?

            await Task.WhenAll(tasks1);

            watch.Stop();

            Log.Information($";SO;All task terminated;0");

            Log.Information($";SO;Tempo decorrido (ms);{watch.ElapsedMilliseconds}");
            Log.Information($";SO;Total de respostas processadas;{results.Count}");
        }

        private static async Task TaskExampleAsync(int callId, ConcurrentBag<string> results)
        {
            Log.Information($";SI;Started Task - Id;{callId}");

            try
            {
                //query string
                var response = await _httpClient.GetAsync($"https://localhost:7195/GetQueryStringAsync?id={callId}");
                                               
                //path
                //var response = await _httpClient.GetAsync($"https://localhost:7195/GetAsync/{callId}");

                results.Add($"ID: {callId} | Status: {response.StatusCode}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                results.Add($"ID: {callId} | Erro: {ex.Message}");
            }
            finally
            {
                Log.Information($";FI;Finished Task (Inside) - Id;{callId}");
            }
        }

        private static async Task TaskExampleSemaphoreAsync(int id, SemaphoreSlim semaphore, ConcurrentBag<string> results)
        {
            Log.Information($";SI;Started Task (Inside) - startCount;{id}");

            await semaphore.WaitAsync();

            Log.Information($";SI;Started Task - After semaphore - startCount;{id}");

            try
            {
                var response = await _httpClient.GetAsync($"https://localhost:7195/GetAsync/{id}");
                results.Add($"ID: {id} | Status: {response.StatusCode}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                results.Add($"id: {id} | Error: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
                Log.Information($";FI;Finished Task (Inside) - startCount;{id}");
            }
        }


        private static async Task<string> TaskSimpleAsync(int id)
        {
            Log.Information($";SI;Started Task - id;{id}");

            string  retValue = String.Empty;

            try
            {
                //query string
                //var response = await _httpClient.GetAsync($"https://localhost:7195/GetQueryStringAsync?id={callId}");

                //path
                string url = $"https://localhost:7195/GetSimpleAsync/{id}";
                var httpResponseMessage = await _httpClient.GetAsync(url);

                if (httpResponseMessage == null)
                    throw new HttpRequestException($"HttpClient return null. url={url}");

                httpResponseMessage.EnsureSuccessStatusCode();

                retValue = await httpResponseMessage.Content.ReadAsStringAsync();
                //var jsonValue = await httpResponseMessage.Content.ReadFromJsonAsync<Product>();

                await Task.Delay(1000);

                Log.Information($";FI;Finished Task (Inside) - id;{id}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }

            return retValue;
        }

    }
}





