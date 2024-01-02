using System.Data;
using System.Text.Json;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews;

#region Interface

public interface IHistoryService
{
    Task SaveAsync(int id, Type type, string recordName, string operation);
}
#endregion

public class HistoryService : IHistoryService
{
    #region Dependency Injection

    private readonly UniversityContext _db;
    private readonly ICurrentUser _currentUser;

    public HistoryService(UniversityContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    #endregion

    #region Handlers

    public async Task SaveAsync(int id, Type type, string recordName, string operation)
    {
        // Serialize and save a history record

        string sql = "SELECT * FROM [" + type.Name + "] WHERE Id = @0";
        var row = _db.DataRow(sql, id);

        if (row != null)
        {
            var dictionary = new Dictionary<string, object?>();
            foreach (DataColumn col in row.Table.Columns)
                dictionary.Add(col.ColumnName, row.IsNull(col) ? null : row[col]);

            var json = JsonSerializer.Serialize(dictionary); 

            var history = new History()
            {
                UserId = _currentUser.Id,
                WhatId = id,
                What = type.Name,
                Name = recordName,
                Operation = operation,
                HistoryDate = DateTime.Now,
                Content = json,
                Txn = _db.Database.CurrentTransaction?.TransactionId
            };

            _db.Histories.Add(history);

            await _db.SaveChangesAsync();
        }
    }

    #endregion
}