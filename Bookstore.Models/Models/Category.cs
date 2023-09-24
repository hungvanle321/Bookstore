using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Models
{
	public class Category
	{
		[Key]
		[MaxLength(36)]
		public string CategoryId { get; set; }
		[Required]
		public string CategoryName { get; set;}
	}

	public class Book_Category
	{
		[MaxLength(36)]
		public string CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		[ValidateNever]
		public Category Category { get; set; }
		[MaxLength(36)]
		public string BookId { get; set; }
		[ForeignKey("BookId")]
		[ValidateNever]
		public Book Book { get; set; }
	}
}
