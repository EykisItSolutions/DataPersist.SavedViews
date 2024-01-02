using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace DataPersist.SavedViews;

public static class EfCoreContextExtensions
{
    public static DataTable DataTable(this DbContext db, string sql, params object?[] parms)
    {
        var connection = db.Database.GetDbConnection();
        var dbFactory = DbProviderFactories.GetFactory(connection);
        
        var dataTable = new DataTable();
        using (var cmd = dbFactory!.CreateCommand())
        {
            cmd!.Connection = connection;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;

            cmd.AddParameters(parms);
           
            using (var adapter = dbFactory!.CreateDataAdapter())
            {
                adapter!.SelectCommand = cmd;
                adapter.Fill(dataTable);
            }
        }
        return dataTable;
    }

    public static DataRow? DataRow(this DbContext db, string sql, params object?[] parms)
    {
        var dataTable = db.DataTable(sql, parms);

        return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
    }

    private static void AddParameters(this DbCommand command, params object?[] parms)
    {
        if (parms != null && parms.Any())
        {
            // named parameters. Used in INSERT, UPDATE, DELETE
            string firstParam = parms[0]!.ToString()!;

            if (!string.IsNullOrEmpty(firstParam) && firstParam.StartsWith("@"))
            {
                for (int i = 0; i < parms.Length; i += 2)
                {
                    var p = command.CreateParameter();

                    p.ParameterName = parms[i]!.ToString()!;

                    // No empty strings to the database
                    if (parms[i + 1] is string && parms[i + 1]!.ToString()! == "")
                        parms[i + 1] = null;

                    p.Value = parms[i + 1] ?? DBNull.Value;

                    command.Parameters.Add(p);
                }
            }
            else  // ordinal parameters. Used in SELECT or possibly custom EXECUTE statements
            {
                for (int i = 0; i < parms.Length; i++)
                {
                    // Allow no empty strings going to the database
                    if (parms[i] is string && parms[i]!.ToString()! == "")
                        parms[i] = null;

                    var p = command.CreateParameter();
                    p.ParameterName = "@" + i;
                    p.Value = parms[i] ?? DBNull.Value;

                    command.Parameters.Add(p);
                }
            }
        }
    }
}