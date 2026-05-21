using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;

namespace PRN232.Lab1.Repositories.Data
{
    public static class DbSeeder
    {
        private static readonly DateTime BaseDate = new(2024, 1, 1);

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await EnsureSemestersAsync(context);
            await EnsureSubjectsAsync(context);
            await EnsureCoursesAsync(context);
            await EnsureStudentsAsync(context);
            await EnsureEnrollmentsAsync(context);
            await FillMissingDataAsync(context);
        }

        private static async Task EnsureSemestersAsync(ApplicationDbContext context)
        {
            var existingCount = await context.Semesters.CountAsync();
            if (existingCount >= 5)
            {
                return;
            }

            var semesters = Enumerable.Range(existingCount + 1, 5 - existingCount)
                .Select(i => new Semester
                {
                    SemesterName = $"Semester {i}",
                    StartDate = BaseDate.AddMonths((i - 1) * 4),
                    EndDate = BaseDate.AddMonths((i - 1) * 4 + 4).AddDays(-1)
                });

            await context.Semesters.AddRangeAsync(semesters);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureSubjectsAsync(ApplicationDbContext context)
        {
            var existingCount = await context.Subjects.CountAsync();
            if (existingCount >= 10)
            {
                return;
            }

            var subjectNames = new[]
            {
                "Programming Fundamentals",
                "Object Oriented Programming",
                "Database Systems",
                "Web API Development",
                "Software Engineering",
                "Cloud Computing",
                "Data Structures",
                "Operating Systems",
                "Computer Networks",
                "Mobile Development"
            };

            var subjects = Enumerable.Range(existingCount + 1, 10 - existingCount)
                .Select(i => new Subject
                {
                    SubjectCode = $"PRN{i:000}",
                    SubjectName = subjectNames[i - 1],
                    Credit = i % 3 == 0 ? 4 : 3
                });

            await context.Subjects.AddRangeAsync(subjects);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureCoursesAsync(ApplicationDbContext context)
        {
            var existingCount = await context.Courses.CountAsync();
            if (existingCount >= 20)
            {
                return;
            }

            var semesters = await context.Semesters.OrderBy(x => x.SemesterId).ToListAsync();
            var subjects = await context.Subjects.OrderBy(x => x.SubjectId).ToListAsync();

            var courses = Enumerable.Range(existingCount + 1, 20 - existingCount)
                .Select(i =>
                {
                    var subject = subjects[(i - 1) % subjects.Count];
                    var semester = semesters[(i - 1) % semesters.Count];

                    return new Course
                    {
                        CourseName = $"{subject.SubjectCode}-{semester.SemesterName}",
                        SemesterId = semester.SemesterId,
                        SubjectId = subject.SubjectId
                    };
                });

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureStudentsAsync(ApplicationDbContext context)
        {
            var existingCount = await context.Students.CountAsync();
            if (existingCount >= 50)
            {
                return;
            }

            var students = Enumerable.Range(existingCount + 1, 50 - existingCount)
                .Select(i => new Student
                {
                    FullName = $"Student {i:00}",
                    Email = $"student{i:00}@example.com",
                    DateOfBirth = new DateTime(2000, 1, 1).AddDays(i * 23)
                });

            await context.Students.AddRangeAsync(students);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureEnrollmentsAsync(ApplicationDbContext context)
        {
            var existingCount = await context.Enrollments.CountAsync();
            if (existingCount >= 500)
            {
                return;
            }

            var students = await context.Students.OrderBy(x => x.StudentId).ToListAsync();
            var courses = await context.Courses.OrderBy(x => x.CourseId).ToListAsync();
            var statuses = new[] { "Active", "Completed", "Dropped" };

            var enrollments = Enumerable.Range(existingCount + 1, 500 - existingCount)
                .Select(i => new Enrollment
                {
                    StudentId = students[(i - 1) % students.Count].StudentId,
                    CourseId = courses[((i - 1) * 7) % courses.Count].CourseId,
                    EnrollDate = BaseDate.AddDays(i),
                    Status = statuses[(i - 1) % statuses.Length]
                });

            await context.Enrollments.AddRangeAsync(enrollments);
            await context.SaveChangesAsync();
        }

        private static async Task FillMissingDataAsync(ApplicationDbContext context)
        {
            var hasChanges = false;

            var semesters = await context.Semesters
                .OrderBy(x => x.SemesterId)
                .ToListAsync();

            for (var i = 0; i < semesters.Count; i++)
            {
                var semester = semesters[i];

                if (string.IsNullOrWhiteSpace(semester.SemesterName))
                {
                    semester.SemesterName = $"Semester {i + 1}";
                    hasChanges = true;
                }

                if (semester.StartDate == default)
                {
                    semester.StartDate = BaseDate.AddMonths(i * 4);
                    hasChanges = true;
                }

                if (semester.EndDate == default || semester.EndDate < semester.StartDate)
                {
                    semester.EndDate = semester.StartDate.AddMonths(4).AddDays(-1);
                    hasChanges = true;
                }
            }

            var subjects = await context.Subjects
                .OrderBy(x => x.SubjectId)
                .ToListAsync();

            for (var i = 0; i < subjects.Count; i++)
            {
                var subject = subjects[i];

                if (string.IsNullOrWhiteSpace(subject.SubjectCode))
                {
                    subject.SubjectCode = $"PRN{subject.SubjectId:000}";
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(subject.SubjectName))
                {
                    subject.SubjectName = $"Subject {i + 1}";
                    hasChanges = true;
                }

                if (subject.Credit <= 0)
                {
                    subject.Credit = 3;
                    hasChanges = true;
                }
            }

            var students = await context.Students
                .OrderBy(x => x.StudentId)
                .ToListAsync();

            for (var i = 0; i < students.Count; i++)
            {
                var student = students[i];

                if (string.IsNullOrWhiteSpace(student.FullName))
                {
                    student.FullName = $"Student {i + 1:00}";
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(student.Email))
                {
                    student.Email = $"student{student.StudentId:00}@example.com";
                    hasChanges = true;
                }

                if (student.DateOfBirth == default)
                {
                    student.DateOfBirth = new DateTime(2000, 1, 1).AddDays((i + 1) * 23);
                    hasChanges = true;
                }
            }

            var defaultSemesterId = semesters.FirstOrDefault()?.SemesterId;
            var defaultSubjectId = subjects.FirstOrDefault()?.SubjectId;

            var courses = await context.Courses
                .Include(x => x.Semester)
                .Include(x => x.Subject)
                .OrderBy(x => x.CourseId)
                .ToListAsync();

            for (var i = 0; i < courses.Count; i++)
            {
                var course = courses[i];

                if (course.SemesterId <= 0 && defaultSemesterId.HasValue)
                {
                    course.SemesterId = defaultSemesterId.Value;
                    hasChanges = true;
                }

                if (course.SubjectId <= 0 && defaultSubjectId.HasValue)
                {
                    course.SubjectId = defaultSubjectId.Value;
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(course.CourseName))
                {
                    var subjectCode = course.Subject?.SubjectCode
                        ?? subjects.FirstOrDefault(x => x.SubjectId == course.SubjectId)?.SubjectCode
                        ?? $"PRN{course.SubjectId:000}";
                    var semesterName = course.Semester?.SemesterName
                        ?? semesters.FirstOrDefault(x => x.SemesterId == course.SemesterId)?.SemesterName
                        ?? $"Semester {course.SemesterId}";

                    course.CourseName = $"{subjectCode}-{semesterName}";
                    hasChanges = true;
                }
            }

            var defaultStudentId = students.FirstOrDefault()?.StudentId;
            var defaultCourseId = courses.FirstOrDefault()?.CourseId;
            var statuses = new[] { "Active", "Completed", "Dropped" };

            var enrollments = await context.Enrollments
                .OrderBy(x => x.EnrollmentId)
                .ToListAsync();

            for (var i = 0; i < enrollments.Count; i++)
            {
                var enrollment = enrollments[i];

                if (enrollment.StudentId <= 0 && defaultStudentId.HasValue)
                {
                    enrollment.StudentId = defaultStudentId.Value;
                    hasChanges = true;
                }

                if (enrollment.CourseId <= 0 && defaultCourseId.HasValue)
                {
                    enrollment.CourseId = defaultCourseId.Value;
                    hasChanges = true;
                }

                if (enrollment.EnrollDate == default)
                {
                    enrollment.EnrollDate = BaseDate.AddDays(i + 1);
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(enrollment.Status))
                {
                    enrollment.Status = statuses[i % statuses.Length];
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
