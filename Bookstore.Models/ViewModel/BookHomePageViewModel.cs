using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bookstore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Models;

namespace Bookstore.Models.ViewModel
{
	public class CheckBoxOption
	{
		public bool IsChecked { get; set; } = false;
		public string Name { get; set; }
		public int Count { get; set; }
	}
	public class BookHomePageViewModel
	{
		public PaginatedList<Book> BookList { get; set; }
		public IEnumerable<CheckBoxOption> CategoryList { get; set; }
		public IEnumerable<CheckBoxOption> AuthorList { get; set; }
		public IEnumerable<CheckBoxOption> PublisherList { get; set; }

		public string? SearchString { get; set; }
		public string? SortType { get; set; }

		public double ? MaxPrice { get; set; }
		public double? MinPrice { get; set;}
		public int TotalCount { get; set; }
	}
}
