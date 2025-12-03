using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CegidRepository
{
    private readonly string _connectionString;

    public CegidRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CegidLegacyDb");
    }

    public async Task<Dictionary<string, int>> GetStockByEansAsync(List<string> eans)
    {
        var result = new Dictionary<string, int>();

        // Chunking igual que en tu Python (lotes de 1000) para no romper SQL
        var chunks = eans.Chunk(1000);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var batch in chunks)
        {
            // Dapper maneja el "IN" automáticamente
            var query = @"
                SELECT 
                    GA_CODEBARRE AS Ean, 
                    CAST(ISNULL(DISPO_REP.GQ_PHYSIQUE, 0) AS INT) AS Units 
                FROM DISPO_REP WITH(NOLOCK)
                LEFT JOIN ARTICLE ON GA_ARTICLE = GQ_ARTICLE
                WHERE GQ_DEPOT = 'CO2003'
                AND GA_CODEBARRE IN @Eans";

            var rows = await connection.QueryAsync<(string Ean, int Units)>(query, new { Eans = batch });

            foreach (var row in rows)
            {
                // Manejo de duplicados: si un EAN sale 2 veces, tomamos el último o sumamos
                result[row.Ean] = row.Units;
            }
        }

        return result;
    }
}