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
        /// Player characters.
        /// </summary>
        public DbSet<PlayerCharacter>? PlayerCharacters { get; set; }

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
            var playerAccount = modelBuilder.Entity<PlayerAccount>();
            playerAccount.HasKey(m => m.Id);
            playerAccount.HasIndex(m => m.Accountname).IsUnique();
            playerAccount.Property(m => m.Accountname).HasMaxLength(16).IsRequired();
            playerAccount.Property(m => m.PasswordHash).HasMaxLength(48).IsFixedLength().IsRequired();
            playerAccount.Property(m => m.CreatedAt).IsRequired();

            var playerCharacter = modelBuilder.Entity<PlayerCharacter>();
            playerCharacter.HasKey(m => m.Id);
            playerCharacter.HasIndex(m => m.Charactername).IsUnique();
            playerCharacter.HasOne(m => m.PlayerAccount).WithMany(m => m!.PlayerCharacters);
            playerCharacter.Property(m => m.Charactername).HasMaxLength(16).IsRequired();
            playerCharacter.Property(m => m.CreatedAt).IsRequired();
            playerCharacter.Property(m => m.PlayerAccountId).IsRequired();
        }
    }
}
