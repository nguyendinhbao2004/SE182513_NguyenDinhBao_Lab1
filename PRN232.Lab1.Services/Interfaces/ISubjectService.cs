using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<PagedResult<SubjectModel>> GetPagedAsync(QueryOptions options);
        Task<SubjectModel?> GetByIdAsync(int id, QueryOptions? options = null);
        Task<ServiceResult<SubjectModel>> CreateAsync(SubjectModel model);
        Task<ServiceResult<SubjectModel>> UpdateAsync(int id, SubjectModel model);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
