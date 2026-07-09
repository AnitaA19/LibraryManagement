
using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;
using System.Text.Json;

namespace LibraryManagement.DataAccess.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly string _path;

    public BaseRepository(string path)
    {
        _path = path;

        var content = File.ReadAllText(_path);
        if (string.IsNullOrEmpty(content))
        {
            File.WriteAllText(path, "[]");
        }
    }

    protected List<TEntity> ReadEntitiesFromFile()
    {
        var content = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<List<TEntity>>(content) ??
             throw new Exception("No entities found.");
    }

    protected void WriteEntitiesToFile(List<TEntity> entities)
    {
        var content = JsonSerializer.Serialize(entities);
        File.WriteAllText(_path, content);
    }

    public void AddEntity(TEntity entity)
    {
        var entities = ReadEntitiesFromFile();
        if (entities.Count == 0)
        {
            entity.Id = 1;

        }
        else
        {
            entity.Id = entities.Max(x => x.Id) + 1;
        }

        entities.Add(entity);

        WriteEntitiesToFile(entities);
    }

    public void DeleteEntity(int id)
    {
        var entities = ReadEntitiesFromFile();

        var entityToDelete = entities.Find(x => x.Id == id) ??
            throw new Exception($"{typeof(TEntity).Name} not found.");

        entities.Remove(entityToDelete);

        WriteEntitiesToFile(entities);

    }

    public IEnumerable<TEntity> GetEntities()
    {
        return ReadEntitiesFromFile();
    }

    public TEntity GetEntity(int id)
    {
        var entities = ReadEntitiesFromFile();

        return entities.FirstOrDefault(e => e.Id == id) ??
            throw new Exception($"{typeof(TEntity).Name} not found.");

    }

    public void UpdateEntity(TEntity entity)
    {
        var entities = ReadEntitiesFromFile();
        int index = entities.FindIndex(x => x.Id == entity.Id);

        if (index == -1)
        {
            throw new Exception($"{typeof(TEntity).Name} not found.");
        }

        entities[index] = entity;

        WriteEntitiesToFile(entities);
    }
}
