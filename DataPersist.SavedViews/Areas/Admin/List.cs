using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace DataPersist.SavedViews.Areas.Admin;

public class List : BaseModel
{
    #region Data

    public List<TableStats> Tables { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        await GetTableStatisticsAsync();

        return View(this);
    }

    #endregion

    #region Helpers

    private async Task GetTableStatisticsAsync()
    {
        // Get record counts and storage info for all tables in a single operation.

        var sql = @"SELECT T.NAME AS [Table],
                           P.rows AS [Rows],
                           SUM(A.total_pages) * 8 AS KB
                      FROM sys.tables T
                      JOIN sys.indexes I ON T.object_id = I.object_id
                      JOIN sys.partitions P ON I.object_id = P.object_id AND I.index_id = P.index_id
                      JOIN sys.allocation_units A ON P.partition_id = A.container_id
                     WHERE T.NAME NOT LIKE 'dt%' 
                       AND T.is_ms_shipped = 0
                       AND I.object_id > 255 
                     GROUP BY T.Name,  P.Rows
                     ORDER BY T.Name";

        // Dapper query

        using var connection = new SqlConnection(_db.Database.GetConnectionString());

        Tables = (await connection.QueryAsync<TableStats>(sql)).ToList();
    }

    #endregion
}

public record TableStats
{
    public string Table { get; set; } = null!;
    public long Rows { get; set; }
    public long KB { get; set; }
}