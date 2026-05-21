using PRN232.Lab1.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN232.Lab1.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentModel?> GetByIdAsync(int id);
        Task<PagedResult<StudentModel>> GetPagedAsync(QueryOptions options);
        Task<ServiceResult<StudentModel>> CreateAsync(StudentModel model);
        Task<ServiceResult<StudentModel>> UpdateAsync(int id, StudentModel model);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
