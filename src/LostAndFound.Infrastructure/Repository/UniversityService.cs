namespace LostAndFound.Infrastructure.Repository;

public class UniversityService
{
    private readonly IUnitOfWork _unitOfWork;
    public UniversityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<University>> GetAllAsync()
        => await _unitOfWork.Universities.GetAllAsync();

    public async Task<University?> GetByIdAsync(int id)
        => await _unitOfWork.Universities.FindAsync(id);

    public async Task AddAsync(University university)
    {
        await _unitOfWork.Universities.AddAsync(university);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateAsync(University university)
    {
        _unitOfWork.Universities.Update(university);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var university = await _unitOfWork.Universities.FindAsync(id);
        if (university is null) return;

        _unitOfWork.Universities.Remove(university);
        await _unitOfWork.SaveAsync();
    }

}
