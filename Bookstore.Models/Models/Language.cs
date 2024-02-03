using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Models
{
	public class Language
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int LanguageId { get; set; }
		[Required(ErrorMessage = "Please language name")]
		[MaxLength(50)]
		[Display(Name = "Name")]
		public string LanguageName { get; set; }
		public bool IsDeleted { get; set; } = false;
	}
}
