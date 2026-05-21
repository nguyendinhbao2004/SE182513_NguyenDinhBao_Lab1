using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Services
{
    public class CourseService : ICourseService
    {
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IGenericRepository<Semester> _semesterRepository;
        private readonly IGenericRepository<Subject> _subjectRepository;

        public CourseService(
            IGenericRepository<Course> courseRepository,
            IGenericRepository<Semester> semesterRepository,
            IGenericRepository<Subject> subjectRepository)
        {
            _courseRepository = courseRepository;
            _semesterRepository = semesterRepository;
            _subjectRepository = subjectRepository;
        }

        public async Task<PagedResult<CourseModel>> GetPagedAsync(QueryOptions options)
        {
            options.Normalize();

            var query = _courseRepository.Query();

            if (options.HasExpand("semester"))
            {
                query = query.Include(x => x.Semester);
            }

            if (options.HasExpand("subject"))
            {
                query = query.Include(x => x.Subject);
            }

            if (options.HasExpand("enrollments.student"))
            {
                query = query.Include(x => x.Enrollments!)
                    .ThenInclude(x => x.Student);
            }
            else if (options.HasExpand("enrollments"))
            {
                query = query.Include(x => x.Enrollments);
            }

            if (options.CourseId.HasValue)
            {
                query = query.Where(x => x.CourseId == options.CourseId.Value);
            }

            if (options.SemesterId.HasValue)
            {
                query = query.Where(x => x.SemesterId == options.SemesterId.Value);
            }

            if (options.SubjectId.HasValue)
            {
                query = query.Where(x => x.SubjectId == options.SubjectId.Value);
            }

            if (options.MinCredit.HasValue)
            {
                query = query.Where(x => x.Subject != null && x.Subject.Credit >= options.MinCredit.Value);
            }

            if (options.MaxCredit.HasValue)
            {
                query = query.Where(x => x.Subject != null && x.Subject.Credit <= options.MaxCredit.Value);
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var search = options.Search.Trim();
                query = query.Where(x =>
                    (x.CourseName != null && x.CourseName.Contains(search)) ||
                    (x.Semester != null && x.Semester.SemesterName != null && x.Semester.SemesterName.Contains(search)) ||
                    (x.Subject != null && x.Subject.SubjectCode != null && x.Subject.SubjectCode.Contains(search)) ||
                    (x.Subject != null && x.Subject.SubjectName != null && x.Subject.SubjectName.Contains(search)));
            }

            query = ApplySorting(query, options);

            return await QueryHelpers.ToPagedResultAsync(query, options, MapCourse);
        }

        public async Task<CourseModel?> GetByIdAsync(int id)
        {
            var entity = await _courseRepository.Query()
                .Include(x => x.Semester)
                .Include(x => x.Subject)
                .Include(x => x.Enrollments!)
                    .ThenInclude(x => x.Student)
                .FirstOrDefaultAsync(x => x.CourseId == id);

            return entity == null ? null : MapCourse(entity);
        }

        public async Task<ServiceResult<CourseModel>> CreateAsync(CourseModel model)
        {
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            var entity = new Course
            {
                CourseName = model.CourseName?.Trim(),
                SemesterId = model.SemesterId,
                SubjectId = model.SubjectId
            };

            await _courseRepository.AddAsync(entity);
            await _courseRepository.SaveChangesAsync();

            var created = await GetByIdAsync(entity.CourseId);
            return ServiceResult<CourseModel>.Ok(created!, "Course created successfully");
        }

        public async Task<ServiceResult<CourseModel>> UpdateAsync(int id, CourseModel model)
        {
            var entity = await _courseRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<CourseModel>.Fail("Course not found", ServiceErrorType.NotFound);
            }

            model.CourseId = id;
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            entity.CourseName = model.CourseName?.Trim();
            entity.SemesterId = model.SemesterId;
            entity.SubjectId = model.SubjectId;

            _courseRepository.Update(entity);
            await _courseRepository.SaveChangesAsync();

            var updated = await GetByIdAsync(id);
            return ServiceResult<CourseModel>.Ok(updated!, "Course updated successfully");
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var entity = await _courseRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<bool>.Fail("Course not found", ServiceErrorType.NotFound);
            }

            _courseRepository.Delete(entity);
            await _courseRepository.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true, "Course deleted successfully");
        }

        private async Task<ServiceResult<CourseModel>?> ValidateAsync(CourseModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CourseName))
            {
                return ServiceResult<CourseModel>.Fail("Course name is required", ServiceErrorType.Validation);
            }

            var semesterExists = await _semesterRepository.Query().AnyAsync(x => x.SemesterId == model.SemesterId);
            if (!semesterExists)
            {
                return ServiceResult<CourseModel>.Fail("Semester does not exist", ServiceErrorType.Validation);
            }

            var subjectExists = await _subjectRepository.Query().AnyAsync(x => x.SubjectId == model.SubjectId);
            if (!subjectExists)
            {
                return ServiceResult<CourseModel>.Fail("Subject does not exist", ServiceErrorType.Validation);
            }

            return null;
        }

        private static IQueryable<Course> ApplySorting(IQueryable<Course> query, QueryOptions options)
        {
            var ordered = false;

            foreach (var (field, descending) in options.GetSorts())
            {
                switch (field.ToLowerInvariant())
                {
                    case "id":
                    case "courseid":
                        query = QueryHelpers.ApplySort(query, x => x.CourseId, descending, ref ordered);
                        break;
                    case "coursename":
                    case "name":
                        query = QueryHelpers.ApplySort(query, x => x.CourseName, descending, ref ordered);
                        break;
                    case "semesterid":
                        query = QueryHelpers.ApplySort(query, x => x.SemesterId, descending, ref ordered);
                        break;
                    case "subjectid":
                        query = QueryHelpers.ApplySort(query, x => x.SubjectId, descending, ref ordered);
                        break;
                    case "semestername":
                        query = QueryHelpers.ApplySort(query, x => x.Semester!.SemesterName, descending, ref ordered);
                        break;
                    case "subjectname":
                        query = QueryHelpers.ApplySort(query, x => x.Subject!.SubjectName, descending, ref ordered);
                        break;
                }
            }

            return ordered ? query : query.OrderBy(x => x.CourseId);
        }

        private static CourseModel MapCourse(Course entity)
        {
            return new CourseModel
            {
                CourseId = entity.CourseId,
                CourseName = entity.CourseName,
                SemesterId = entity.SemesterId,
                SemesterName = entity.Semester?.SemesterName,
                Semester = entity.Semester == null ? null : new SemesterModel
                {
                    SemesterId = entity.Semester.SemesterId,
                    SemesterName = entity.Semester.SemesterName,
                    StartDate = entity.Semester.StartDate,
                    EndDate = entity.Semester.EndDate
                },
                SubjectId = entity.SubjectId,
                SubjectCode = entity.Subject?.SubjectCode,
                SubjectName = entity.Subject?.SubjectName,
                Subject = entity.Subject == null ? null : new SubjectModel
                {
                    SubjectId = entity.Subject.SubjectId,
                    SubjectCode = entity.Subject.SubjectCode,
                    SubjectName = entity.Subject.SubjectName,
                    Credit = entity.Subject.Credit
                },
                Enrollments = entity.Enrollments?.Select(enrollment => new EnrollmentModel
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    StudentId = enrollment.StudentId,
                    StudentName = enrollment.Student?.FullName,
                    Student = enrollment.Student == null ? null : new StudentModel
                    {
                        StudentId = enrollment.Student.StudentId,
                        FullName = enrollment.Student.FullName,
                        Email = enrollment.Student.Email,
                        DateOfBirth = enrollment.Student.DateOfBirth
                    },
                    CourseId = enrollment.CourseId,
                    CourseName = entity.CourseName,
                    EnrollDate = enrollment.EnrollDate,
                    Status = enrollment.Status
                }).ToList()
            };
        }
    }
}
