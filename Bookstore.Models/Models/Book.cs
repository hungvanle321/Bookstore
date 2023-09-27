using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Models
{
	public class Book
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int BookId { get; set; }
		[Required(ErrorMessage = "Please enter book title")]
		[MaxLength(100)]
		public string Title { get; set; }
		[Required(ErrorMessage = "Please enter book description")]
		public string Description { get; set; }
		[Required(ErrorMessage = "Please enter book ISBN)")]
		[MaxLength(13)]
		public string ISBN { get; set; }
		[Required(ErrorMessage = "Please enter book language")]
		[Display(Name = "Language")]
		public int LanguageId { get; set; }
		[ForeignKey("LanguageId")]
		[ValidateNever]
		public Language Language { get; set; }

		[Required(ErrorMessage = "Please enter number of pages")]
		[Display(Name = "Number of pages")]
		[Range(1, int.MaxValue, ErrorMessage = "Number of pages must greater than 0")]
		[RegularExpression("^\\d*[1-9]\\d*$", ErrorMessage = "You must type a positive integer")]
		public int PagesCount { get; set; }
		[Display(Name = "Publication date")]
		public DateOnly PublicationDate { get; set; }
		[NotMapped]
		public DateTime PublicationDateUI
		{
			get => PublicationDate.ToDateTime(new TimeOnly());
			set => PublicationDate = DateOnly.FromDateTime(value);
		}
		[Required(ErrorMessage = "Please enter book publisher")]
		public string Publisher { get; set; }
		[Required(ErrorMessage = "Please enter book original price")]
		[RegularExpression("^\\d+(\\.\\d{1,2})?$", ErrorMessage = "Please enter a valid number with up to two decimal places")]
		[Display(Name = "Original Price")]
		public double OriginPrice { get; set;}
		[RegularExpression("^\\d+(\\.\\d{1,2})?$", ErrorMessage = "Please enter a valid number with up to two decimal places")]
		[Display(Name = "Discount Price")]
		public double DiscountPrice { get; set; }
		[Display(Name = "Product Image")]
		[ValidateNever]
		public string ImageUrl { get; set; }
		[Required]
		public string Author { get; set; }
		[Required(ErrorMessage = "The Category is required !")]
		[Display(Name = "Category")]
		public int CategoryID { get; set; }
		[ForeignKey("CategoryID")]

		[ValidateNever]
		public virtual Category Category { get; set; }
	}
}
