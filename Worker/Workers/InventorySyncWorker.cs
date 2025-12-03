using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

public class InventorySyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InventorySyncWorker> _logger;
    private readonly CegidRepository _cegidRepo; // Inyectar Singleton o Scoped con truco

    // Configura cada cuánto corre (ej: 10 minutos)
    private readonly TimeSpan _period = TimeSpan.FromMinutes(10);

    public InventorySyncWorker(IServiceProvider serviceProvider, ILogger<InventorySyncWorker> logger, CegidRepository cegidRepo)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cegidRepo = cegidRepo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("--- INICIO SINCRONIZACIÓN INVENTARIO ---");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // 1. Obtener mapeo desde TU base de datos (No más archivo txt)
                    var dbContext = scope.ServiceProvider.GetRequiredService<OrchestratorDbContext>();
                    var vtexService = scope.ServiceProvider.GetRequiredService<VtexApiService>();

                    // Traemos solo lo necesario para memoria
                    var productsToSync = await dbContext.Inventories
                        .Where(x => x.StateId == 1) // Solo activos
                        .Select(x => new { x.Sku, x.Ean })
                        .ToListAsync(stoppingToken);

                    var eanList = productsToSync.Select(x => x.Ean).Distinct().ToList();

                    if (eanList.Any())
                    {
                        // 2. Consultar CEGID masivamente (Dapper)
                        var cegidStock = await _cegidRepo.GetStockByEansAsync(eanList);

                        // 3. Iterar y Actualizar VTEX
                        foreach (var product in productsToSync)
                        {
                            if (cegidStock.TryGetValue(product.Ean, out int quantity))
                            {
                                // Opcional: Solo actualizar si cambió la cantidad respecto a lo que tenemos en BD
                                // para ahorrar llamadas API

                                await vtexService.UpdateInventoryAsync(product.Sku, quantity);

                                // Opcional: Actualizar tu DB local con el dato fresco
                                // var localInv = await dbContext.Inventories.FirstOrDefaultAsync(x => x.Sku == product.Sku);
                                // localInv.Units = quantity;
                            }
                            else
                            {
                                _logger.LogWarning($"EAN {product.Ean} (SKU {product.Sku}) no encontrado en CEGID.");
                            }
                        }
                        // await dbContext.SaveChangesAsync(); // Si actualizas local
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en proceso de inventario");
            }

            _logger.LogInformation("--- FIN SINCRONIZACIÓN. DORMIR... ---");
            await Task.Delay(_period, stoppingToken);
        }
    }
}