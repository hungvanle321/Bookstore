using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models
{
	public class Address
	{
		[Key]
		public int AddressID { get; set; }
		public string ApplicationUserID {  get; set; }
		[ForeignKey("ApplicationUserID")]
		public virtual ApplicationUser ApplicationUser { get; set; }
		[Required]
		public string Location { get; set; }
		public bool IsDefault { get; set; } = false;

	}
}
