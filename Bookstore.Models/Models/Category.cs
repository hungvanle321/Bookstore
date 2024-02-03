using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Models
{
	public class Category
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int CategoryId { get; set; }
		[Required]
		[Display(Name = "Name")]
		public string CategoryName { get; set;}
		public bool IsDeleted { get; set; } = false;
		public DateTime UpdatedAt { get; set; }
		public string UpdatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; }
	}
}
