namespace HR.LeaveManagement.Application.Persistence.Contracts;

public interface IGenericRepository<T> where T: class
{
    Task<T> Get(int id);
    
    // the benefit of a ReadOnlyList vs a List is that its clear
    // that pulled from the db like this cant be modified
    // (less db-instuctions after pulling necessary)
    Task<IReadOnlyList<T>> GetAll();

    Task<T> Add(T entity);
    
    Task<T> Update(T entity);
    
    Task<T> Delete(T entity);
}