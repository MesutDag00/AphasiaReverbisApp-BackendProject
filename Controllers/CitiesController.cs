using AphaisaReverbes.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AphaisaReverbes.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CitiesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CitiesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CityItem>>> GetAll(CancellationToken ct)
    {
        var cities = await _db.Cities
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new CityItem(x.Id, x.Name))
            .ToListAsync(ct);

        return Ok(cities);
    }

    public sealed record CityItem(int Id, string Name);
}

