using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace DataPersist.SavedViews.Areas.Admin;

public class Adhoc : BaseModel
{
    #region Data

    public Dictionary<string, Dictionary<string, Column>> Schema { get; set; } = null!;

    public string? Sql { get; set; }
    public string? Exception { get; set; }

    public string? CurrentBuiltin { get; set; }

    public List<List<string?>> Results { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> PostAsync()
    {
        // ** SQL Injection pattern
        // basic protection against sql injection

        if (Sql == null || Sql.Length < 12)
        {
            Failure = "Invalid query";
            return View(this);
        }

        if (Sql.IndexOf(";") > -1)
        {
            Failure = "Illegal query - cannot use ';'";
            return View(this);
        }

        if (Sql.IndexOf("--") > -1)
        {
            Failure = "Illegal query - cannot use '--'";
            return View(this);
        }

        // Only these 4 CRUD operations are supported

        var action = Sql.Trim().ToLower().Substring(0, 10);
        if (!(action.StartsWith("select") ||
              action.StartsWith("update") ||
              action.StartsWith("insert") ||
              action.StartsWith("delete")))
        {
            Failure = "Invalid query";
            return View(this);
        }

        try
        {
            if (action.StartsWith("select"))
            {
                using var connection = new SqlConnection(_db.Database.GetConnectionString());

                var rows = (await connection.QueryAsync<dynamic>(Sql)).ToList();

                bool first = true;

                foreach (var row in rows)
                {
                    // Column headers

                    if (first) 
                    {
                        var headers = new List<string?>();

                        foreach (var column in (IDictionary<string, object>)row)  // get column names
                        {
                            headers.Add(column.Key);
                        }

                        Results.Add(headers);
                        first = false;
                    }

                    // Column names

                    var values = new List<string?>();

                    foreach (var column in (IDictionary<string, object>)row)  // get column values
                    {
                        string? value = column.Value == null ? "" : column.Value.ToString();
                        values.Add(value);
                    }
                    Results.Add(values);
                }
            }
            else
            {
                // Enable if secure.

                //db.Execute(sql);
                Results.Add(new List<string?> { "At this time we only support SELECT." });
            }
        }
        catch (Exception ex)
        {
            Failure = ex.Message;
        }

   
        return View(this);
    }
   
    #endregion
}

