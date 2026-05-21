using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.Models;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers
{
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected Response<T> Success<T>(T data, string message = "Request processed successfully", PaginationMetadata? pagination = null)
        {
            return new Response<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Pagination = pagination
            };
        }

        protected Response<object> Failure(string message, object? errors = null)
        {
            return new Response<object>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        protected PaginationMetadata Pagination<T>(PagedResult<T> result)
        {
            return new PaginationMetadata
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        }

        protected ActionResult<Response<TResponse>> ServiceResponse<TModel, TResponse>(
            ServiceResult<TModel> result,
            Func<TModel, TResponse> map)
        {
            if (result.Success && result.Data != null)
            {
                return Ok(Success(map(result.Data), result.Message));
            }

            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(Failure(result.Message)),
                ServiceErrorType.Conflict => BadRequest(Failure(result.Message)),
                _ => BadRequest(Failure(result.Message))
            };
        }

        protected ActionResult<Response<object>> DeleteResponse(ServiceResult<bool> result)
        {
            if (result.Success)
            {
                return Ok(Success<object?>(null, result.Message));
            }

            return result.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(Failure(result.Message)),
                ServiceErrorType.Conflict => BadRequest(Failure(result.Message)),
                _ => BadRequest(Failure(result.Message))
            };
        }
    }
}
