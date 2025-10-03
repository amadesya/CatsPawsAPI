using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Контроллер для работы с заданиями.
/// </summary>
/// <remarks>
/// Доступ к заданиям разделён по ролям:
/// - Студент: просмотр заданий и обновление статуса выполнения
/// - Преподаватель: просмотр, создание, редактирование и удаление заданий
/// - Администратор: полный доступ
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Конструктор контроллера TasksController.
    /// </summary>
    /// <param name="context">Контекст базы данных ApplicationDbContext</param>
    public TasksController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех заданий с информацией о теме.
    /// </summary>
    /// <returns>Список заданий типа <see cref="Task"/></returns>
    /// <remarks>
    /// Доступно всем авторизованным пользователям: Студент, Преподаватель, Администратор.
    /// </remarks>
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
    {
        return await _context.Tasks.Include(t => t.Topic).ToListAsync();
    }

    /// <summary>
    /// Получить конкретное задание по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задания</param>
    /// <returns>Задание типа <see cref="Task"/> или код 404, если задание не найдено</returns>
    /// <remarks>
    /// Доступно всем авторизованным пользователям: Студент, Преподаватель, Администратор.
    /// </remarks>
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Task>> GetTask(int id)
    {
        var task = await _context.Tasks.Include(t => t.Topic)
                                       .FirstOrDefaultAsync(t => t.TaskId == id);
        if (task == null)
            return NotFound();

        return task;
    }

    /// <summary>
    /// Создать новое задание.
    /// </summary>
    /// <param name="task">Объект задания для создания</param>
    /// <returns>Созданное задание с кодом 201 и ссылкой на ресурс</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPost]
    public async Task<ActionResult<Task>> CreateTask(Task task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.TaskId }, task);
    }

    /// <summary>
    /// Обновить существующее задание.
    /// </summary>
    /// <param name="id">Идентификатор задания для обновления</param>
    /// <param name="task">Объект задания с обновлёнными данными</param>
    /// <returns>Код 204 при успешном обновлении, 400 при несоответствии id, 404 если задание не найдено</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, Task task)
    {
        if (id != task.TaskId)
            return BadRequest();

        _context.Entry(task).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Tasks.Any(t => t.TaskId == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить задание по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задания для удаления</param>
    /// <returns>Код 204 при успешном удалении или 404, если задание не найдено</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Обновить статус выполнения задания конкретным студентом.
    /// </summary>
    /// <param name="id">Идентификатор задания</param>
    /// <param name="statusId">Идентификатор нового статуса</param>
    /// <param name="userId">Идентификатор студента, обновляющего статус</param>
    /// <returns>Объект <see cref="TaskStatusHistory"/> с новым статусом</returns>
    /// <remarks>
    /// Доступно только студентам. Создаёт запись в истории статусов задания.
    /// </remarks>
    [Authorize(Roles = "Студент")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] int statusId, [FromQuery] int userId)
    {
        // Проверяем, существует ли задание
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        // Создаём запись в истории статусов
        var taskStatus = new TaskStatusHistory
        {
            TaskId = id,
            UserId = userId,
            StatusId = statusId
        };

        _context.TaskStatusHistories.Add(taskStatus);
        await _context.SaveChangesAsync();

        return Ok(taskStatus);
    }
}
