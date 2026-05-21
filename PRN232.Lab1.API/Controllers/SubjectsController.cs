using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.Models;
using PRN232.Lab1.API.Models.Requests;
using PRN232.Lab1.API.Models.Responses;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers
{
    [ApiController]
    [Route("api/subjects")]
    public class SubjectsController : ApiControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetSubjects([FromQuery] QueryParameters query)
        {
            var result = await _subjectService.GetPagedAsync(query.ToOptions());
            var responses = result.Items.Select(x => x.ToResponse()).ToList();
            var data = FieldSelector.Apply(responses, query.Fields);

            return Ok(Success(data, pagination: Pagination(result)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Response<SubjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> GetSubjectById(int id, [FromQuery] QueryParameters query)
        {
            var subject = await _subjectService.GetByIdAsync(id, query.ToOptions());
            if (subject == null)
            {
                return NotFound(Failure("Subject not found"));
            }

            return Ok(Success(FieldSelector.Apply(subject.ToResponse(), query.Fields)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(Response<SubjectResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<SubjectResponse>>> CreateSubject([FromBody] SubjectRequest request)
        {
            var result = await _subjectService.CreateAsync(new SubjectModel
            {
                SubjectCode = request.SubjectCode,
                SubjectName = request.SubjectName,
                Credit = request.Credit
            });

            if (!result.Success || result.Data == null)
            {
                return ServiceResponse(result, x => x.ToResponse());
            }

            var response = result.Data.ToResponse();
            return CreatedAtAction(
                nameof(GetSubjectById),
                new { id = response.SubjectId },
                Success(response, result.Message));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Response<SubjectResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<SubjectResponse>>> UpdateSubject(int id, [FromBody] SubjectRequest request)
        {
            var result = await _subjectService.UpdateAsync(id, new SubjectModel
            {
                SubjectCode = request.SubjectCode,
                SubjectName = request.SubjectName,
                Credit = request.Credit
            });

            return ServiceResponse(result, x => x.ToResponse());
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Response<object>>> DeleteSubject(int id)
        {
            var result = await _subjectService.DeleteAsync(id);
            return DeleteResponse(result);
        }
    }
}
