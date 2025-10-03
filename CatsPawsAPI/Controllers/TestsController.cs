using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Контроллер для работы с тестами.
/// </summary>
/// <remarks>
/// Доступ к тестам разделён по ролям:
/// - Студент: просмотр тестов и отправка ответов
/// - Преподаватель: просмотр, создание, редактирование и удаление тестов
/// - Администратор: полный доступ
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class TestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Конструктор контроллера TestsController.
    /// </summary>
    /// <param name="context">Контекст базы данных ApplicationDbContext</param>
    public TestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех тестов с информацией о теме.
    /// </summary>
    /// <returns>Список тестов типа <see cref="Test"/></returns>
    /// <remarks>
    /// Доступно всем авторизованным пользователям: Студент, Преподаватель, Администратор.
    /// </remarks>
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Test>>> GetTests()
    {
        return await _context.Tests.Include(t => t.Topic).ToListAsync();
    }

    /// <summary>
    /// Получить конкретный тест по идентификатору с вопросами и вариантами ответов.
    /// </summary>
    /// <param name="id">Идентификатор теста</param>
    /// <returns>Тест типа <see cref="Test"/> или код 404, если тест не найден</returns>
    /// <remarks>
    /// Доступно всем авторизованным пользователям: Студент, Преподаватель, Администратор.
    /// </remarks>
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

    /// <summary>
    /// Создать новый тест.
    /// </summary>
    /// <param name="test">Объект теста для создания</param>
    /// <returns>Созданный тест с кодом 201 и ссылкой на ресурс</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPost]
    public async Task<ActionResult<Test>> CreateTest(Test test)
    {
        _context.Tests.Add(test);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTest), new { id = test.TestId }, test);
    }

    /// <summary>
    /// Обновить существующий тест.
    /// </summary>
    /// <param name="id">Идентификатор теста для обновления</param>
    /// <param name="test">Объект теста с обновлёнными данными</param>
    /// <returns>Код 204 при успешном обновлении, 400 при несоответствии id, 404 если тест не найден</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
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

    /// <summary>
    /// Удалить тест по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор теста для удаления</param>
    /// <returns>Код 204 при успешном удалении или 404, если тест не найден</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
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

    /// <summary>
    /// Отправка ответов на тест студентом.
    /// </summary>
    /// <param name="model">Модель с идентификатором теста, пользователя и выбранными ответами</param>
    /// <returns>Объект с оценкой, количеством вопросов и правильных ответов</returns>
    /// <remarks>
    /// Доступно только студентам.
    /// Подсчёт баллов: процент правильных ответов.
    /// </remarks>
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

/// <summary>
/// Модель для отправки ответов на тест студентом.
/// </summary>
public class SubmitTestModel
{
    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Идентификатор теста
    /// </summary>
    public int TestId { get; set; }

    /// <summary>
    /// Список ответов на вопросы теста
    /// </summary>
    public List<SubmitAnswerModel> Answers { get; set; }
}

/// <summary>
/// Модель одного ответа студента на вопрос теста.
/// </summary>
public class SubmitAnswerModel
{
    /// <summary>
    /// Идентификатор вопроса
    /// </summary>
    public int QuestionId { get; set; }

    /// <summary>
    /// Идентификатор выбранного варианта ответа
    /// </summary>
    public int SelectedOptionId { get; set; }
}
