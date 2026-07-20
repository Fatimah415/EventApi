using EventApi.Models;
using EventApi.Repositories;

namespace EventApi.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly IFileService _fileService;
    private readonly ILogger<EventService> _logger;

    public EventService(
        IEventRepository repository,
        IFileService fileService,
        ILogger<EventService> logger)
    {
        _repository  = repository;
        _fileService = fileService;
        _logger      = logger;
    }

    // -------------------------------------------------------------------------
    // GetAllAsync — unchanged
    // -------------------------------------------------------------------------
    public async Task<IEnumerable<EventResponseDto>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();
        return events.Select(MapToDto);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync — unchanged
    // -------------------------------------------------------------------------
    public async Task<EventResponseDto?> GetByIdAsync(int id)
    {
        var ev = await _repository.GetByIdAsync(id);
        return ev is null ? null : MapToDto(ev);
    }

    // -------------------------------------------------------------------------
    // CreateAsync — extended with optional image upload
    // -------------------------------------------------------------------------
    public async Task<EventResponseDto?> CreateAsync(CreateEventDto dto)
    {
        // Existing validation: owner must exist.
        if (!await _repository.UserExistsAsync(dto.UserId))
            return null;

        // Map DTO → entity (existing behaviour preserved).
        var ev = new Event
        {
            Title       = dto.Title,
            Description = dto.Description,
            EventDate   = dto.EventDate,
            Location    = dto.Location,
            CategoryId  = dto.CategoryId,
            UserId      = dto.UserId,
            CreatedAt   = DateTime.UtcNow
        };

        // If an image was included in the request, save it before the DB write.
        // ArgumentException (invalid file) bubbles → GlobalExceptionHandler → 400.
        if (dto.Image is not null)
            ev.ImagePath = await _fileService.SaveImageAsync(dto.Image);

        // Persist to the database.
        // If AddAsync fails and we already saved an image, clean up the orphan
        // before re-throwing so the file system stays consistent.
        try
        {
            await _repository.AddAsync(ev);
        }
        catch
        {
            if (ev.ImagePath is not null)
                await _fileService.DeleteImageAsync(ev.ImagePath);

            throw; // re-throw original exception → GlobalExceptionHandler → 500
        }

        return MapToDto(ev);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync — extended with optional image replacement
    // -------------------------------------------------------------------------
    public async Task<bool> UpdateAsync(int id, UpdateEventDto dto)
    {
        // GetByIdAsync uses AsNoTracking() so we re-attach via Update() later.
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return false;

        // Capture the old image path before we overwrite it in memory.
        var oldImagePath = existing.ImagePath;
        string? newImagePath = null;

        // If a new image is provided, save it first.
        // If file validation fails, ArgumentException bubbles → 400.
        // The DB record is untouched at this point, so everything stays consistent.
        if (dto.Image is not null)
        {
            newImagePath = await _fileService.SaveImageAsync(dto.Image);
            existing.ImagePath = newImagePath;
        }
        // If no new image, existing.ImagePath is preserved (no change).

        // Map all other updatable fields (owner and CreatedAt are intentionally unchanged).
        existing.Title       = dto.Title;
        existing.Description = dto.Description;
        existing.EventDate   = dto.EventDate;
        existing.Location    = dto.Location;
        existing.CategoryId  = dto.CategoryId;

        // Persist the update.
        try
        {
            await _repository.UpdateAsync(existing);
        }
        catch
        {
            // DB update failed. If we saved a new image, it is now an orphan.
            // Delete it to restore consistency, then re-throw.
            if (newImagePath is not null)
                await _fileService.DeleteImageAsync(newImagePath);

            throw; // → GlobalExceptionHandler → 500
        }

        // DB update succeeded. Remove the old image from disk if it was replaced.
        // If this cleanup fails we log a warning and return success — the DB is
        // correct and returning 500 here would lie to the client about an
        // operation that actually completed successfully.
        if (newImagePath is not null && oldImagePath is not null)
        {
            try
            {
                await _fileService.DeleteImageAsync(oldImagePath);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Event {EventId} was updated successfully but the old image " +
                    "could not be deleted from disk: {OldImagePath}. " +
                    "The file is an orphan and can be removed manually.",
                    id, oldImagePath);
            }
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // DeleteAsync — extended with image cleanup after successful DB deletion
    // -------------------------------------------------------------------------
    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return false;

        // Capture the image path before deletion so we can clean up the file.
        var imagePath = existing.ImagePath;

        // Delete the DB record first. If this fails, the image is NOT deleted —
        // the record still exists so everything remains consistent.
        await _repository.DeleteAsync(existing);

        // DB deletion succeeded. Now remove the image from disk.
        // If file deletion fails, the DB record is already gone and cannot be
        // restored, so returning 500 would mislead the client. Log the orphan
        // as a warning and return success.
        if (imagePath is not null)
        {
            try
            {
                await _fileService.DeleteImageAsync(imagePath);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Event {EventId} was deleted successfully but its image " +
                    "could not be removed from disk: {ImagePath}. " +
                    "The file is an orphan and can be removed manually.",
                    id, imagePath);
            }
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // MapToDto — extended to include the new ImagePath field
    // -------------------------------------------------------------------------
    private static EventResponseDto MapToDto(Event ev) => new()
    {
        Id           = ev.Id,
        Title        = ev.Title,
        Description  = ev.Description,
        EventDate    = ev.EventDate,
        Location     = ev.Location,
        CategoryId   = ev.CategoryId,
        CategoryName = ev.Category?.Name,
        UserId       = ev.UserId,
        CreatedAt    = ev.CreatedAt,
        ImagePath    = ev.ImagePath      // ← new field
    };
}
