using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IGenericRepository<Subject> _subjectRepository;
        private readonly IGenericRepository<Course> _courseRepository;

        public SubjectService(
            IGenericRepository<Subject> subjectRepository,
            IGenericRepository<Course> courseRepository)
        {
            _subjectRepository = subjectRepository;
            _courseRepository = courseRepository;
        }

        public async Task<PagedResult<SubjectModel>> GetPagedAsync(QueryOptions options)
        {
            options.Normalize();

            IQueryable<Subject> query = _subjectRepository.Query();

            if (options.HasExpand("courses"))
            {
                query = query.Include(x => x.Courses!)
                    .ThenInclude(x => x.Semester);
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var search = options.Search.Trim();
                query = query.Where(x =>
                    (x.SubjectCode != null && x.SubjectCode.Contains(search)) ||
                    (x.SubjectName != null && x.SubjectName.Contains(search)));
            }

            query = ApplySorting(query, options);

            return await QueryHelpers.ToPagedResultAsync(query, options, entity => MapSubject(entity, options));
        }

        public async Task<SubjectModel?> GetByIdAsync(int id, QueryOptions? options = null)
        {
            options ??= new QueryOptions();

            IQueryable<Subject> query = _subjectRepository.Query();

            if (options.HasExpand("courses"))
            {
                query = query.Include(x => x.Courses!)
                    .ThenInclude(x => x.Semester);
            }

            var entity = await query.FirstOrDefaultAsync(x => x.SubjectId == id);

            return entity == null ? null : MapSubject(entity, options);
        }

        public async Task<ServiceResult<SubjectModel>> CreateAsync(SubjectModel model)
        {
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            var entity = new Subject
            {
                SubjectCode = model.SubjectCode?.Trim().ToUpperInvariant(),
                SubjectName = model.SubjectName?.Trim(),
                Credit = model.Credit
            };

            await _subjectRepository.AddAsync(entity);
            await _subjectRepository.SaveChangesAsync();

            return ServiceResult<SubjectModel>.Ok(MapSubject(entity, new QueryOptions()), "Subject created successfully");
        }

        public async Task<ServiceResult<SubjectModel>> UpdateAsync(int id, SubjectModel model)
        {
            var entity = await _subjectRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<SubjectModel>.Fail("Subject not found", ServiceErrorType.NotFound);
            }

            model.SubjectId = id;
            var validation = await ValidateAsync(model);
            if (validation != null)
            {
                return validation;
            }

            entity.SubjectCode = model.SubjectCode?.Trim().ToUpperInvariant();
            entity.SubjectName = model.SubjectName?.Trim();
            entity.Credit = model.Credit;

            _subjectRepository.Update(entity);
            await _subjectRepository.SaveChangesAsync();

            return ServiceResult<SubjectModel>.Ok(MapSubject(entity, new QueryOptions()), "Subject updated successfully");
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var entity = await _subjectRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<bool>.Fail("Subject not found", ServiceErrorType.NotFound);
            }

            var hasCourses = await _courseRepository.Query().AnyAsync(x => x.SubjectId == id);
            if (hasCourses)
            {
                return ServiceResult<bool>.Fail("Subject cannot be deleted because it has courses", ServiceErrorType.Conflict);
            }

            _subjectRepository.Delete(entity);
            await _subjectRepository.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true, "Subject deleted successfully");
        }

        private async Task<ServiceResult<SubjectModel>?> ValidateAsync(SubjectModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SubjectCode))
            {
                return ServiceResult<SubjectModel>.Fail("Subject code is required", ServiceErrorType.Validation);
            }

            if (string.IsNullOrWhiteSpace(model.SubjectName))
            {
                return ServiceResult<SubjectModel>.Fail("Subject name is required", ServiceErrorType.Validation);
            }

            if (model.Credit < 1 || model.Credit > 10)
            {
                return ServiceResult<SubjectModel>.Fail("Credit must be between 1 and 10", ServiceErrorType.Validation);
            }

            var normalizedCode = model.SubjectCode.Trim().ToUpperInvariant();
            var codeExists = await _subjectRepository.Query()
                .AnyAsync(x => x.SubjectCode == normalizedCode && x.SubjectId != model.SubjectId);
            if (codeExists)
            {
                return ServiceResult<SubjectModel>.Fail("Subject code already exists", ServiceErrorType.Conflict);
            }

            return null;
        }

        private static IQueryable<Subject> ApplySorting(IQueryable<Subject> query, QueryOptions options)
        {
            var ordered = false;

            foreach (var (field, descending) in options.GetSorts())
            {
                switch (field.ToLowerInvariant())
                {
                    case "id":
                    case "subjectid":
                        query = QueryHelpers.ApplySort(query, x => x.SubjectId, descending, ref ordered);
                        break;
                    case "subjectcode":
                    case "code":
                        query = QueryHelpers.ApplySort(query, x => x.SubjectCode, descending, ref ordered);
                        break;
                    case "subjectname":
                    case "name":
                        query = QueryHelpers.ApplySort(query, x => x.SubjectName, descending, ref ordered);
                        break;
                    case "credit":
                    case "credits":
                        query = QueryHelpers.ApplySort(query, x => x.Credit, descending, ref ordered);
                        break;
                }
            }

            return ordered ? query : query.OrderBy(x => x.SubjectId);
        }

        private static SubjectModel MapSubject(Subject entity, QueryOptions options)
        {
            return new SubjectModel
            {
                SubjectId = options.HasField(nameof(SubjectModel.SubjectId)) ? entity.SubjectId : default,
                SubjectCode = options.HasField(nameof(SubjectModel.SubjectCode)) ? entity.SubjectCode : null,
                SubjectName = options.HasField(nameof(SubjectModel.SubjectName)) ? entity.SubjectName : null,
                Credit = options.HasField(nameof(SubjectModel.Credit)) ? entity.Credit : default,
                Courses = options.HasField(nameof(SubjectModel.Courses)) && options.HasExpand("courses") ? entity.Courses?.Select(course => new CourseModel
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    SemesterId = course.SemesterId,
                    SemesterName = course.Semester?.SemesterName,
                    SubjectId = course.SubjectId,
                    SubjectCode = entity.SubjectCode,
                    SubjectName = entity.SubjectName
                }).ToList() : null
            };
        }
    }
}
