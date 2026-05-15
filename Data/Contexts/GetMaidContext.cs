using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts
{
    public class GetMaidContext : IdentityDbContext<ApplicationUser, ApplicationUserRole, string>
    {
        public GetMaidContext(DbContextOptions<GetMaidContext> options) : base(options)
        {
        }

        // DbSets for non-identity business entities
        public DbSet<Child> Children { get; set; } = null!;
        
        // Explicit DbSets for Customer and Worker if direct query access is needed
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Worker> Workers { get; set; } = null!;
        public DbSet<JobPosting> JobPostings { get; set; } = null!;
        // ... Keep your existing DbSets for Children, Customers, Workers, and JobPostings

        // ADD THESE MISSING DBSETS:
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<JobApplication> JobApplications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Essential: Configures the schema for Identity (AspNetUsers, AspNetRoles, etc.)
            base.OnModelCreating(builder);

            builder.Entity<JobApplication>()
                .Property(j => j.Status)
                .HasConversion<string>();
            
            
            // Configure the Child to Customer relationship
            builder.Entity<Child>(entity =>
            {
                entity.HasOne(c => c.Customer)
                    .WithMany(cust => cust.Children)
                    .HasForeignKey(c => c.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TPH Configuration: EF Core will use a 'Discriminator' column by default 
            // to distinguish between Customer and Worker within the AspNetUsers table.
        }
    }
}

