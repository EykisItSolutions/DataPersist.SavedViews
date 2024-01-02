using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataPersist.SavedViews.Domain
{
    public partial class UniversityContext : DbContext
    {
        #region Dependency Injection

        private ICurrentUser _currentUser = null!;

        public UniversityContext(ICurrentUser currentUser)
        {
            Setup(currentUser);
        }

        public UniversityContext(DbContextOptions<UniversityContext> options,
            ICurrentUser currentUser) : base(options)
        {
            Setup(currentUser);
        }

        private void Setup(ICurrentUser currentUser)
        {
            _currentUser = currentUser;

            // Both StateChanged and Tracked (new) are needed

            ChangeTracker.StateChanged += UpdateAuditFields!;
            ChangeTracker.Tracked += UpdateAuditFields!;
        }

        #endregion

        #region Repositories

        public virtual DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Country> Countries { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<DataLog> DataLogs { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;
        public virtual DbSet<Error> Errors { get; set; } = null!;
        public virtual DbSet<History> Histories { get; set; } = null!;
        public virtual DbSet<Instructor> Instructors { get; set; } = null!;
        public virtual DbSet<Preference> Preferences { get; set; } = null!;
        public virtual DbSet<Quiz> Quizzes { get; set; } = null!;
        public virtual DbSet<Setting> Settings { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<View> Views { get; set; } = null!;
        public virtual DbSet<ViewFilter> ViewFilters { get; set; } = null!;
        public virtual DbSet<ViewSort> ViewSorts { get; set; } = null!;
        public virtual DbSet<Viewed> Vieweds { get; set; } = null!;

        #endregion

        #region OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.ToTable("ActivityLog");

                entity.HasIndex(e => e.LogDate, "IndexActivityLogLogDate");

                entity.HasIndex(e => e.UserId, "IndexActivityLogUserId");

                entity.Property(e => e.Activity).HasMaxLength(255);

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IpAddress).HasMaxLength(50);

                entity.Property(e => e.LogDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Result).HasMaxLength(255);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ActivityLogs)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_ACTIVITY_REFERENCE_USER");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("Class");

                entity.HasIndex(e => e.ClassNumber, "IndexClassClassNumber");

                entity.HasIndex(e => e.Course, "IndexClassCourse");

                entity.HasIndex(e => e.CourseId, "IndexClassCourseId");

                entity.HasIndex(e => e.Location, "IndexClassLocation");

                entity.HasIndex(e => e.MaxEnrollments, "IndexClassMaxEnrollments");

                entity.HasIndex(e => e.StartDate, "IndexClassStartDate");

                entity.HasIndex(e => e.TotalEnrollments, "IndexClassTotalEnrollments");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ClassNumber)
                    .HasMaxLength(30)
                    .HasDefaultValueSql("('New Class')");

                entity.Property(e => e.Course).HasMaxLength(75);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Location).HasMaxLength(50);

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.HasOne(d => d.CourseNavigation)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CLASS_REFERENCE_COURSE");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.HasIndex(e => e.Name, "IndexCountryName");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(60);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");

                entity.HasIndex(e => e.Department, "IndexCourseDepartment");

                entity.HasIndex(e => e.DepartmentId, "IndexCourseDepartmentId");

                entity.HasIndex(e => e.Fee, "IndexCourseFee");

                entity.HasIndex(e => e.Instructor, "IndexCourseInstructor");

                entity.HasIndex(e => e.InstructorId, "IndexCourseInstructorId");

                entity.HasIndex(e => e.NumDays, "IndexCourseNumDays");

                entity.HasIndex(e => e.CourseNumber, "IndexCourseNumber");

                entity.HasIndex(e => e.Title, "IndexCourseTitle");

                entity.HasIndex(e => e.TotalClasses, "IndexCourseTotalClasses");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CourseNumber).HasMaxLength(25);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Department).HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Fee).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.Instructor).HasMaxLength(50);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.DepartmentNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_COURSE_REFERENCE_DEPARTME");

                entity.HasOne(d => d.InstructorNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.InstructorId)
                    .HasConstraintName("FK_COURSE_REFERENCE_INSTRUCT");
            });

            modelBuilder.Entity<DataLog>(entity =>
            {
                entity.ToTable("DataLog");

                entity.HasIndex(e => e.LogDate, "IndexDataLogLogDate");

                entity.HasIndex(e => e.UserId, "IndexDataLogUserId");

                entity.HasIndex(e => new { e.What, e.WhatId }, "IndexDataLogWhatWhatId");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Column).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LogDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.What).HasMaxLength(50);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DataLogs)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_DATALOG_REFERENCE_USER");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department");

                entity.HasIndex(e => e.Name, "IndexDepartmentName");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("Enrollment");

                entity.HasIndex(e => e.AmountPaid, "IndexEnrollmentAmountPaid");

                entity.HasIndex(e => e.AvgGrade, "IndexEnrollmentAvgGrade");

                entity.HasIndex(e => e.Class, "IndexEnrollmentClass");

                entity.HasIndex(e => new { e.ClassId, e.EnrollDate }, "IndexEnrollmentClassIdEnrollDate");

                entity.HasIndex(e => e.Course, "IndexEnrollmentCourse");

                entity.HasIndex(e => e.CourseId, "IndexEnrollmentCourseId");

                entity.HasIndex(e => e.EnrollmentNumber, "IndexEnrollmentEnrollmentNumber")
                    .IsUnique();

                entity.HasIndex(e => e.Fee, "IndexEnrollmentFee");

                entity.HasIndex(e => e.Student, "IndexEnrollmentStudent");

                entity.HasIndex(e => new { e.StudentId, e.ClassId }, "IndexEnrollmentStudentIdClassId")
                    .IsUnique();

                entity.HasIndex(e => e.TotalQuizzes, "IndexEnrollmentTotalQuizzes");

                entity.HasIndex(e => e.EnrollDate, "IndexEnrollmentsEnrollDate");

                entity.Property(e => e.AmountPaid).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.AvgGrade).HasColumnType("decimal(6, 2)");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Class).HasMaxLength(50);

                entity.Property(e => e.Course).HasMaxLength(75);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EnrollDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EnrollmentNumber)
                    .HasMaxLength(30)
                    .HasDefaultValueSql("('New Enrollment')");

                entity.Property(e => e.Fee).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.Status)
                    .HasMaxLength(15)
                    .HasDefaultValueSql("('Pending')");

                entity.Property(e => e.Student).HasMaxLength(60);

                entity.HasOne(d => d.ClassNavigation)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ENROLLME_REFERENCE_CLASS");

                entity.HasOne(d => d.StudentNavigation)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ENROLLME_REFERENCE_STUDENT");
            });

            modelBuilder.Entity<Error>(entity =>
            {
                entity.ToTable("Error");

                entity.HasIndex(e => e.ErrorDate, "IndexErrorErrorDate");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ErrorDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.HttpReferer).HasMaxLength(400);

                entity.Property(e => e.IpAddress).HasMaxLength(40);

                entity.Property(e => e.Message).HasMaxLength(600);

                entity.Property(e => e.Url).HasMaxLength(400);

                entity.Property(e => e.UserAgent).HasMaxLength(400);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Errors)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_ERROR_REFERENCE_USER");
            });

            modelBuilder.Entity<History>(entity =>
            {
                entity.ToTable("History");

                entity.HasIndex(e => e.HistoryDate, "IndexHistoryHistoryDate");

                entity.HasIndex(e => e.Txn, "IndexHistoryTxn");

                entity.HasIndex(e => new { e.UserId, e.HistoryDate }, "IndexHistoryUserIdHistoryDate");

                entity.HasIndex(e => new { e.What, e.WhatId }, "IndexHistoryWhatWhatId");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.HistoryDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(75);

                entity.Property(e => e.Operation)
                    .HasMaxLength(25)
                    .HasDefaultValueSql("('UPDATE')");

                entity.Property(e => e.What).HasMaxLength(50);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Histories)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_HISTORY_REFERENCE_USER");
            });

            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.ToTable("Instructor");

                entity.HasIndex(e => e.Alias, "IndexInstructorAlias");

                entity.HasIndex(e => e.DeletedOn, "IndexInstructorDeletedOn");

                entity.HasIndex(e => e.Email, "IndexInstructorEmail")
                    .IsUnique();

                entity.HasIndex(e => e.ExternalId, "IndexInstructorExternalID");

                entity.HasIndex(e => e.HireDate, "IndexInstructorHireDate");

                entity.HasIndex(e => new { e.LastName, e.FirstName }, "IndexInstructorName");

                entity.HasIndex(e => e.Salary, "IndexInstructorSalary");

                entity.HasIndex(e => e.TotalCourses, "IndexInstructorTotalCourses");

                entity.Property(e => e.Alias).HasMaxLength(10);

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.ExternalId)
                    .HasMaxLength(30)
                    .HasColumnName("ExternalID");

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.HireDate).HasColumnType("date");

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Salary).HasColumnType("decimal(12, 2)");
            });

            modelBuilder.Entity<Preference>(entity =>
            {
                entity.ToTable("Preference");

                entity.HasIndex(e => e.UserId, "IndexPreferenceUserId");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FontSize).HasDefaultValueSql("((10))");

                entity.Property(e => e.PageLayout)
                    .HasMaxLength(30)
                    .HasDefaultValueSql("('10')");

                entity.Property(e => e.TimeZone).HasMaxLength(30);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Preferences)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PREFEREN_REFERENCE_USER");
            });

            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.ToTable("Quiz");

                entity.HasIndex(e => new { e.EnrollmentId, e.QuizDate }, "IndexQuizEnrollIdQuizDate");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Enrollment).HasMaxLength(30);

                entity.Property(e => e.Grade).HasColumnType("decimal(6, 2)");

                entity.Property(e => e.QuizDate).HasColumnType("date");

                entity.Property(e => e.QuizNumber)
                    .HasMaxLength(30)
                    .HasDefaultValueSql("('New Quiz')");

                entity.HasOne(d => d.EnrollmentNavigation)
                    .WithMany(p => p.Quizzes)
                    .HasForeignKey(d => d.EnrollmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_QUIZ_REFERENCE_ENROLLME");
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.ToTable("Setting");

                entity.HasIndex(e => e.Name, "IndexSettingName");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LastChangeDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Value).HasMaxLength(255);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Student");

                entity.HasIndex(e => new { e.LastName, e.FirstName }, "IndexStudenName");

                entity.HasIndex(e => e.Alias, "IndexStudentAlias");

                entity.HasIndex(e => e.City, "IndexStudentCity");

                entity.HasIndex(e => e.Country, "IndexStudentCountry");

                entity.HasIndex(e => e.DateOfBirth, "IndexStudentDateOfBirth");

                entity.HasIndex(e => e.Email, "IndexStudentEmail")
                    .IsUnique();

                entity.HasIndex(e => e.TotalEnrollments, "IndexStudentTotalEnrollments");

                entity.Property(e => e.Alias).HasMaxLength(10);

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.Country).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateOfBirth).HasColumnType("date");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.Gender).HasMaxLength(10);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.HasOne(d => d.CountryNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_STUDENT_REFERENCE_COUNTRY");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Email, "IndexUserEmail");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.Image).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Password).HasMaxLength(50);
            });

            modelBuilder.Entity<View>(entity =>
            {
                entity.ToTable("View");

                entity.HasIndex(e => e.Name, "IndexViewName");

                entity.HasIndex(e => e.What, "IndexViewWhat");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.What).HasMaxLength(50);
            });

            modelBuilder.Entity<ViewFilter>(entity =>
            {
                entity.ToTable("ViewFilter");

                entity.HasIndex(e => new { e.ViewId, e.Number }, "IndexViewFilterViewIdNumber");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Clause).HasMaxLength(250);

                entity.Property(e => e.Column).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Operator).HasMaxLength(30);

                entity.Property(e => e.Value).HasMaxLength(100);

                entity.HasOne(d => d.View)
                    .WithMany(p => p.ViewFilters)
                    .HasForeignKey(d => d.ViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VIEWFILT_REFERENCE_VIEW");
            });

            modelBuilder.Entity<ViewSort>(entity =>
            {
                entity.ToTable("ViewSort");

                entity.HasIndex(e => new { e.ViewId, e.Number }, "IndexViewSortViewIdNumber");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Clause).HasMaxLength(100);

                entity.Property(e => e.Column).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Direction)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('Ascending')");

                entity.HasOne(d => d.View)
                    .WithMany(p => p.ViewSorts)
                    .HasForeignKey(d => d.ViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VIEWSORT_REF1_VIEW");
            });

            modelBuilder.Entity<Viewed>(entity =>
            {
                entity.ToTable("Viewed");

                entity.HasIndex(e => new { e.UserId, e.WhatId, e.WhatType }, "IndexViewedUserWhatWhat");

                entity.HasIndex(e => new { e.WhatId, e.WhatType }, "IndexViewedWhatWhat");

                entity.Property(e => e.ChangedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ViewDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.WhatName).HasMaxLength(150);

                entity.Property(e => e.WhatType).HasMaxLength(30);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Vieweds)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VIEWED_REFERENCE_USER");
            });

            // ** Soft delete pattern

            modelBuilder.Entity<Course>().HasQueryFilter(i => !i.IsDeleted);
            modelBuilder.Entity<Instructor>().HasQueryFilter(i => !i.IsDeleted);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        #endregion

        #region Context Overrides

        public override int SaveChanges()
        {
            UpdateSoftDeleteColumns();
            //UpdateAuditColumns();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateSoftDeleteColumns();
            //UpdateAuditColumns();

            return base.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Helpers

        private void UpdateSoftDeleteColumns()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                var type = entry.Entity?.GetType().Name;

                // Handle soft deletes

                switch (type)
                {
                    case "Course":
                    case "Instructor":

                        switch (entry.State)
                        {
                            case EntityState.Deleted:

                                entry.State = EntityState.Modified; 

                                entry.CurrentValues["DeletedOn"] = DateTime.Now;
                                entry.CurrentValues["DeletedBy"] = _currentUser.Id;
                                entry.CurrentValues["IsDeleted"] = true;
                                break;
                        }
                        break;
                }
            }
        }

        private void UpdateAuditFields(object sender, EntityEntryEventArgs e)
        {
            switch (e.Entry.State)
            {
                case EntityState.Added:
                    e.Entry.CurrentValues["CreatedOn"] = DateTime.Now;
                    e.Entry.CurrentValues["CreatedBy"] = _currentUser.Id;
                    break;
                case EntityState.Modified:
                    e.Entry.CurrentValues["ChangedOn"] = DateTime.Now;
                    e.Entry.CurrentValues["ChangedBy"] = _currentUser.Id;
                    break;
            }
        }

        #endregion
    }
}
