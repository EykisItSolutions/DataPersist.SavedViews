using System.ComponentModel.DataAnnotations.Schema;

namespace DataPersist.SavedViews.Domain;

public partial class Student
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

