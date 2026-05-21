using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly IGenericRepository<Semester> _semesterRepository;

        public SemesterService(IGenericRepository<Semester> semesterRepository)
        {
            _semesterRepository = semesterRepository;
        }

        public async Task<PagedResult<SemesterModel>> GetPagedAsync(QueryOptions options)
        {
            options.Normalize();

            IQueryable<Semester> query = _semesterRepository.Query();

            if (options.HasExpand("courses"))
            {
                query = query.Include(x => x.Courses!)
                    .ThenInclude(x => x.Subject);
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var search = options.Search.Trim();
                query = query.Where(x => x.SemesterName != null && x.SemesterName.Contains(search));
            }

            query = ApplySorting(query, options);

            return await QueryHelpers.ToPagedResultAsync(query, options, entity => MapSemester(entity, options));
        }

        public async Task<SemesterModel?> GetByIdAsync(int id, QueryOptions? options = null)
        {
            options ??= new QueryOptions();

            IQueryable<Semester> query = _semesterRepository.Query();

            if (options.HasExpand("courses"))
            {
                query = query.Include(x => x.Courses!)
                    .ThenInclude(x => x.Subject);
            }

            var entity = await query.FirstOrDefaultAsync(x => x.SemesterId == id);

            return entity == null ? null : MapSemester(entity, options);
        }

        public async Task<ServiceResult<SemesterModel>> CreateAsync(SemesterModel model)
        {
            var validation = Validate(model);
            if (validation != null)
            {
                return validation;
            }

            var entity = new Semester
            {
                SemesterName = model.SemesterName?.Trim(),
                StartDate = model.StartDate.Date,
                EndDate = model.EndDate.Date
            };

            await _semesterRepository.AddAsync(entity);
            await _semesterRepository.SaveChangesAsync();

            return ServiceResult<SemesterModel>.Ok(MapSemester(entity, new QueryOptions()), "Semester created successfully");
        }

        public async Task<ServiceResult<SemesterModel>> UpdateAsync(int id, SemesterModel model)
        {
            var entity = await _semesterRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<SemesterModel>.Fail("Semester not found", ServiceErrorType.NotFound);
            }

            var validation = Validate(model);
            if (validation != null)
            {
                return validation;
            }

            entity.SemesterName = model.SemesterName?.Trim();
            entity.StartDate = model.StartDate.Date;
            entity.EndDate = model.EndDate.Date;

            _semesterRepository.Update(entity);
            await _semesterRepository.SaveChangesAsync();

            return ServiceResult<SemesterModel>.Ok(MapSemester(entity, new QueryOptions()), "Semester updated successfully");
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var entity = await _semesterRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return ServiceResult<bool>.Fail("Semester not found", ServiceErrorType.NotFound);
            }

            _semesterRepository.Delete(entity);
            await _semesterRepository.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true, "Semester deleted successfully");
        }

        private static ServiceResult<SemesterModel>? Validate(SemesterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SemesterName))
            {
                return ServiceResult<SemesterModel>.Fail("Semester name is required", ServiceErrorType.Validation);
            }

            if (model.EndDate.Date < model.StartDate.Date)
            {
                return ServiceResult<SemesterModel>.Fail("End date must be greater than or equal to start date", ServiceErrorType.Validation);
            }

            return null;
        }

        private static IQueryable<Semester> ApplySorting(IQueryable<Semester> query, QueryOptions options)
        {
            var ordered = false;

            foreach (var (field, descending) in options.GetSorts())
            {
                switch (field.ToLowerInvariant())
                {
                    case "id":
                    case "semesterid":
                        query = QueryHelpers.ApplySort(query, x => x.SemesterId, descending, ref ordered);
                        break;
                    case "semestername":
                    case "name":
                        query = QueryHelpers.ApplySort(query, x => x.SemesterName, descending, ref ordered);
                        break;
                    case "startdate":
                        query = QueryHelpers.ApplySort(query, x => x.StartDate, descending, ref ordered);
                        break;
                    case "enddate":
                        query = QueryHelpers.ApplySort(query, x => x.EndDate, descending, ref ordered);
                        break;
                }
            }

            return ordered ? query : query.OrderBy(x => x.SemesterId);
        }

        private static SemesterModel MapSemester(Semester entity, QueryOptions options)
        {
            return new SemesterModel
            {
                SemesterId = options.HasField(nameof(SemesterModel.SemesterId)) ? entity.SemesterId : default,
                SemesterName = options.HasField(nameof(SemesterModel.SemesterName)) ? entity.SemesterName : null,
                StartDate = options.HasField(nameof(SemesterModel.StartDate)) ? entity.StartDate : default,
                EndDate = options.HasField(nameof(SemesterModel.EndDate)) ? entity.EndDate : default,
                Courses = options.HasField(nameof(SemesterModel.Courses)) && options.HasExpand("courses") ? entity.Courses?.Select(course => new CourseModel
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    SemesterId = course.SemesterId,
                    SemesterName = entity.SemesterName,
                    SubjectId = course.SubjectId,
                    SubjectCode = course.Subject?.SubjectCode,
                    SubjectName = course.Subject?.SubjectName
                }).ToList() : null
            };
        }
    }
}
