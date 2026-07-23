using EventApi.Data;
using EventApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Event?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event> AddAsync(Event ev, CancellationToken cancellationToken = default)
    {
        _context.Events.Add(ev);
        await _context.SaveChangesAsync(cancellationToken);
        return ev;
    }

    public async Task UpdateAsync(Event ev, CancellationToken cancellationToken = default)
    {
        _context.Events.Update(ev);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Event ev, CancellationToken cancellationToken = default)
    {
        _context.Events.Remove(ev);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UserExistsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> CategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AnyAsync(
            category => category.Id == categoryId,
            cancellationToken);
    }
}
