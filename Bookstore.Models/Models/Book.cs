using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Models
{
	public class Book
	{
		[Key]
		[MaxLength(36)]
		public string BookId { get; set; }
		[Required(ErrorMessage = "Please enter book title")]
		[MaxLength(100)]
		public string Title { get; set; }
		[Required(ErrorMessage = "Please enter book description")]
		public string Description { get; set; }
		[Required(ErrorMessage = "Please enter book ISBN)")]
		[MaxLength(13)]
		public string ISBN { get; set; }
		[Required(ErrorMessage = "Please enter book language")]
		public int LanguageId { get; set; }
		[ForeignKey("PublisherId")]
		[ValidateNever]
		public Language Language { get; set; }

		[Required(ErrorMessage = "Please enter number of pages")]
		public int PagesCount { get; set; }
		public DateTime PublicationDate { get; set; }
		[Required(ErrorMessage = "Please enter book publisher")]
		public int PublisherId { get; set; }
		[ForeignKey("PublisherId")]
		[ValidateNever]
		public Publisher Publisher { get; set; }
		[Required(ErrorMessage = "Please enter book original price")]
		[RegularExpression("^\\d+(\\.\\d{1,2})?$", ErrorMessage = "Please enter a valid number with up to two decimal places")]
		public double OriginPrice { get; set;}
		[RegularExpression("^\\d+(\\.\\d{1,2})?$", ErrorMessage = "Please enter a valid number with up to two decimal places")]
		public double DiscountPrice { get; set; }
	}
}
