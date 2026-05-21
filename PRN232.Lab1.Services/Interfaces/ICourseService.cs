using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResult<CourseModel>> GetPagedAsync(QueryOptions options);
        Task<CourseModel?> GetByIdAsync(int id);
        Task<ServiceResult<CourseModel>> CreateAsync(CourseModel model);
        Task<ServiceResult<CourseModel>> UpdateAsync(int id, CourseModel model);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
