using DigitalSeal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Repositories
{
    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<TEntity> _entities;

        public Repository(AppDbContext context)
        {
            _context = context;
            _entities = _context.Set<TEntity>();

            //_addBuffer = new(_entities.AddRangeAsync);
            //_removeBuffer = new(_entities.RemoveRange);

            //_context.OnSaveChangesAsync += _context_OnSaveChangesAsync;
        }

        private QueryBuffer<TEntity>? _addBuffer;
        private QueryBuffer<TEntity> AddBuffer
        {
            get
            {
                if (_addBuffer == null)
                {
                    _addBuffer = new QueryBuffer<TEntity>(_entities.AddRangeAsync);
                    _context.SavingChangedAsync += OnSavingChangesAsync;
                }

                return _addBuffer;
            }
        }

        private QueryBuffer<TEntity>? _removeBuffer;
        private QueryBuffer<TEntity> RemoveBuffer
        {
            get
            {
                if (_removeBuffer == null)
                {
                    _removeBuffer = new QueryBuffer<TEntity>(_entities.RemoveRange);
                    _context.SavingChangedAsync += OnSavingChangesAsync;
                }

                return _removeBuffer;
            }
        }

        private async Task OnSavingChangesAsync()
        {
            await CommitBuffers();
        }


        protected AppDbContext Context => _context;

        public async Task AddAsync(TEntity entity)
        {
            await _entities.AddAsync(entity);
        }

        public void Add(TEntity entity)
        {
            _entities.Add(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _entities.AddRangeAsync(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Remove(TEntity entity)
        {
            _entities.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _entities.RemoveRange(entities);
        }

        public void BufferedAdd(TEntity entity)
        {
            AddBuffer.Push(entity);
        }

        public void BufferedRemove(TEntity entity)
        {
            RemoveBuffer.Push(entity);
        }

        public async Task CommitBuffers()
        {
            RemoveBuffer.Commit();
            await AddBuffer.CommitAsync();
        }
    }

    public class QueryBuffer<TEntity>
    {
        private readonly List<TEntity> _entities = [];
        private readonly Func<TEntity[], Task>? _commitAsync;
        private readonly Action<TEntity[]>? _commit;
        public QueryBuffer(Func<TEntity[], Task> commitAsync)
        {
            _commitAsync = commitAsync;
        }

        public QueryBuffer(Action<TEntity[]> commit)
        {
            _commit = commit;
        }

        public void Push(TEntity entity)
        {
            _entities.Add(entity);
        }

        public async Task CommitAsync()
        {
            if (_commitAsync != null)
            {
                await _commitAsync([.. _entities]);
            }

            _entities.Clear();
        }

        public void Commit()
        {
            _commit?.Invoke([.. _entities]);
            _entities.Clear();
        }
    }
}
