namespace DigitalSeal.Data.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task AddAsync(TEntity entity);
        void Add(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        Task SaveChangesAsync();

        /// <summary>
        /// Methods below are created for fun. Tried to create a middle ground between using AddRange and creating collections before each loop, and calling AddAsync at each iteration of a loop.
        /// </summary>


        //void BufferedAdd(TEntity entity);
        //void BufferedRemove(TEntity entity);
        //Task CommitBuffers();
    }
}