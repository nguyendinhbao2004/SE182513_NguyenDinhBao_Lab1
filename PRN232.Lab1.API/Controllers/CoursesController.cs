using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.Models;
using PRN232.Lab1.API.Models.Requests;
using PRN232.Lab1.API.Models.Responses;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers
{
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ApiControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetCourses([FromQuery] QueryParameters query)
        {
            var result = await _courseService.GetPagedAsync(query.ToOptions());
            var responses = result.Items.Select(x => x.ToResponse()).ToList();
            var data = FieldSelector.Apply(responses, query.Fields);

            return Ok(Success(data, pagination: Pagination(result)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Response<CourseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetCourseById(int id, [FromQuery] QueryParameters query)
        {
            var course = await _courseService.GetByIdAsync(id, query.ToOptions());
            if (course == null)
            {
                return NotFound(Failure("Course not found"));
            }

            return Ok(Success(FieldSelector.Apply(course.ToResponse(), query.Fields)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<CourseResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<CourseResponse>>> CreateCourse([FromBody] CourseRequest request)
        {
            var result = await _courseService.CreateAsync(new CourseModel
            {
                CourseName = request.CourseName,
                SemesterId = request.SemesterId,
                SubjectId = request.SubjectId
            });

            if (!result.Success || result.Data == null)
            {
                return ServiceResponse(result, x => x.ToResponse());
            }

            var response = result.Data.ToResponse();
            return CreatedAtAction(
                nameof(GetCourseById),
                new { id = response.CourseId },
                Success(response, result.Message));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Response<CourseResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<CourseResponse>>> UpdateCourse(int id, [FromBody] CourseRequest request)
        {
            var result = await _courseService.UpdateAsync(id, new CourseModel
            {
                CourseName = request.CourseName,
                SemesterId = request.SemesterId,
                SubjectId = request.SubjectId
            });

            return ServiceResponse(result, x => x.ToResponse());
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteAsync(id);
            return DeleteResponse(result);
        }
    }
}
