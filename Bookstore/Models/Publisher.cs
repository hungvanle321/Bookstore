using System.ComponentModel.DataAnnotations;

namespace Bookstore.Models
{
	public class Publisher
	{
		[Key]
		public int PublisherId { get; set; }
		[Required(ErrorMessage = "Please enter publisher name")]
		[MaxLength(100)]
		public string PublisherName { get; set;}
	}
}
