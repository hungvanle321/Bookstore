using Bookstore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.DataAccess.Data
{
	public class ApplicationDbContext: IdentityDbContext<IdentityUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
		public DbSet<Book> Books { get; set; }
		public DbSet<Language> Languages { get; set; }
		public DbSet<Publisher> Publishers { get; set; }
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
