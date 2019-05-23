using SQLite.CodeFirst;
using System.Data.Entity;
using WatchCake.Models;

namespace WatchCake.DAL
{
    /// <summary>
    /// EntityFramework DB Context fot the WathCake app.
    /// </summary>
    public class WcDbContext : DbContext
    {
        /// <summary>
        /// Default constructor with calling the connection string by the name.
        /// </summary>
        public WcDbContext() : base("WcSQLite") { }

        //Entity sets declarations.
        public DbSet<Option> Options { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Snapshot> Snapshots { get; set; }
        public DbSet<Tracker> Trackers { get; set; }
        public DbSet<TrackedPage> TrackedPages { get; set; }

        /// <summary>
        /// EF method for fine tuning code-first creation approach.
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //SQLite code-first correction, resetting initializer with the custom one so it supports automatic tables creation.
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<WcDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);

            //Make sure ParentShop property on the Page entity wont lead to creation of the Shop table in DB.
            modelBuilder.Entity<Page>().Ignore(x => x.ParentShop);
        }
    }
}
