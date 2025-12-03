using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

using Infrastructure.Persistence;
using Domain.Entities;

var builder = Host.CreateApplicationBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERVICIOS (DI)
// ==========================================

// Configuración de Entity Framework
var connectionString = builder.Configuration.GetConnectionString("OrchestratorDb");
builder.Services.AddDbContext<OrchestratorDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repositorios
builder.Services.AddSingleton<CegidRepository>();

// Cliente HTTP para VTEX
builder.Services.AddHttpClient<VtexApiService>(client =>
{
    var vtexSettings = builder.Configuration.GetSection("VtexSettings");

    // Validación de nulos para evitar errores en tiempo de ejecución
    string baseUrl = vtexSettings["BaseUrl"] ?? throw new ArgumentNullException("VtexSettings:BaseUrl no configurado");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", vtexSettings["AppKey"]);
    client.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", vtexSettings["AppToken"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(GetRetryPolicy());

// Registrar los Workers
builder.Services.AddHostedService<InventorySyncWorker>();

// ==========================================
// 2. CONSTRUCCIÓN DEL HOST
// ==========================================
var host = builder.Build();

// ==========================================
// 3. SEEDING / CARGA INICIAL
// ==========================================
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrchestratorDbContext>();

        // Crear la DB si no existe (útil para pruebas locales)
        // context.Database.EnsureCreated(); 

        if (!context.Inventories.Any())
        {
            string fileName = "RelaciónSkuEan.txt";

            if (File.Exists(fileName))
            {
                Console.WriteLine($"[SEED] Iniciando carga desde {fileName}...");

                var lines = File.ReadAllLines(fileName);
                var inventoriesToAdd = new List<Inventory>();

                foreach (var line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        var skuVtex = parts[0].Trim();
                        var eanCegid = parts[1].Trim();

                        if (!string.IsNullOrEmpty(skuVtex) && !string.IsNullOrEmpty(eanCegid))
                        {
                            inventoriesToAdd.Add(new Inventory
                            {
                                Sku = skuVtex,
                                Ean = eanCegid,
                                Name = "Carga Inicial",
                                Units = 0,
                                WarehouseId = 1,
                                StateId = 1,
                                Reservation = 0,
                                Price = 0
                            });
                        }
                    }
                }

                if (inventoriesToAdd.Any())
                {
                    context.Inventories.AddRange(inventoriesToAdd);
                    context.SaveChanges();
                    Console.WriteLine($"[SEED] Éxito: {inventoriesToAdd.Count} SKUs cargados.");
                }
            }
            else
            {
                Console.WriteLine($"[SEED] Advertencia: No se encontró '{fileName}'. Asegúrate de copiarlo al directorio de salida.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SEED] Error crítico: {ex.Message}");
    }
}

host.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}