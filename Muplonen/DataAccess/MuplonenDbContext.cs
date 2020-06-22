using Microsoft.EntityFrameworkCore;

namespace Muplonen.DataAccess
{
    /// <summary>
    /// The entityframework database context.
    /// </summary>
    public class MuplonenDbContext : DbContext
    {
        /// <summary>
        /// Player accounts.
        /// </summary>
        public DbSet<PlayerAccount>? PlayerAccounts { get; set; }

        /// <summary>
        /// Creates a new <see cref="MuplonenDbContext"/> with the specified context options.
        /// </summary>
        /// <param name="dbContextOptions">Options for the context.</param>
        public MuplonenDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {

        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var playerAccountEntity = modelBuilder.Entity<PlayerAccount>();
            playerAccountEntity.HasKey(m => m.Id);
            playerAccountEntity.HasIndex(m => m.Accountname).IsUnique();
            playerAccountEntity.Property(m => m.Accountname).HasMaxLength(16).IsRequired();
            playerAccountEntity.Property(m => m.PasswordHash).HasMaxLength(48).IsFixedLength().IsRequired();
            playerAccountEntity.Property(m => m.CreatedAt).IsRequired();
        }
    }
}
