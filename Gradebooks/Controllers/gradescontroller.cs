using Gradebooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Gradebook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]   // every grade endpoint needs a valid token
    public class GradesController : ControllerBase
    {
        private static readonly List<Grade> _grades = new()
    {
        new Grade { Id = 1, StudentUsername = "juan",  Subject = "IPT",        Score = 92 },
        new Grade { Id = 2, StudentUsername = "juan",  Subject = "Networking", Score = 85 },
        new Grade { Id = 3, StudentUsername = "maria", Subject = "IPT",        Score = 95 }
    };
        private static int _nextId = 4;
        // ANY logged-in user: only THEIR OWN grades (ownership from the token)
        [HttpGet("me")]
        public IActionResult MyGrades()
        {
            var me = User.Identity!.Name;                       // from the JWT, not the URL
            return Ok(_grades.Where(g => g.StudentUsername == me));
        }
        // INSTRUCTOR only: everyone's grades
        [Authorize(Roles = "Instructor")]
        [HttpGet]
        public IActionResult AllGrades() => Ok(_grades);
        // INSTRUCTOR only: one student's grades
        [Authorize(Roles = "Instructor")]
        [HttpGet("{username}")]
        public IActionResult StudentGrades(string username)
            => Ok(_grades.Where(g => g.StudentUsername == username));
        public record GradeInput(string StudentUsername, string Subject, int Score);
        // INSTRUCTOR only: post a grade
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public IActionResult Post(GradeInput input)
        {
            var g = new Grade
            {
                Id = _nextId++,
                StudentUsername = input.StudentUsername,
                Subject = input.Subject,
                Score = input.Score
            };
            _grades.Add(g);
            return CreatedAtAction(nameof(StudentGrades), new { username = g.StudentUsername }, g);
        }
    }
}