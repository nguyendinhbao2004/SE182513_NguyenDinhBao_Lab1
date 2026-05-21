using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.Models;
using PRN232.Lab1.API.Models.Requests;
using PRN232.Lab1.API.Models.Responses;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ApiControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetStudents([FromQuery] QueryParameters query)
        {
            var result = await _studentService.GetPagedAsync(query.ToOptions());
            var responses = result.Items.Select(x => x.ToResponse()).ToList();
            var data = FieldSelector.Apply(responses, query.Fields);

            return Ok(Success(data, pagination: Pagination(result)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Response<StudentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetStudentById(int id, [FromQuery] QueryParameters query)
        {
            var student = await _studentService.GetByIdAsync(id, query.ToOptions());
            if (student == null)
            {
                return NotFound(Failure("Student not found"));
            }

            return Ok(Success(FieldSelector.Apply(student.ToResponse(), query.Fields)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<StudentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<StudentResponse>>> CreateStudent([FromBody] StudentRequest request)
        {
            var result = await _studentService.CreateAsync(new StudentModel
            {
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth
            });

            if (!result.Success || result.Data == null)
            {
                return ServiceResponse(result, x => x.ToResponse());
            }

            var response = result.Data.ToResponse();
            return CreatedAtAction(
                nameof(GetStudentById),
                new { id = response.StudentId },
                Success(response, result.Message));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Response<StudentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<StudentResponse>>> UpdateStudent(int id, [FromBody] StudentRequest request)
        {
            var result = await _studentService.UpdateAsync(id, new StudentModel
            {
                FullName = request.FullName,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth
            });

            return ServiceResponse(result, x => x.ToResponse());
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> DeleteStudent(int id)
        {
            var result = await _studentService.DeleteAsync(id);
            return DeleteResponse(result);
        }
    }
}
