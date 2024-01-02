using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews;

#region Interface

public interface ISearch
{
    void ReIndexAll();

    public void UpdateStudent(Student student);
    public void DeleteStudent(Student student);

    public void UpdateEnrollment(Enrollment enrollment);
    public void DeleteEnrollment(Enrollment enrollment);

    public void UpdateQuiz(Quiz quiz);
    public void DeleteQuiz(Quiz quiz);

    public void UpdateCourse(Course course);
    public void DeleteCourse(Course course);

    public void UpdateClass(Class clas);
    public void DeleteClass(Class clas);

    public void UpdateInstructor(Instructor instructor);
    public void DeleteInstructor(Instructor instructor);

    public void Update(string what, int id);

    public List<SearchResult> Get(string q, string? what = null);
}

#endregion

public class Search : ISearch
{
    #region Dependency Injection

    private static object locker = new();
    private static Searcher searcher { get; set; } = null!;

    private readonly ICache _cache;
    private readonly UniversityContext _db;

    static string path { get; set; } = null!;

    public Search(IWebHostEnvironment env, ICache cache, UniversityContext db)
    {
        _cache = cache;
        _db = db;

        path = Path.Combine(env.ContentRootPath, "Data");

        // ensure no left-over lock file remains
        var lockFile = Path.Combine(path, "write.lock");
        if (File.Exists(lockFile)) File.Delete(lockFile);
    }

    #endregion

    #region Search

    public List<SearchResult> Get(string q, string? what = null)
    {
        var results = new List<SearchResult>();

        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var searcher = new IndexSearcher(Directory, true))
            {
                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Text", analyzer);

                var query = parser.Parse(q.Replace("*","").Replace("?","") + "*");  // wildcard search by default

                var hits = searcher.Search(query, 200).ScoreDocs;

                foreach (var hit in hits)
                {
                    var document = searcher.Doc(hit.Doc);

                    var tokens = document.Get("Id").Split('-');

                    // Skip any type that is not 'what'
                    if (what is not null && tokens[0] != what) continue;

                    results.Add(new() { What = tokens[0], Id = tokens[1] });
                }
            }
        }

        return results;
    }

    #endregion

    #region ReIndex

    public void ReIndexAll()
    {
        lock (locker)
        {
            try
            {
                using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    // The 'true' is an overwrite flag -- Important for reindexing
                    using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                    {
                        // (re)index all application records
                        foreach (var student in _db.Students.AsNoTracking().ToList()) writer.AddDocument(CreateStudentDocument(student));
                        foreach (var enrollment in _db.Enrollments.AsNoTracking().ToList()) writer.AddDocument(CreateEnrollmentDocument(enrollment));
                        foreach (var clas in _db.Classes.AsNoTracking().ToList()) writer.AddDocument(CreateClassDocument(clas));
                        foreach (var quiz in _db.Quizzes.AsNoTracking().ToList()) writer.AddDocument(CreateQuizDocument(quiz));
                        foreach (var course in _db.Courses.AsNoTracking().Where(c => c.IsDeleted == false).ToList()) writer.AddDocument(CreateCourseDocument(course));
                        foreach (var instructor in _db.Instructors.AsNoTracking().Where(i => i.IsDeleted == false).ToList()) writer.AddDocument(CreateInstructorDocument(instructor));

                        writer.Optimize();
                        writer.Commit();
                    }
                }
            }
            catch (Exception)
            {
                //Service.LogError("In Reindex", ex);
            }
        }
    }

    #endregion

    #region Add, Updates & Deletes

    public void UpdateStudent(Student student)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var doc = CreateStudentDocument(student);
                writer.AddDocument(doc);

                var enrollments = _db.Enrollments.Where(e => e.StudentId == student.Id);
                foreach (var enrollment in enrollments)
                {
                    doc = CreateEnrollmentDocument(enrollment);
                    writer.AddDocument(doc);
                }

                writer.Commit();
            }
        }
    }

    public void DeleteStudent(Student student)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteDocuments(new Term("Id", "Student-" + student.Id));

                writer.Commit();
            }
        }
    }

    public void UpdateEnrollment(Enrollment enrollment)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var doc = CreateEnrollmentDocument(enrollment);
                writer.AddDocument(doc);

                writer.Commit();
            }
        }
    }

    public void DeleteEnrollment(Enrollment enrollment)
    {

        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteDocuments(new Term("Id", "Enrollment-" + enrollment.Id));
                writer.Commit();
            }
        }
    }

    public void UpdateQuiz(Quiz quiz)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var doc = CreateQuizDocument(quiz);
                writer.AddDocument(doc);

                writer.Commit();
            }
        }
    }

    public void DeleteQuiz(Quiz quiz)
    {

        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteDocuments(new Term("Id", "Quiz-" + quiz.Id));

                writer.Commit();
            }
        }
    }

    public void UpdateClass(Class klass)
    {

        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var doc = CreateClassDocument(klass);
                writer.AddDocument(doc);

                writer.Commit();
            }
        }
    }

    public void DeleteClass(Class klass)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteDocuments(new Term("Id", "Class-" + klass.Id));

                writer.Commit();
            }
        }
    }

    public void UpdateCourse(Course course)
    {

        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var doc = CreateCourseDocument(course);
                writer.AddDocument(doc);

                if (course.InstructorId != null)
                {
                    var instructor = _db.Instructors.Single(i => i.Id == course.InstructorId);
                    doc = CreateInstructorDocument(instructor);
                    writer.AddDocument(doc);
                }

                writer.Commit();
            }
        }
    }

    public void DeleteCourse(Course course)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteDocuments(new Term("Id", "Course-" + course.Id));
                writer.Commit();
            }
        }
    }

    public void UpdateInstructor(Instructor instructor)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var doc = CreateInstructorDocument(instructor);
                writer.AddDocument(doc);

                var courses = _db.Courses.Where(c => c.InstructorId == instructor.Id);
                foreach (var course in courses)
                {
                    doc = CreateCourseDocument(course);
                    writer.AddDocument(doc);
                }

                writer.Commit();
            }
        }
    }

    public void DeleteInstructor(Instructor instructor)
    {
        using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
        {
            using (var writer = new IndexWriter(Directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                writer.DeleteDocuments(new Term("Id", "Instructor-" + instructor.Id));
                writer.Commit();
            }
        }
    }

    // 'Generic' update of entity

    public void Update(string what, int id)
    {
        switch (what)
        {
            case "Student": UpdateStudent(_db.Students.Single(s => s.Id == id)); break;
            case "Enrollment": UpdateEnrollment(_db.Enrollments.Single(e => e.Id == id)); break;
            case "Class": UpdateClass(_db.Classes.Single(c => c.Id == id)); break;
            case "Quiz": UpdateQuiz(_db.Quizzes.Single(q => q.Id == id)); break;
            case "Course": UpdateCourse(_db.Courses.Single(c => c.Id == id)); break;
            case "Instructor": UpdateInstructor(_db.Instructors.Single(i => i.Id == id)); break;
        }
    }

    #endregion

    #region Helpers

    private Document CreateStudentDocument(Student student)
    {
        var document = new Document();

        document.Add(new Field("Id", "Student-" + student.Id, Field.Store.YES, Field.Index.NO));

        var text = student.FirstName + " " + student.LastName + ", " + student.Email + ", " + student.City + ", " + student.Country;

        document.Add(new Field("Text", text, Field.Store.YES, Field.Index.ANALYZED));

        return document;
    }

    private Document CreateEnrollmentDocument(Enrollment enrollment)
    {
        var document = new Document();

        document.Add(new Field("Id", "Enrollment-" + enrollment.Id, Field.Store.YES, Field.Index.NO));

        string text = enrollment.EnrollmentNumber + ", " + enrollment.Student + " " + enrollment.Course + " " + enrollment.Class;

        document.Add(new Field("Text", text, Field.Store.YES, Field.Index.ANALYZED));

        return document;
    }

    private Document CreateClassDocument(Class clas)
    {
        var document = new Document();

        document.Add(new Field("Id", "Class-" + clas.Id, Field.Store.YES, Field.Index.NO));

        string text = clas.ClassNumber + " " + clas.Course + " " + clas.Location + " " + clas.StartDate.ToDate() + " - " + clas.EndDate.ToDate();

        document.Add(new Field("Text", text, Field.Store.YES, Field.Index.ANALYZED));

        return document;
    }

    private Document CreateQuizDocument(Quiz quiz)
    {
        var document = new Document();

        document.Add(new Field("Id", "Quiz-" + quiz.Id, Field.Store.YES, Field.Index.NO));

        string text = quiz.QuizNumber + " " + quiz.QuizDate.ToDate() + " - " + quiz.Grade.ToString();

        document.Add(new Field("Text", text, Field.Store.YES, Field.Index.ANALYZED));

        return document;
    }

    private Document CreateCourseDocument(Course course)
    {
        var document = new Document();

        document.Add(new Field("Id", "Course-" + course.Id, Field.Store.YES, Field.Index.NO));

        string instructorName = course.InstructorId == null ? "" : _cache.Instructors[(int)course.InstructorId].FullName;

        string text = course.Title + " " + course.Description + " " + (course.Department ?? "") + " " + instructorName;

        document.Add(new Field("Text", text, Field.Store.YES, Field.Index.ANALYZED));

        return document;
    }

    private Document CreateInstructorDocument(Instructor instructor)
    {
        var document = new Document();

        document.Add(new Field("Id", "Instructor-" + instructor.Id, Field.Store.YES, Field.Index.NO));

        string courses = "";
        foreach (var course in _cache.Courses.Values)
            if (course.InstructorId == instructor.Id) courses += course.Title + " ";

        string text = instructor.FirstName + " " + instructor.LastName + " " + instructor.Alias + " " + instructor.HireDate.ToDate() + (instructor.IsFulltime ? "Fulltime" : "Parttime") + " " + courses;

        document.Add(new Field("Text", text, Field.Store.YES, Field.Index.ANALYZED));

        return document;
    }

    private FSDirectory directory { get; set; } = null!;

    public FSDirectory Directory
    {
        get
        {
            if (directory == null) directory = FSDirectory.Open(new DirectoryInfo(path));
            if (IndexWriter.IsLocked(directory)) IndexWriter.Unlock(directory);

            return directory;
        }
    }

    #endregion
}

public class SearchResult
{
    public string Id { get; set; } = null!;
    public string What { get; set; } = null!;
}
