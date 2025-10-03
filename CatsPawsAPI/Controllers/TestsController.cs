using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class TestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ---------- GET: api/Tests ----------
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Test>>> GetTests()
    {
        return await _context.Tests.Include(t => t.Topic).ToListAsync();
    }

    // ---------- GET: api/Tests/5 ----------
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Test>> GetTest(int id)
    {
        var test = await _context.Tests
            .Include(t => t.TestQuestions)
                .ThenInclude(q => q.TestOptions)
            .FirstOrDefaultAsync(t => t.TestId == id);

        if (test == null)
            return NotFound();

        return test;
    }

    // ---------- POST: api/Tests ----------
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPost]
    public async Task<ActionResult<Test>> CreateTest(Test test)
    {
        _context.Tests.Add(test);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTest), new { id = test.TestId }, test);
    }

    // ---------- PUT: api/Tests/5 ----------
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTest(int id, Test test)
    {
        if (id != test.TestId)
            return BadRequest();

        _context.Entry(test).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Tests.Any(t => t.TestId == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // ---------- DELETE: api/Tests/5 ----------
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTest(int id)
    {
        var test = await _context.Tests.FindAsync(id);
        if (test == null)
            return NotFound();

        _context.Tests.Remove(test);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ---------- POST: api/Tests/submit ----------
    // Студент отправляет ответы на тест
    [Authorize(Roles = "Студент")]
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitTest([FromBody] SubmitTestModel model)
    {
        var test = await _context.Tests
            .Include(t => t.TestQuestions)
                .ThenInclude(q => q.TestOptions)
            .FirstOrDefaultAsync(t => t.TestId == model.TestId);

        if (test == null)
            return NotFound("Тест не найден");

        int correctCount = 0;
        int totalQuestions = test.TestQuestions.Count;

        foreach (var answer in model.Answers)
        {
            var question = test.TestQuestions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
            if (question != null)
            {
                var correctOption = question.TestOptions.FirstOrDefault(o => o.IsCorrect);
                if (correctOption != null && answer.SelectedOptionId == correctOption.OptionId)
                {
                    correctCount++;
                }
            }
        }

        decimal score = (decimal)correctCount / totalQuestions * 100;

        // Сохраняем результат
        var result = new TestResult
        {
            UserId = model.UserId,
            TestId = model.TestId,
            Score = score
        };
        _context.TestResults.Add(result);
        await _context.SaveChangesAsync();

        return Ok(new { score, totalQuestions, correctCount });
    }
}

// ---------- Модель для отправки ответов ----------
public class SubmitTestModel
{
    public int UserId { get; set; }
    public int TestId { get; set; }
    public List<SubmitAnswerModel> Answers { get; set; }
}

public class SubmitAnswerModel
{
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
}
