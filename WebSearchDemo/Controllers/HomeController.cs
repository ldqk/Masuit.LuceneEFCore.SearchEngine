using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="s">关键词</param>
        /// <param name="page">第几页</param>
        /// <param name="size">页大小</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(string s, int page, int size)
        {
            var result = _searchEngine.ScoredSearch<Post>(new SearchOptions(s, page, size, typeof(Post)));
            return Ok(result);
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        [HttpGet]
        public void CreateIndex()
        {
            //_searchEngine.CreateIndex();//扫描所有数据表，创建符合条件的库的索引
            _searchEngine.CreateIndex(new List<string>() { nameof(Post) });//创建指定的数据表的索引
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        [HttpPost]
        public void AddIndex(Post p)
        {
            // 添加到数据库并更新索引
            _searchEngine.Context.Post.Add(p);
            _searchEngine.SaveChanges();

            //_luceneIndexer.Add(p); //单纯的只添加索引库
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        [HttpDelete]
        public void DeleteIndex(Post post)
        {
            //从数据库删除并更新索引库
            Post p = _searchEngine.Context.Post.Find(post.Id);
            _searchEngine.Context.Post.Remove(p);
            _searchEngine.SaveChanges();

            //_luceneIndexer.Delete(p);// 单纯的从索引库移除
        }

        /// <summary>
        /// 更新索引库
        /// </summary>
        /// <param name="post"></param>
        [HttpPatch]
        public void UpdateIndex(Post post)
        {
            //从数据库更新并同步索引库
            Post p = _searchEngine.Context.Post.Find(post.Id);

            // update...
            _searchEngine.Context.Post.Update(p);
            _searchEngine.SaveChanges();

            //_luceneIndexer.Update(p);// 单纯的更新索引库
        }
    }
}
