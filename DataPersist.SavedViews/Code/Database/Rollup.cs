using DataPersist.SavedViews.Domain;
using Microsoft.EntityFrameworkCore;

namespace DataPersist.SavedViews
{
    // ** Transaction Script Design Pattern

    #region Interface

    public interface IRollup
    {
        Task AllAsync();

        Task InstructorAsync(Instructor instructor);
        Task CourseAsync(Course course);
        Task ClassAsync(Class cls);
        Task EnrollmentAsync(Enrollment enrollment);
        Task QuizAsync(Quiz quiz);
        Task StudentAsync(Student student);

        Task InstructorAsync(int id);
        Task CourseAsync(int id);
        Task ClassAsync(int id);
        Task EnrollmentAsync(int id);
        Task QuizAsync(int id);
        Task StudentAsync(int id);
    }

    #endregion

    public class Rollup : IRollup
    {
        // Performs global, table, and record level rollups (aggregation)

        #region Dependency Injection

        private readonly UniversityContext _db;

        public Rollup(UniversityContext db) { _db = db; }

        #endregion

        #region Global Rollups

        public async Task AllAsync()
        {
            // Rollup database

            await StudentsAsync();
            await EnrollmentsAsync();
            await CoursesAsync();
            await ClassesAsync();
            await InstructorsAsync();
            await QuizzesAsync();
        }

        #endregion

        #region Table Rollups

        private async Task InstructorsAsync()
        {
            string sql =
            $@"UPDATE [Instructor] 
                  SET TotalCourses = (SELECT COUNT(C.Id) 
                                        FROM [Course] C 
                                       WHERE C.InstructorId = [Instructor].Id);";

            await _db.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task CoursesAsync()
        {
            string sql =
            @"UPDATE [Course] 
                 SET TotalClasses = (SELECT COUNT(K.Id) 
                                       FROM [Class] K 
                                      WHERE K.CourseId = C.Id),
                     [Course].Department = D.Name,
                     [Course].Instructor = I.FirstName + ' ' + I.LastName
                FROM [Course] C
                JOIN [Department] D ON (C.DepartmentId = D.Id)
                JOIN [Instructor] I ON (C.InstructorId = I.Id);";

            await _db.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task ClassesAsync()
        {
            string sql =
           @"UPDATE [Class] 
                SET TotalEnrollments = (SELECT COUNT(E.Id) 
                                          FROM [Enrollment] E 
                                         WHERE E.ClassId = K.Id),
                    [Class].Course = C.Title
               FROM [Class] K
               JOIN [Course] C ON (K.CourseId = C.Id);";

            await _db.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task StudentsAsync()
        {
            string sql =
            @"UPDATE [Student] 
                 SET TotalEnrollments = (SELECT COUNT(E.Id) 
                                           FROM [Enrollment] E 
                                          WHERE E.StudentId = S.Id),
                     [Student].Country = C.Name
                FROM [Student] S
                JOIN [Country] C ON S.CountryId = C.Id;";

            await _db.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task EnrollmentsAsync()
        {
            string sql =
           @"UPDATE [Enrollment] 
                SET TotalQuizzes = (SELECT COUNT(Q.Id) 
                                      FROM [Quiz] Q 
                                     WHERE Q.EnrollmentId = E.Id),
                    AvgGrade = (SELECT AVG(Q.Grade) 
                                      FROM [Quiz] Q 
                                     WHERE Q.EnrollmentId = E.Id),
                    [Enrollment].Student = S.FirstName + ' ' + S.LastName,
                    [Enrollment].Course = C.Title,
                    [Enrollment].Fee = C.Fee,
                    [Enrollment].Class = K.Location 
               FROM [Enrollment] E
               JOIN [Student] S ON E.StudentId = S.Id
               JOIN [Course] C ON E.CourseId = C.Id
               JOIN [Class] K ON E.ClassId =  K.Id;";

            await _db.Database.ExecuteSqlRawAsync(sql);
        }
        private async Task QuizzesAsync()
        {
            string sql =
           $@"UPDATE [Quiz] 
                 SET [Quiz].Enrollment = E.EnrollmentNumber
                FROM [Quiz] Q
                JOIN [Enrollment] E ON (Q.EnrollmentId = E.Id);";

            await _db.Database.ExecuteSqlRawAsync(sql);
        }

        #endregion

        #region Record Rollups

        public async Task InstructorAsync(Instructor instructor) => await InstructorAsync(instructor.Id);

        public async Task InstructorAsync(int id)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE [Course] 
                             SET [Course].Instructor = I.FirstName + ' ' + I.LastName
                            FROM [Course] C
                            JOIN [Instructor] I ON (C.InstructorId = I.Id)
                           WHERE C.Id = {id};");
        }

        public async Task CourseAsync(int id)
        {
            var instructor = await _db.Instructors.SingleAsync(i => i.Id == id);
            await InstructorAsync(instructor);
        }

        public async Task CourseAsync(Course course)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE [Instructor] 
                              SET TotalCourses = (SELECT COUNT(C.Id) 
                                                    FROM [Course] C 
                                                   WHERE C.InstructorId = I.Id)
                             FROM [Instructor] I
                             JOIN [Course] C ON C.InstructorId = I.Id
                            WHERE C.InstructorId ={course.InstructorId};");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE [Class] 
                            SET [Class].Course = C.Title
                           FROM [Class] K
                           JOIN [Course] C ON K.CourseId = C.Id
                          WHERE K.CourseId = {course.Id};");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE [Enrollment] 
                            SET [Enrollment].Course = C.Title
                           FROM [Enrollment] E
                           JOIN [Course] C ON E.CourseId = C.Id
                          WHERE E.CourseId = {course.Id};");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE [Course] 
                             SET [Course].Instructor = I.FirstName + ' ' + I.LastName,
                                 [Course].Department = D.Name
                            FROM [Course] C
                            JOIN [Instructor] I ON C.InstructorId = I.Id
                            JOIN [Department] D ON C.DepartmentId = D.Id
                           WHERE C.Id = {course.Id};");
        }

        public async Task ClassAsync(int id)
        {
            var clas = await _db.Classes.SingleAsync(i => i.Id == id);
            await ClassAsync(clas);
        }

        public async Task ClassAsync(Class cls)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Course] 
                        SET TotalClasses = (SELECT COUNT(K.Id) 
                                              FROM [Class] K 
                                             WHERE K.CourseId = C.Id)
                       FROM [Course] C
                      WHERE C.Id = {cls.CourseId};");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Enrollment] 
                          SET [Enrollment].Class = K.Location 
                         FROM [Enrollment] E
                         JOIN [Class] K ON E.ClassId =  K.Id
                        WHERE E.ClassId = {cls.Id};");


        }

        public async Task EnrollmentAsync(int id)
        {
            var enrollment = await _db.Enrollments.SingleAsync(e => e.Id == id);
            await EnrollmentAsync(enrollment);
        }

        public async Task EnrollmentAsync(Enrollment enrollment)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Class] 
                          SET TotalEnrollments = (SELECT COUNT(E.Id) 
                                                  FROM [Enrollment] E 
                                                  WHERE E.ClassId = K.Id)
                        FROM [Class] K
                       WHERE K.Id = {enrollment.ClassId};");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Student] 
                          SET TotalEnrollments = (SELECT COUNT(E.Id) 
                                                  FROM [Enrollment] E 
                                                  WHERE E.StudentId = S.Id)
                        FROM [Student] S
                       WHERE S.Id = {enrollment.StudentId};");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Quiz] 
                          SET [Quiz].Enrollment = E.EnrollmentNumber
                         FROM [Quiz] Q
                         JOIN [Enrollment] E ON Q.EnrollmentId = Q.Id
                        WHERE Q.EnrollmentId = {enrollment.Id}; ");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Enrollment] 
                          SET TotalQuizzes = (SELECT COUNT(Q.Id) 
                                              FROM [Quiz] Q 
                                              WHERE Q.EnrollmentId = E.Id),
                              AvgGrade = (SELECT AVG(Q.Grade) 
                                              FROM [Quiz] Q 
                                              WHERE Q.EnrollmentId = E.Id),
                              [Enrollment].Student = S.FirstName + ' ' + S.LastName,
                              [Enrollment].Course = C.Title,
                              [Enrollment].Fee = C.Fee,
                              [Enrollment].Class = K.Location 
                        FROM [Enrollment] E
                        JOIN [Student] S ON E.StudentId = S.Id
                        JOIN [Course] C ON E.CourseId = C.Id
                        JOIN [Class] K ON E.ClassId =  K.Id
                       WHERE E.Id = {enrollment.Id}");
        }

        public async Task StudentAsync(Student student) => await StudentAsync(student.Id);

        public async Task StudentAsync(int id)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Enrollment] 
                          SET [Enrollment].Student = S.FirstName + ' ' + S.LastName
                         FROM [Enrollment] E
                         JOIN [Student] S ON (E.StudentId = S.Id)
                        WHERE E.StudentId = {id};");
        }

        public async Task QuizAsync(int id)
        {
            var quiz = await _db.Quizzes.SingleAsync(e => e.Id == id);
            await QuizAsync(quiz);
        }

        public async Task QuizAsync(Quiz quiz)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Enrollment] 
                          SET TotalQuizzes = (SELECT COUNT(Q.Id) 
                                              FROM [Quiz] Q 
                                              WHERE Q.EnrollmentId = E.Id), 
                                      
                              AvgGrade = (SELECT AVG(Q.Grade)
                                              FROM [Quiz] Q
                                              WHERE Q.EnrollmentId = E.Id)
                        FROM [Enrollment] E
                       WHERE E.Id = {quiz.EnrollmentId};");

            await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE [Quiz] 
                          SET [Quiz].Enrollment = E.EnrollmentNumber
                         FROM [Quiz] Q
                         JOIN [Enrollment] E ON Q.EnrollmentId = E.Id
                        WHERE Q.Id = {quiz.Id};");
        }

        #endregion
    }
}
