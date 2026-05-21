using PRN232.Lab1.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN232.Lab1.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<PagedResult<EnrollmentModel>> GetPagedAsync(QueryOptions options);
        Task<EnrollmentModel?> GetByIdAsync(int id);
        Task<ServiceResult<EnrollmentModel>> CreateAsync(EnrollmentModel model);
        Task<ServiceResult<EnrollmentModel>> UpdateAsync(int id, EnrollmentModel model);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
