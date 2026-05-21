using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.Models;
using PRN232.Lab1.API.Models.Requests;
using PRN232.Lab1.API.Models.Responses;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ApiControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetEnrollments([FromQuery] QueryParameters query)
        {
            var result = await _enrollmentService.GetPagedAsync(query.ToOptions());
            var responses = result.Items.Select(x => x.ToResponse()).ToList();
            var data = FieldSelector.Apply(responses, query.Fields);

            return Ok(Success(data, pagination: Pagination(result)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Response<EnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetEnrollmentById(int id, [FromQuery] QueryParameters query)
        {
            var enrollment = await _enrollmentService.GetByIdAsync(id, query.ToOptions());
            if (enrollment == null)
            {
                return NotFound(Failure("Enrollment not found"));
            }

            return Ok(Success(FieldSelector.Apply(enrollment.ToResponse(), query.Fields)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<EnrollmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<EnrollmentResponse>>> CreateEnrollment([FromBody] EnrollmentRequest request)
        {
            var result = await _enrollmentService.CreateAsync(new EnrollmentModel
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                EnrollDate = request.EnrollDate ?? DateTime.UtcNow,
                Status = request.Status
            });

            if (!result.Success || result.Data == null)
            {
                return ServiceResponse(result, x => x.ToResponse());
            }

            var response = result.Data.ToResponse();
            return CreatedAtAction(
                nameof(GetEnrollmentById),
                new { id = response.EnrollmentId },
                Success(response, result.Message));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Response<EnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<EnrollmentResponse>>> UpdateEnrollment(int id, [FromBody] EnrollmentRequest request)
        {
            var result = await _enrollmentService.UpdateAsync(id, new EnrollmentModel
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                EnrollDate = request.EnrollDate ?? default,
                Status = request.Status
            });

            return ServiceResponse(result, x => x.ToResponse());
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> DeleteEnrollment(int id)
        {
            var result = await _enrollmentService.DeleteAsync(id);
            return DeleteResponse(result);
        }
    }
}
