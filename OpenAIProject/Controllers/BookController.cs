using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenAI_API.Models;

namespace OpenAIProject.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class BookController : ControllerBase
	{
		private readonly ILogger<BookController> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOpenAIService _openAIService;

		public BookController(ILogger<BookController> logger, IUnitOfWork unitOfWork, IOpenAIService openAIService)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
			_openAIService = openAIService;
		}

		[HttpGet("{id}")]
		public async Task<Book> GetAll(int id)
		{
			Book objBook = (await _unitOfWork.BookRepo.GetAsync(b => b.BookId == id));
			objBook.PublicationDate = DateOnly.FromDateTime(objBook.PublicationDateUI);
			return objBook;
		}

		[HttpPost("{text}")]
		public async Task<string> CompleteSentence (string text)
		{
			var result = await _openAIService.CompleteSentence(text);
			return result;
		}

	}

	public class OpenAIConfig
	{
		public string Key { get; set; } = "";
	}

	public interface IOpenAIService
	{
		Task<string> CompleteSentence(string sentence);
	}

	public class OpenAIService : IOpenAIService
	{
		private readonly OpenAIConfig _config;
		public OpenAIService(
				IOptionsMonitor<OpenAIConfig> optionsMonitor
			) {
			_config = optionsMonitor.CurrentValue;
		}

		public async Task<string> CompleteSentence(string sentence)
		{
			var api = new OpenAI_API.OpenAIAPI(_config.Key);
			var result = await api.Completions.CreateCompletionAsync(
				new OpenAI_API.Completions.CompletionRequest(sentence, model: Model.CurieText, temperature: 0.8, max_tokens: 500));

			return result.Completions[0].Text;
		}
	}
}