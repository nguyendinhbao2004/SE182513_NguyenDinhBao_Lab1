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
    }
}
