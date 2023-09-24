namespace Bookstore.Utility
{
	public class PaginatedList<T> : List<T>
	{
		public int PageIndex { get; private set; }
		public int TotalPages { get; private set; }
		public int StartPage { get; private set; }
		public int EndPage { get; private set; }

		public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
		{
			PageIndex = pageIndex;
			TotalPages = (int)Math.Ceiling(count / (double)pageSize);
			StartPage = PageIndex - 5;
			EndPage = PageIndex + 4;
			if (StartPage <= 0)
			{
				EndPage -= (StartPage - 1);
				StartPage = 1;
			}

			if (EndPage > TotalPages)
			{
				EndPage = TotalPages;
				if (EndPage > 10)
				{
					StartPage = EndPage - 9; 
				}
			}

			AddRange(items);
		}

		public bool HasPreviousPage => PageIndex > 1;

		public bool HasNextPage => PageIndex < TotalPages;

		public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
		{
			var count = source.Count();
			var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
			return new PaginatedList<T>(items, count, pageIndex, pageSize);
		}
	}
}
