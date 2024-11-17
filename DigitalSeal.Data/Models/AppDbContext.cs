using DigitalSeal.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalSeal.Data.Models
{
    public class AppDbContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, 
        IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        private readonly IDataSeeder _dataSeeder;
        public AppDbContext(DbContextOptions<AppDbContext> options, IDataSeeder dataSeeder) : base(options)
            => _dataSeeder = dataSeeder;

        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentParty> DocumentParties => Set<DocumentParty>();
        public DbSet<FileContent> FileContents => Set<FileContent>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<RSAKey> RSAKeys => Set<RSAKey>();
        public DbSet<SignatureInfo> SignatureInfos => Set<SignatureInfo>();
        public DbSet<X509Certificate> X509Certificates => Set<X509Certificate>();
        public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
        public DbSet<PlainTextNotification> PlainTextNotifications => Set<PlainTextNotification>();
        public DbSet<InviteNotification> InviteNotifications => Set<InviteNotification>();
        public DbSet<Party> Parties => Set<Party>();
        public DbSet<DocumentNote> DocumentNotes => Set<DocumentNote>();


        public event Func<Task> SavingChangedAsync;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (SavingChangedAsync != null)
            {
                Delegate[] handlers = SavingChangedAsync.GetInvocationList();
                Task[] tasks = new Task[handlers.Length];
                for (int i = 0; i < handlers.Length; i++)
                {
                    tasks[i] = ((Func<Task>)handlers[i])();
                }
                
                await Task.WhenAll(tasks);
            }
            return await base.SaveChangesAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureEntities(modelBuilder);
            _dataSeeder.Seed(modelBuilder);
        }

        private static void ConfigureEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileContent>()
                .HasKey(e => e.DocId);

            modelBuilder.Entity<Document>()
                .HasOne(e => e.FileContent)
                .WithOne(e => e.Document)
                .HasForeignKey<FileContent>(e => e.DocId);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(e => e.Role)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.RoleId);

            modelBuilder.Entity<UserNotification>()
                .HasOne(uNotif => uNotif.Sender)
                .WithMany(user => user.SentNotifications)
                .HasForeignKey(uNotif => uNotif.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserNotification>()
                .HasOne(uNotif => uNotif.Receiver)
                .WithMany(user => user.ReceivedNotifications)
                .HasForeignKey(uNotif => uNotif.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<PendingOrganizationInvitation>()
            //    .HasOne(invite => invite.InviteSender)
            //    .WithMany(user => user.SentInvites)
            //    .HasForeignKey(invite => invite.InviteSenderId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<PendingOrganizationInvitation>()
            //    .HasOne(invite => invite.InvitedUser)
            //    .WithMany(user => user.ReceivedInvites)
            //    .HasForeignKey(invite => invite.InvitedUserId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
