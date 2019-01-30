using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebSearchDemo.Database;

namespace WebSearchDemo.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly ISearchEngine<DataContext> _searchEngine;

        public HomeController(ISearchEngine<DataContext> searchEngine)
        {
            _searchEngine = searchEngine;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string s, int page, int size)
        {
            //var result = _searchEngine.ScoredSearch<Post>(new SearchOptions(s, page, size, "Title,Content,Email,Author"));
            var result = _searchEngine.ScoredSearch<Post>(new SearchOptions(s, page, size, typeof(Post)));
            return Ok(result);
        }
    }
}