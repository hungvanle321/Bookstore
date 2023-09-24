using Bookstore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Bookstore.Models;

namespace Bookstore.DataAccess.Data
{
	public class ApplicationDbContext: IdentityDbContext<IdentityUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
		public DbSet<Book> Books { get; set; }
		public DbSet<Language> Languages { get; set; }
		public DbSet<Publisher> Publishers { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<ApplicationUser> ApplicationUsers { get; set; }
		public DbSet<ShoppingCart> ShoppingCarts { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Language>().HasData(
					new Language { LanguageId=1, LanguageName = "English" },
					new Language { LanguageId = 2, LanguageName = "Vietnamese" }
				);
		}
	}
}
