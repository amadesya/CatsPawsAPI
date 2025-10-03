using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TasksController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ---------- GET: api/Tasks ----------
    // Доступно всем авторизованным пользователям (Студенты, Преподаватели, Администраторы)
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
    {
        return await _context.Tasks.Include(t => t.Topic).ToListAsync();
    }

    // ---------- GET: api/Tasks/5 ----------
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

    // ---------- POST: api/Tasks ----------
    // Только преподаватели и администраторы могут создавать задания
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPost]
    public async Task<ActionResult<Task>> CreateTask(Task task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.TaskId }, task);
    }

    // ---------- PUT: api/Tasks/5 ----------
    // Только преподаватели и администраторы могут редактировать задание
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

    // ---------- DELETE: api/Tasks/5 ----------
    // Только преподаватели и администраторы могут удалять задания
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

    // ---------- PUT: api/Tasks/5/status ----------
    // Только студенты могут обновлять статус выполнения своего задания
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
