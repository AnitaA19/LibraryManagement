using LibraryManagement.Core.Entities;

namespace LibraryManagement.DataAccess.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    IEnumerable<TEntity> GetEntities();
    TEntity GetEntity(int id);
    void AddEntity(TEntity entity);
    void UpdateEntity(TEntity entity);
    void DeleteEntity(int id);
}
