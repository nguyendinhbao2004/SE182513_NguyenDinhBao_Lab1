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

            IQueryable<Course> query = _courseRepository.Query()
                .Include(x => x.Semester)
                .Include(x => x.Subject);

            if (options.HasExpand("enrollments.student"))
            {
                query = query.Include(x => x.Enrollments!)
                    .ThenInclude(x => x.Student);
            }
            else if (options.HasExpand("enrollments"))
            {
                query = query.Include(x => x.Enrollments);
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

            return await QueryHelpers.ToPagedResultAsync(query, options, entity => MapCourse(entity, options));
        }

        public async Task<CourseModel?> GetByIdAsync(int id, QueryOptions? options = null)
        {
            options ??= new QueryOptions();

            IQueryable<Course> query = _courseRepository.Query()
                .Include(x => x.Semester)
                .Include(x => x.Subject);

            if (options.HasExpand("enrollments.student"))
            {
                query = query.Include(x => x.Enrollments!)
                    .ThenInclude(x => x.Student)
                    .AsQueryable();
            }
            else if (options.HasExpand("enrollments"))
            {
                query = query.Include(x => x.Enrollments);
            }

            var entity = await query.FirstOrDefaultAsync(x => x.CourseId == id);

            return entity == null ? null : MapCourse(entity, options);
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

        private static CourseModel MapCourse(Course entity, QueryOptions options)
        {
            var expandEnrollments = options.HasExpand("enrollments") || options.HasExpand("enrollments.student");
            var expandEnrollmentStudent = options.HasExpand("enrollments.student");

            return new CourseModel
            {
                CourseId = options.HasField(nameof(CourseModel.CourseId)) ? entity.CourseId : default,
                CourseName = options.HasField(nameof(CourseModel.CourseName)) ? entity.CourseName : null,
                SemesterId = options.HasField(nameof(CourseModel.SemesterId)) ? entity.SemesterId : default,
                SemesterName = options.HasField(nameof(CourseModel.SemesterName)) ? entity.Semester?.SemesterName : null,
                Semester = options.HasField(nameof(CourseModel.Semester)) && options.HasExpand("semester") && entity.Semester != null ? new SemesterModel
                {
                    SemesterId = entity.Semester.SemesterId,
                    SemesterName = entity.Semester.SemesterName,
                    StartDate = entity.Semester.StartDate,
                    EndDate = entity.Semester.EndDate
                } : null,
                SubjectId = options.HasField(nameof(CourseModel.SubjectId)) ? entity.SubjectId : default,
                SubjectCode = options.HasField(nameof(CourseModel.SubjectCode)) ? entity.Subject?.SubjectCode : null,
                SubjectName = options.HasField(nameof(CourseModel.SubjectName)) ? entity.Subject?.SubjectName : null,
                Subject = options.HasField(nameof(CourseModel.Subject)) && options.HasExpand("subject") && entity.Subject != null ? new SubjectModel
                {
                    SubjectId = entity.Subject.SubjectId,
                    SubjectCode = entity.Subject.SubjectCode,
                    SubjectName = entity.Subject.SubjectName,
                    Credit = entity.Subject.Credit
                } : null,
                Enrollments = options.HasField(nameof(CourseModel.Enrollments)) && expandEnrollments ? entity.Enrollments?.Select(enrollment => new EnrollmentModel
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    StudentId = enrollment.StudentId,
                    StudentName = enrollment.Student?.FullName,
                    Student = expandEnrollmentStudent && enrollment.Student != null ? new StudentModel
                    {
                        StudentId = enrollment.Student.StudentId,
                        FullName = enrollment.Student.FullName,
                        Email = enrollment.Student.Email,
                        DateOfBirth = enrollment.Student.DateOfBirth
                    } : null,
                    CourseId = enrollment.CourseId,
                    CourseName = entity.CourseName,
                    EnrollDate = enrollment.EnrollDate,
                    Status = enrollment.Status
                }).ToList() : null
            };
        }
    }
}
