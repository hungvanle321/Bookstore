using System.ComponentModel.DataAnnotations;

namespace Bookstore.Models
{
	public class Language
	{
		[Key]
		public int LanguageId { get; set; }
		[Required(ErrorMessage = "Please language name")]
		[MaxLength(50)]
		public string LanguageName { get; set; }
	}
}
