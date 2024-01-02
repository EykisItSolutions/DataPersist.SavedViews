using System.ComponentModel.DataAnnotations.Schema;

namespace DataPersist.SavedViews.Domain;

public partial class Instructor
{
    [NotMapped]
    public string FullName
    {
        get
        {
            return FirstName + " " + LastName;
        }
    }
}
