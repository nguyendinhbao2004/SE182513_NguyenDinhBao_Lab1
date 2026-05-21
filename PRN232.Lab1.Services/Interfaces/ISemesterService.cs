using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces
{
    public interface ISemesterService
    {
        Task<PagedResult<SemesterModel>> GetPagedAsync(QueryOptions options);
        Task<SemesterModel?> GetByIdAsync(int id);
        Task<ServiceResult<SemesterModel>> CreateAsync(SemesterModel model);
        Task<ServiceResult<SemesterModel>> UpdateAsync(int id, SemesterModel model);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
