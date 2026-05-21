using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private static readonly string[] ValidStatuses = { "Active", "Completed", "Dropped" };

        private readonly IGenericRepository<Enrollment> _enrollmentRepository;
        private readonly IGenericRepository<Student> _studentRepository;
        private readonly IGenericRepository<Course> _courseRepository;

        public EnrollmentService(
            IGenericRepository<Enrollment> enrollmentRepository,
            IGenericRepository<Student> studentRepository,
            IGenericRepository<Course> courseRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
        }

        public async Task<PagedResult<EnrollmentModel>> GetPagedAsync(QueryOptions options)
        {
            options.Normalize();

            var query = _enrollmentRepository.Query();

            if (options.HasExpand("student"))
            {
                query = query.Include(x => x.Student);
            }

            if (options.HasExpand("course.semester"))
            {
                query = query.Include(x => x.Course!)
                    .ThenInclude(x => x.Semester);
            }

            if (options.HasExpand("course.subject"))
            {
                query = query.Include(x => x.Course!)
                    .ThenInclude(x => x.Subject);
            }

            if (options.HasExpand("course"))
            {
                query = query.Include(x => x.Course);
            }

            if (options.StudentId.HasValue)
            {
                query = query.Where(x => x.StudentId == options.StudentId.Value);
            }

            if (options.CourseId.HasValue)
            {
                query = query.Where(x => x.CourseId == options.CourseId.Value);
            }

            if (options.SemesterId.HasValue)
            {
                query = query.Where(x => x.Course != null && x.Course.SemesterId == options.SemesterId.Value);
            }

            if (options.SubjectId.HasValue)
            {
                query = query.Where(x => x.Course != null && x.Course.SubjectId == options.SubjectId.Value);
            }

            if (!string.IsNullOrWhiteSpace(options.Status))
            {
                query = query.Where(x => x.Status == options.Status.Trim());
            }

            if (options.FromDate.HasValue)
            {
                query = query.Where(x => x.EnrollDate >= options.FromDate.Value);
            }

            if (options.ToDate.HasValue)
            {
                query = query.Where(x => x.EnrollDate <= options.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var search = options.Search.Trim();
                query = query.Where(x =>
                    (x.Status != null && x.Status.Contains(search)) ||
                    (x.Student != null && x.Student.FullName != null && x.Student.FullName.Contains(search)) ||
                    (x.Course != null && x.Course.CourseName != null && x.Course.CourseName.Contains(search)));
            }

            query = ApplySorting(query, options);

            return await QueryHelpers.ToPagedResultAsync(query, options, MapEnrollment);
        }

        public async Task<EnrollmentModel?> GetByIdAsync(int id)
        {
            var entity = await _enrollmentRepository.Query()
                .Include(x => x.Student)
                .Include(x => x.Course!)
                    .ThenInclude(x => x.Semester)
                .Include(x => x.Course!)
                    .ThenInclude(x => x.Subject)
                .FirstOrDefaultAsync(x => x.EnrollmentId == id);

            return entity == null ? null : MapEnrollment(entity);
        }

        public async Task<ServiceResult<EnrollmentModel>> CreateAsync(EnrollmentModel model)
        {
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            var entity = new Enrollment
            {
                StudentId = model.StudentId,
                CourseId = model.CourseId,
                EnrollDate = model.EnrollDate == default ? DateTime.UtcNow : model.EnrollDate,
                Status = NormalizeStatus(model.Status)
            };

            await _enrollmentRepository.AddAsync(entity);
            await _enrollmentRepository.SaveChangesAsync();

            var created = await GetByIdAsync(entity.EnrollmentId);
            return ServiceResult<EnrollmentModel>.Ok(created!, "Enrollment created successfully");
        }

        public async Task<ServiceResult<EnrollmentModel>> UpdateAsync(int id, EnrollmentModel model)
        {
            var entity = await _enrollmentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<EnrollmentModel>.Fail("Enrollment not found", ServiceErrorType.NotFound);
            }

            model.EnrollmentId = id;
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            entity.StudentId = model.StudentId;
            entity.CourseId = model.CourseId;
            entity.EnrollDate = model.EnrollDate == default ? entity.EnrollDate : model.EnrollDate;
            entity.Status = NormalizeStatus(model.Status);

            _enrollmentRepository.Update(entity);
            await _enrollmentRepository.SaveChangesAsync();

            var updated = await GetByIdAsync(id);
            return ServiceResult<EnrollmentModel>.Ok(updated!, "Enrollment updated successfully");
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var entity = await _enrollmentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<bool>.Fail("Enrollment not found", ServiceErrorType.NotFound);
            }

            _enrollmentRepository.Delete(entity);
            await _enrollmentRepository.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true, "Enrollment deleted successfully");
        }

        private async Task<ServiceResult<EnrollmentModel>?> ValidateAsync(EnrollmentModel model)
        {
            var studentExists = await _studentRepository.Query().AnyAsync(x => x.StudentId == model.StudentId);
            if (!studentExists)
            {
                return ServiceResult<EnrollmentModel>.Fail("Student does not exist", ServiceErrorType.Validation);
            }

            var courseExists = await _courseRepository.Query().AnyAsync(x => x.CourseId == model.CourseId);
            if (!courseExists)
            {
                return ServiceResult<EnrollmentModel>.Fail("Course does not exist", ServiceErrorType.Validation);
            }

            if (!string.IsNullOrWhiteSpace(model.Status) &&
                !ValidStatuses.Any(x => x.Equals(model.Status, StringComparison.OrdinalIgnoreCase)))
            {
                return ServiceResult<EnrollmentModel>.Fail("Status must be Active, Completed, or Dropped", ServiceErrorType.Validation);
            }

            return null;
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return "Active";
            }

            return ValidStatuses.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        private static IQueryable<Enrollment> ApplySorting(IQueryable<Enrollment> query, QueryOptions options)
        {
            var ordered = false;

            foreach (var (field, descending) in options.GetSorts())
            {
                switch (field.ToLowerInvariant())
                {
                    case "id":
                    case "enrollmentid":
                        query = QueryHelpers.ApplySort(query, x => x.EnrollmentId, descending, ref ordered);
                        break;
                    case "studentid":
                        query = QueryHelpers.ApplySort(query, x => x.StudentId, descending, ref ordered);
                        break;
                    case "courseid":
                        query = QueryHelpers.ApplySort(query, x => x.CourseId, descending, ref ordered);
                        break;
                    case "enrolldate":
                    case "date":
                        query = QueryHelpers.ApplySort(query, x => x.EnrollDate, descending, ref ordered);
                        break;
                    case "status":
                        query = QueryHelpers.ApplySort(query, x => x.Status, descending, ref ordered);
                        break;
                }
            }

            return ordered ? query : query.OrderBy(x => x.EnrollmentId);
        }

        private static EnrollmentModel MapEnrollment(Enrollment entity)
        {
            return new EnrollmentModel
            {
                EnrollmentId = entity.EnrollmentId,
                StudentId = entity.StudentId,
                StudentName = entity.Student?.FullName,
                Student = entity.Student == null ? null : new StudentModel
                {
                    StudentId = entity.Student.StudentId,
                    FullName = entity.Student.FullName,
                    Email = entity.Student.Email,
                    DateOfBirth = entity.Student.DateOfBirth
                },
                CourseId = entity.CourseId,
                CourseName = entity.Course?.CourseName,
                Course = entity.Course == null ? null : new CourseModel
                {
                    CourseId = entity.Course.CourseId,
                    CourseName = entity.Course.CourseName,
                    SemesterId = entity.Course.SemesterId,
                    SemesterName = entity.Course.Semester?.SemesterName,
                    SubjectId = entity.Course.SubjectId,
                    SubjectCode = entity.Course.Subject?.SubjectCode,
                    SubjectName = entity.Course.Subject?.SubjectName,
                    Semester = entity.Course.Semester == null ? null : new SemesterModel
                    {
                        SemesterId = entity.Course.Semester.SemesterId,
                        SemesterName = entity.Course.Semester.SemesterName,
                        StartDate = entity.Course.Semester.StartDate,
                        EndDate = entity.Course.Semester.EndDate
                    },
                    Subject = entity.Course.Subject == null ? null : new SubjectModel
                    {
                        SubjectId = entity.Course.Subject.SubjectId,
                        SubjectCode = entity.Course.Subject.SubjectCode,
                        SubjectName = entity.Course.Subject.SubjectName,
                        Credit = entity.Course.Subject.Credit
                    }
                },
                EnrollDate = entity.EnrollDate,
                Status = entity.Status
            };
        }
    }
}
