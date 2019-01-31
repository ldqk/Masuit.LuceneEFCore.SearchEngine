### 基于EntityFrameworkCore和Lucene.NET实现的全文检索搜索引擎
基于EntityFrameworkCore和Lucene.NET实现的全文检索搜索引擎，可轻松实现高性能的全文检索。可以轻松应用于任何基于EntityFrameworkCore的实体框架数据库。

⭐⭐⭐喜欢这个项目的话就点个star关♂注一下吧⭐⭐⭐

### 快速开始
#### EntityFrameworkCore基架搭建
新建项目，并安装EntityFrameworkCore相关库以及全文检索包：

根据你的项目情况，选择对应的后缀版本，提供了4个主键版本的库，后缀为int的代表主键是基于int自增类型的，后缀为Guid的代表主键是基于Guid自增类型的...
```shell
Install-Package Masuit.LuceneEFCore.SearchEngine_int
Install-Package Masuit.LuceneEFCore.SearchEngine_long
Install-Package Masuit.LuceneEFCore.SearchEngine_string
Install-Package Masuit.LuceneEFCore.SearchEngine_Guid
```
按照套路我们需要首先搭建好EntityFrameworkCore的基架，即数据库上下文和实体对象；

准备数据库上下文对象：
```csharp
public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options){}
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }
    public virtual DbSet<Post> Post { get; set; }
}
```
准备实体对象，这里开始需要注意了，要想这个库的数据被全文检索，需要符合两个条件：
1.实体必须继承自LuceneIndexableBaseEntity；
2.需要被检索的字段需要被LuceneIndexAttribute所标记。
```csharp
/// <summary>
/// 文章
/// </summary>
[Table("Post")]
public class Post : LuceneIndexableBaseEntity
{
    public Post()
    {
        PostDate = DateTime.Now;
    }

    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "文章标题不能为空！"), LuceneIndex]
    public string Title { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [Required, MaxLength(24, ErrorMessage = "作者名最长支持24个字符！"), LuceneIndex]
    public string Author { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required(ErrorMessage = "文章内容不能为空！"), LuceneIndex(IsHtml = true)]
    public string Content { get; set; }

    /// <summary>
    /// 发表时间
    /// </summary>
    public DateTime PostDate { get; set; }

    /// <summary>
    /// 作者邮箱
    /// </summary>
    [Required(ErrorMessage = "作者邮箱不能为空！"), LuceneIndex]
    public string Email { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [StringLength(256, ErrorMessage = "标签最大允许255个字符"), LuceneIndex]
    public string Label { get; set; }

    /// <summary>
    /// 文章关键词
    /// </summary>
    [StringLength(256, ErrorMessage = "文章关键词最大允许255个字符"), LuceneIndex]
    public string Keyword { get; set; }

}
```
LuceneIndexAttribute对应的4个自定义参数：
1.Name：自定义索引字段名，默认为空；
2.Index：索引行为，默认为Field.Index.ANALYZED；
3.Store：是否被存储到索引库，默认为Field.Store.YES；
4.IsHtml：是否是html，默认为false，若标记为true，则在索引解析时会先清空其中的html标签。
#### 搜索引擎配置
Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddDbContext<DataContext>(db =>
    {
        db.UseSqlServer("Data Source=.;Initial Catalog=MyBlogs;Integrated Security=True");
    });// 配置数据库上下文
    services.AddSearchEngine<DataContext>(new LuceneIndexerOptions()
    {
        Path = "lucene"
    });// 依赖注入搜索引擎，并配置索引库路径
    // ...
}
```
HomeController.cs
```csharp
[Route("[controller]/[action]")]
public class HomeController : Controller
{
    private readonly ISearchEngine<DataContext> _searchEngine;
    private readonly ILuceneIndexer _luceneIndexer;
    public HomeController(ISearchEngine<DataContext> searchEngine, ILuceneIndexer luceneIndexer)
    {
        _searchEngine = searchEngine;
        _luceneIndexer = luceneIndexer;
    }

    /// <summary>
    /// 搜索
    /// </summary>
    /// <param name="s">关键词</param>
    /// <param name="page">第几页</param>
    /// <param name="size">页大小</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index(string s, int page, int size)
    {
        //var result = _searchEngine.ScoredSearch<Post>(new SearchOptions(s, page, size, "Title,Content,Email,Author"));
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
```
#### 关于更新索引
要在执行任何CRUD操作后更新索引，只需从ISearchEngine调用SaveChanges()方法，而不是从DataContext调用SaveChanges()。 这才会更新索引，然后会自动调用DataContexts的SaveChanges()方法。如果直接调用DataContexts的SaveChanges()方法，只会保存到数据库，而不会更新索引库。
#### 关于搜索结果
搜索返回IScoredSearchResultCollection<T>，其中包括执行搜索所花费的时间，命中总数以及每个包含的对象的结果集以及在搜索中匹配度的数量。

<font color=#f00>特别注意：单元测试中使用内存RAM目录进行索引和搜索，但这仅用于测试目的，真实生产环境应使用物理磁盘的目录。</font>

#### 演示项目
[点击这里](/WebSearchDemo "demo")
