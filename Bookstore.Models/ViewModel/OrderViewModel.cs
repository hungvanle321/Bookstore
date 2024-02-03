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
		public string? jsonData { get; set; }
		public string ShipPhoneNumber { get; set; }
		public string ShipName { get; set; }
		public string ShipEmail { get; set; }
		public string ShipAddress { get; set; }
		public List<ShoppingCart> ChosenBooks { get; set; } = new List<ShoppingCart>();
		public double GetOrderTotal()
		{
			double total = 0;
			foreach (ShoppingCart cart in ChosenBooks)
				total += cart.Book.DiscountPrice * cart.Count;
			return total;
		}
	}
}
