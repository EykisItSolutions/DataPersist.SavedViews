namespace DataPersist.SavedViews;

#region Interface

public interface ICurrentUser
{
    int? Id { get; }
    string? FirstName { get; }
    string? LastName { get; }
    string? Email { get; }
    string? Image { get; }

    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsUser { get; }
}
#endregion

#region Implementation

public class CurrentUser : ICurrentUser
{
    // Stubbed out with hard coded numbers.
    // For a fully implemented solution see DataPersist .NET

    public int? Id { get => 1; }
    public string? FirstName { get => "Deborah"; }
    public string? LastName { get => "Walker"; }
    public string? Email { get => "debbie@company.com"; }
    public string? Image { get => "/img/debbie.jpg"; }

    public bool IsAuthenticated { get => true; }
    public bool IsAdmin { get => true; }
    public bool IsUser { get => false; }
}

#endregion
