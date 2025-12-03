using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Polly;
var builder = Host.CreateApplicationBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERVICIOS (DI)
// ==========================================

// Configuración de Entity Framework (Orchestrator DB)
builder.Services.AddDbContext<OrchestratorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrchestratorDb")));

// Repositorios (Dapper para CEGID)
builder.Services.AddSingleton<CegidRepository>();

// Cliente HTTP para VTEX con Resiliencia (Polly)
builder.Services.AddHttpClient<VtexApiService>(client =>
{
    var vtexSettings = builder.Configuration.GetSection("VtexSettings");
    client.BaseAddress = new Uri(vtexSettings["BaseUrl"]);
    client.DefaultRequestHeaders.Add("X-VTEX-API-AppKey", vtexSettings["AppKey"]);
    client.DefaultRequestHeaders.Add("X-VTEX-API-AppToken", vtexSettings["AppToken"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(GetRetryPolicy());

// Registrar los Workers (Servicios en segundo plano)
builder.Services.AddHostedService<InventorySyncWorker>();
// builder.Services.AddHostedService<OrderWorker>(); // Otros workers...

// ==========================================
// 2. CONSTRUCCIÓN DEL HOST
// ==========================================
var host = builder.Build();

// ==========================================
// 3. SEEDING / MIGRACIÓN INICIAL (Lógica del TXT)
// ==========================================
// Creamos un Scope temporal para obtener el DbContext y ejecutar la carga inicial
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrchestratorDbContext>();

        // Opcional: Ejecutar migraciones pendientes automáticamente al iniciar
        // context.Database.Migrate();

        // Verificamos si la tabla Inventories está vacía antes de cargar
        if (!context.Inventories.Any())
        {
            string fileName = "RelaciónSkuEan.txt";

            // Asegúrate que el archivo se copie al directorio de salida en sus propiedades
            if (File.Exists(fileName))
            {
                Console.WriteLine("Iniciando carga masiva de SKUs desde archivo...");

                var lines = File.ReadAllLines(fileName);
                var inventoriesToAdd = new List<Inventory>();

                foreach (var line in lines.Skip(1)) // Saltar cabecera
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        var skuVtex = parts[0].Trim();
                        var eanCegid = parts[1].Trim();

                        // Validación básica para evitar duplicados en el archivo
                        if (!string.IsNullOrEmpty(skuVtex) && !string.IsNullOrEmpty(eanCegid))
                        {
                            inventoriesToAdd.Add(new Inventory
                            {
                                Sku = skuVtex,
                                Ean = eanCegid,
                                Name = "Carga Inicial " + DateTime.Now.ToShortDateString(),
                                Units = 0,
                                WarehouseId = 1, // ID Bodega Default
                                StateId = 1,      // Activo
                                Reservation = 0,
                                Price = 0
                            });
                        }
                    }
                }

                // Insertar por lotes (Bulk Insert implícito de EF)
                if (inventoriesToAdd.Any())
                {
                    context.Inventories.AddRange(inventoriesToAdd);
                    context.SaveChanges();
                    Console.WriteLine($"Se cargaron {inventoriesToAdd.Count} SKUs exitosamente.");
                }
            }
            else
            {
                Console.WriteLine($"Advertencia: No se encontró el archivo '{fileName}'. Se omitió la carga inicial.");
            }
        }
    }
    catch (Exception ex)
    {
        // Loguear el error de migración pero no detener la app necesariamente
        Console.WriteLine($"Error crítico durante el Seeding de datos: {ex.Message}");
    }
}

// ==========================================
// 4. EJECUCIÓN
// ==========================================
host.Run();


// Helper para políticas de reintento HTTP (Polly)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // Errores 5xx, 408 o fallos de red
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}