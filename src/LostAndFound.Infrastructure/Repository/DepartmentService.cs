namespace LostAndFound.Infrastructure.Repository;

public class DepartmentService 
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
        => await _unitOfWork.Departments.GetAllAsync();

    public async Task<Department?> GetByIdAsync(int id)
        => await _unitOfWork.Departments.FindAsync(id);

    public async Task AddAsync(Department department)
    {
        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveAsync();
    }
    public async Task DeleteAsync(int id) 
    {
        var department = await _unitOfWork.Departments.FindAsync(id);
        if (department is null) return;
        _unitOfWork.Departments.Remove(department);
        await _unitOfWork.SaveAsync();
    }
}
