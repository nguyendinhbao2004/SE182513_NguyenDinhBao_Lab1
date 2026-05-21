using PRN232.Lab1.API.Models.Responses;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Models
{
    public static class MappingExtensions
    {
        public static StudentResponse ToResponse(this StudentModel model)
        {
            return new StudentResponse
            {
                StudentId = model.StudentId,
                FullName = model.FullName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                Enrollments = model.Enrollments?.Select(x => x.ToSummaryResponse()).ToList() ?? []
            };
        }

        public static SemesterResponse ToResponse(this SemesterModel model)
        {
            return new SemesterResponse
            {
                SemesterId = model.SemesterId,
                SemesterName = model.SemesterName,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Courses = model.Courses?.Select(x => x.ToSummaryResponse()).ToList() ?? []
            };
        }

        public static SubjectResponse ToResponse(this SubjectModel model)
        {
            return new SubjectResponse
            {
                SubjectId = model.SubjectId,
                SubjectCode = model.SubjectCode,
                SubjectName = model.SubjectName,
                Credit = model.Credit,
                Courses = model.Courses?.Select(x => x.ToSummaryResponse()).ToList() ?? []
            };
        }

        public static CourseResponse ToResponse(this CourseModel model)
        {
            return new CourseResponse
            {
                CourseId = model.CourseId,
                CourseName = model.CourseName,
                SemesterId = model.SemesterId,
                SemesterName = model.SemesterName,
                Semester = model.Semester?.ToSummaryResponse(),
                SubjectId = model.SubjectId,
                SubjectCode = model.SubjectCode,
                SubjectName = model.SubjectName,
                Subject = model.Subject?.ToSummaryResponse(),
                Enrollments = model.Enrollments?.Select(x => x.ToSummaryResponse()).ToList() ?? []
            };
        }

        public static EnrollmentResponse ToResponse(this EnrollmentModel model)
        {
            return new EnrollmentResponse
            {
                EnrollmentId = model.EnrollmentId,
                StudentId = model.StudentId,
                StudentName = model.StudentName,
                Student = model.Student?.ToSummaryResponse(),
                CourseId = model.CourseId,
                CourseName = model.CourseName,
                Course = model.Course?.ToSummaryResponse(),
                EnrollDate = model.EnrollDate,
                Status = model.Status
            };
        }

        public static StudentSummaryResponse ToSummaryResponse(this StudentModel model)
        {
            return new StudentSummaryResponse
            {
                StudentId = model.StudentId,
                FullName = model.FullName,
                Email = model.Email
            };
        }

        public static SemesterSummaryResponse ToSummaryResponse(this SemesterModel model)
        {
            return new SemesterSummaryResponse
            {
                SemesterId = model.SemesterId,
                SemesterName = model.SemesterName,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
        }

        public static SubjectSummaryResponse ToSummaryResponse(this SubjectModel model)
        {
            return new SubjectSummaryResponse
            {
                SubjectId = model.SubjectId,
                SubjectCode = model.SubjectCode,
                SubjectName = model.SubjectName,
                Credit = model.Credit
            };
        }

        public static CourseSummaryResponse ToSummaryResponse(this CourseModel model)
        {
            return new CourseSummaryResponse
            {
                CourseId = model.CourseId,
                CourseName = model.CourseName,
                SemesterId = model.SemesterId,
                SemesterName = model.SemesterName,
                SubjectId = model.SubjectId,
                SubjectCode = model.SubjectCode,
                SubjectName = model.SubjectName
            };
        }

        public static EnrollmentSummaryResponse ToSummaryResponse(this EnrollmentModel model)
        {
            return new EnrollmentSummaryResponse
            {
                EnrollmentId = model.EnrollmentId,
                StudentId = model.StudentId,
                StudentName = model.StudentName ?? model.Student?.FullName,
                CourseId = model.CourseId,
                CourseName = model.CourseName ?? model.Course?.CourseName,
                EnrollDate = model.EnrollDate,
                Status = model.Status
            };
        }
    }
}
