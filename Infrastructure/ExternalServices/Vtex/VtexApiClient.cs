using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class VtexApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VtexApiService> _logger;

    public VtexApiService(HttpClient httpClient, ILogger<VtexApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task UpdateInventoryAsync(string skuId, int quantity, int warehouseId = 1)
    {
        var url = $"/api/logistics/pvt/inventory/skus/{skuId}/warehouses/{warehouseId}/quantity";

        var payload = new
        {
            quantity = quantity,
            unlimitedQuantity = false,
            dateUtcOnBalanceSystem = (string)null // Opcional
        };

        var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PatchAsync(url, json); // PATCH o PUT según doc de VTEX (Python usaba PATCH)

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error VTEX SKU {skuId}: {response.StatusCode} - {error}");
            }
            else
            {
                _logger.LogInformation($"VTEX Actualizado SKU {skuId}: {quantity} Unidades");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción HTTP VTEX SKU {skuId}: {ex.Message}");
        }
    }
}