using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.DataAccess.Interfaces;
using System.Text.Json;

namespace LibraryManagement.DataAccess.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly string _path;

    public BaseRepository(string path)
    {
        _path = path;

        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_path) || string.IsNullOrWhiteSpace(File.ReadAllText(_path)))
            {
                File.WriteAllText(_path, "[]");
            }
        }
        catch (IOException ex)
        {
            throw new DataAccessException($"Could not initialize the data file at '{_path}'.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new DataAccessException($"Access denied while initializing the data file at '{_path}'.", ex);
        }
    }

    protected List<TEntity> ReadEntitiesFromFile()
    {
        string content;
        try
        {
            content = File.ReadAllText(_path);
        }
        catch (IOException ex)
        {
            throw new DataAccessException($"Could not read {typeof(TEntity).Name} data from '{_path}'.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new DataAccessException($"Access denied while reading {typeof(TEntity).Name} data from '{_path}'.", ex);
        }

        try
        {
            return JsonSerializer.Deserialize<List<TEntity>>(content) ??
                throw new DataAccessException($"{typeof(TEntity).Name} data file '{_path}' is empty or corrupted.");
        }
        catch (JsonException ex)
        {
            try
            {
                var trimmed = content?.Trim();
                if (!string.IsNullOrEmpty(trimmed) && trimmed.StartsWith('{'))
                {
                    var single = JsonSerializer.Deserialize<TEntity>(content);
                    if (single != null)
                    {
                        return new List<TEntity> { single };
                    }
                }

                var backupPath = _path + ".corrupt." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".bak";
                try
                {
                    File.Copy(_path, backupPath, overwrite: true);
                }
                catch
                {
                }

                File.WriteAllText(_path, "[]");
                return new List<TEntity>();
            }
            catch (Exception inner)
            {
                throw new DataAccessException($"{typeof(TEntity).Name} data file '{_path}' contains invalid JSON.", inner);
            }
        }
    }

    protected void WriteEntitiesToFile(List<TEntity> entities)
    {
        try
        {
            var content = JsonSerializer.Serialize(entities);
            File.WriteAllText(_path, content);
        }
        catch (IOException ex)
        {
            throw new DataAccessException($"Could not write {typeof(TEntity).Name} data to '{_path}'.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new DataAccessException($"Access denied while writing {typeof(TEntity).Name} data to '{_path}'.", ex);
        }
    }

    public void AddEntity(TEntity entity)
    {
        var entities = ReadEntitiesFromFile();
        entity.Id = entities.Count == 0 ? 1 : entities.Max(x => x.Id) + 1;

        entities.Add(entity);

        WriteEntitiesToFile(entities);
    }

    public void DeleteEntity(int id)
    {
        var entities = ReadEntitiesFromFile();

        var entityToDelete = entities.Find(x => x.Id == id) ??
            throw new NotFoundException(typeof(TEntity).Name, id);

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
            throw new NotFoundException(typeof(TEntity).Name, id);
    }

    public void UpdateEntity(TEntity entity)
    {
        var entities = ReadEntitiesFromFile();
        int index = entities.FindIndex(x => x.Id == entity.Id);

        if (index == -1)
        {
            throw new NotFoundException(typeof(TEntity).Name, entity.Id);
        }

        entities[index] = entity;

        WriteEntitiesToFile(entities);
    }
}