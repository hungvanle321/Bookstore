using Bookstore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models.ViewModel
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> ShoppingCarts { get; set; }
        public double GetOrderTotal()
        {
            double total = 0;
            foreach (ShoppingCart cart in ShoppingCarts)
                total += cart.Book.DiscountPrice * cart.Count;
            return total;
        }
    }
}
