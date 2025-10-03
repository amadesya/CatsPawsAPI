using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Контроллер для работы с лекционными материалами.
/// </summary>
/// <remarks>
/// Доступ к материалам разделён по ролям:
/// - Студент: просмотр материалов
/// - Преподаватель: просмотр, создание, редактирование и удаление материалов
/// - Администратор: полный доступ
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class MaterialsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Конструктор контроллера MaterialsController.
    /// </summary>
    /// <param name="context">Контекст базы данных ApplicationDbContext</param>
    public MaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех материалов.
    /// </summary>
    /// <returns>Список материалов типа <see cref="Material"/></returns>
    /// <remarks>
    /// Доступно всем авторизованным пользователям: Студент, Преподаватель, Администратор.
    /// </remarks>
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Material>>> GetMaterials()
    {
        return await _context.Materials.ToListAsync();
    }

    /// <summary>
    /// Получить материал по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор материала</param>
    /// <returns>Материал типа <see cref="Material"/> или код 404, если материал не найден</returns>
    /// <remarks>
    /// Доступно всем авторизованным пользователям: Студент, Преподаватель, Администратор.
    /// </remarks>
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Material>> GetMaterial(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
            return NotFound();

        return material;
    }

    /// <summary>
    /// Создать новый материал.
    /// </summary>
    /// <param name="material">Объект материала для создания</param>
    /// <returns>Созданный материал с кодом 201 и ссылкой на ресурс</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPost]
    public async Task<ActionResult<Material>> CreateMaterial(Material material)
    {
        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMaterial), new { id = material.MaterialId }, material);
    }

    /// <summary>
    /// Обновить существующий материал.
    /// </summary>
    /// <param name="id">Идентификатор материала для обновления</param>
    /// <param name="material">Объект материала с обновлёнными данными</param>
    /// <returns>Код 204 при успешном обновлении, 400 при несоответствии id, 404 если материал не найден</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(int id, Material material)
    {
        if (id != material.MaterialId)
            return BadRequest();

        _context.Entry(material).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Materials.Any(m => m.MaterialId == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить материал по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор материала для удаления</param>
    /// <returns>Код 204 при успешном удалении или 404, если материал не найден</returns>
    /// <remarks>
    /// Доступно только преподавателям и администраторам.
    /// </remarks>
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
            return NotFound();

        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
