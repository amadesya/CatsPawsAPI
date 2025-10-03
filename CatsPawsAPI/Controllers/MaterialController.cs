using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class MaterialsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ---------- GET: api/Materials ----------
    // Доступно всем авторизованным пользователям (Студенты и Преподаватели)
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Material>>> GetMaterials()
    {
        return await _context.Materials.ToListAsync();
    }

    // ---------- GET: api/Materials/5 ----------
    [Authorize(Roles = "Студент,Преподаватель,Администратор")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Material>> GetMaterial(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
            return NotFound();

        return material;
    }

    // ---------- POST: api/Materials ----------
    // Только преподаватели и администраторы могут создавать материалы
    [Authorize(Roles = "Преподаватель,Администратор")]
    [HttpPost]
    public async Task<ActionResult<Material>> CreateMaterial(Material material)
    {
        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMaterial), new { id = material.MaterialId }, material);
    }

    // ---------- PUT: api/Materials/5 ----------
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

    // ---------- DELETE: api/Materials/5 ----------
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
