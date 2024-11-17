using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Services
{
    /// <summary>
    /// Class used for database data seeding.
    /// </summary>
    public interface IDataSeeder
    {
        /// <summary>
        /// Seeds data into the database.
        /// </summary>
        /// <param name="modelBuilder">Object that is used for data seeding.</param>
        void Seed(ModelBuilder modelBuilder);
    }
}
