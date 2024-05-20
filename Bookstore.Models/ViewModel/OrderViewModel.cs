using Bookstore.Utility.PaymentServices;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models.ViewModel
{
	public class OrderViewModel
	{
		[Required(ErrorMessage ="Phone Number is required")]
		public string ShipPhoneNumber { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string ShipName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string ShipEmail { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string ShipAddress { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public List<ShoppingCart> ChosenBooks { get; set; } = new List<ShoppingCart>();
		public int PaymentMethod { get; set; }
		public bool IsCardLink { get; set; } = false;
		[ValidateNever]
		public string ChosenCardToken { get; set; }
		public List<TokenizationInfo> LinkedCard { get; set; } = new List<TokenizationInfo>();
		public double GetOrderTotal()
		{
			double total = 0;
			foreach (ShoppingCart cart in ChosenBooks)
				total += cart.Book.DiscountPrice * cart.Count;
			return total;
		}
	}
}
