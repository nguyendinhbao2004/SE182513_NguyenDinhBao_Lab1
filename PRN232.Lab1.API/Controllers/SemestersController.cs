using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.Models;
using PRN232.Lab1.API.Models.Requests;
using PRN232.Lab1.API.Models.Responses;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers
{
    [ApiController]
    [Route("api/semesters")]
    public class SemestersController : ApiControllerBase
    {
        private readonly ISemesterService _semesterService;

        public SemestersController(ISemesterService semesterService)
        {
            _semesterService = semesterService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetSemesters([FromQuery] QueryParameters query)
        {
            var result = await _semesterService.GetPagedAsync(query.ToOptions());
            var responses = result.Items.Select(x => x.ToResponse()).ToList();
            var data = FieldSelector.Apply(responses, query.Fields);

            return Ok(Success(data, pagination: Pagination(result)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Response<SemesterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<SemesterResponse>>> GetSemesterById(int id)
        {
            var semester = await _semesterService.GetByIdAsync(id);
            if (semester == null)
            {
                return NotFound(Failure("Semester not found"));
            }

            return Ok(Success(semester.ToResponse()));
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<SemesterResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<SemesterResponse>>> CreateSemester([FromBody] SemesterRequest request)
        {
            var result = await _semesterService.CreateAsync(new SemesterModel
            {
                SemesterName = request.SemesterName,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            });

            if (!result.Success || result.Data == null)
            {
                return ServiceResponse(result, x => x.ToResponse());
            }

            var response = result.Data.ToResponse();
            return CreatedAtAction(
                nameof(GetSemesterById),
                new { id = response.SemesterId },
                Success(response, result.Message));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Response<SemesterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<SemesterResponse>>> UpdateSemester(int id, [FromBody] SemesterRequest request)
        {
            var result = await _semesterService.UpdateAsync(id, new SemesterModel
            {
                SemesterName = request.SemesterName,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            });

            return ServiceResponse(result, x => x.ToResponse());
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> DeleteSemester(int id)
        {
            var result = await _semesterService.DeleteAsync(id);
            return DeleteResponse(result);
        }
    }
}
