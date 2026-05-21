using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Services
{
    public class StudentService : IStudentService
    {
        private readonly IGenericRepository<Student> _studentRepository;

        public StudentService(IGenericRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<PagedResult<StudentModel>> GetPagedAsync(QueryOptions options)
        {
            options.Normalize();

            IQueryable<Student> query = _studentRepository.Query();

            if (options.HasExpand("enrollments"))
            {
                query = query
                    .Include(x => x.Enrollments!)
                        .ThenInclude(x => x.Course!)
                        .ThenInclude(x => x.Semester)
                    .Include(x => x.Enrollments!)
                        .ThenInclude(x => x.Course!)
                        .ThenInclude(x => x.Subject);
            }

            if (options.StudentId.HasValue)
            {
                query = query.Where(x => x.StudentId == options.StudentId.Value);
            }

            if (options.FromDate.HasValue)
            {
                query = query.Where(x => x.DateOfBirth >= options.FromDate.Value);
            }

            if (options.ToDate.HasValue)
            {
                query = query.Where(x => x.DateOfBirth <= options.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var search = options.Search.Trim();
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.Contains(search)) ||
                    (x.Email != null && x.Email.Contains(search)));
            }

            query = ApplySorting(query, options);

            return await QueryHelpers.ToPagedResultAsync(query, options, entity => MapStudent(entity, options));
        }

        public async Task<StudentModel?> GetByIdAsync(int id, QueryOptions? options = null)
        {
            options ??= new QueryOptions();

            IQueryable<Student> query = _studentRepository.Query();

            if (options.HasExpand("enrollments"))
            {
                query = query
                    .Include(x => x.Enrollments!)
                        .ThenInclude(x => x.Course!)
                        .ThenInclude(x => x.Semester)
                    .Include(x => x.Enrollments!)
                        .ThenInclude(x => x.Course!)
                        .ThenInclude(x => x.Subject);
            }

            var entity = await query.FirstOrDefaultAsync(x => x.StudentId == id);

            return entity == null ? null : MapStudent(entity, options);
        }

        public async Task<ServiceResult<StudentModel>> CreateAsync(StudentModel model)
        {
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            var entity = new Student
            {
                FullName = model.FullName?.Trim(),
                Email = model.Email?.Trim(),
                DateOfBirth = model.DateOfBirth.Date
            };

            await _studentRepository.AddAsync(entity);
            await _studentRepository.SaveChangesAsync();

            return ServiceResult<StudentModel>.Ok(MapStudent(entity, new QueryOptions()), "Student created successfully");
        }

        public async Task<ServiceResult<StudentModel>> UpdateAsync(int id, StudentModel model)
        {
            var entity = await _studentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<StudentModel>.Fail("Student not found", ServiceErrorType.NotFound);
            }

            model.StudentId = id;
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            entity.FullName = model.FullName?.Trim();
            entity.Email = model.Email?.Trim();
            entity.DateOfBirth = model.DateOfBirth.Date;

            _studentRepository.Update(entity);
            await _studentRepository.SaveChangesAsync();

            return ServiceResult<StudentModel>.Ok(MapStudent(entity, new QueryOptions()), "Student updated successfully");
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var entity = await _studentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<bool>.Fail("Student not found", ServiceErrorType.NotFound);
            }

            _studentRepository.Delete(entity);
            await _studentRepository.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true, "Student deleted successfully");
        }

        private async Task<ServiceResult<StudentModel>?> ValidateAsync(StudentModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return ServiceResult<StudentModel>.Fail("Full name is required", ServiceErrorType.Validation);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return ServiceResult<StudentModel>.Fail("Email is required", ServiceErrorType.Validation);
            }

            if (model.DateOfBirth.Date >= DateTime.UtcNow.Date)
            {
                return ServiceResult<StudentModel>.Fail("Date of birth must be in the past", ServiceErrorType.Validation);
            }

            var normalizedEmail = model.Email.Trim();
            var emailExists = await _studentRepository.Query()
                .AnyAsync(x => x.Email == normalizedEmail && x.StudentId != model.StudentId);
            if (emailExists)
            {
                return ServiceResult<StudentModel>.Fail("Email already exists", ServiceErrorType.Conflict);
            }

            return null;
        }

        private static IQueryable<Student> ApplySorting(IQueryable<Student> query, QueryOptions options)
        {
            var ordered = false;

            foreach (var (field, descending) in options.GetSorts())
            {
                switch (field.ToLowerInvariant())
                {
                    case "id":
                    case "studentid":
                        query = QueryHelpers.ApplySort(query, x => x.StudentId, descending, ref ordered);
                        break;
                    case "fullname":
                    case "name":
                        query = QueryHelpers.ApplySort(query, x => x.FullName, descending, ref ordered);
                        break;
                    case "email":
                        query = QueryHelpers.ApplySort(query, x => x.Email, descending, ref ordered);
                        break;
                    case "dateofbirth":
                    case "dob":
                        query = QueryHelpers.ApplySort(query, x => x.DateOfBirth, descending, ref ordered);
                        break;
                }
            }

            return ordered ? query : query.OrderBy(x => x.StudentId);
        }

        private static StudentModel MapStudent(Student entity, QueryOptions options)
        {
            return new StudentModel
            {
                StudentId = entity.StudentId,
                FullName = entity.FullName,
                Email = entity.Email,
                DateOfBirth = entity.DateOfBirth,
                Enrollments = options.HasExpand("enrollments")
                    ? entity.Enrollments?.Select(enrollment => MapEnrollmentSummary(enrollment, entity)).ToList()
                    : null
            };
        }

        private static EnrollmentModel MapEnrollmentSummary(Enrollment entity, Student student)
        {
            return new EnrollmentModel
            {
                EnrollmentId = entity.EnrollmentId,
                StudentId = entity.StudentId,
                StudentName = student.FullName,
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
                    SubjectName = entity.Course.Subject?.SubjectName
                },
                EnrollDate = entity.EnrollDate,
                Status = entity.Status
            };
        }
    }
}
