### 基于EntityFrameworkCore和Lucene.NET实现的全文检索搜索引擎
 <a href="https://gitee.com/masuit/Masuit.LuceneEFCore.SearchEngine"><img src="https://gitee.com/static/images/logo-black.svg" height="32"></a> <a href="https://github.com/ldqk/Masuit.LuceneEFCore.SearchEngine"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/95/Font_Awesome_5_brands_github.svg/54px-Font_Awesome_5_brands_github.svg.png" height="36"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/2/29/GitHub_logo_2013.svg/128px-GitHub_logo_2013.svg.png" height="28"></a>  
**仅70KB的代码量！** 基于EntityFrameworkCore和Lucene.NET实现的全文检索搜索引擎，可轻松实现高性能的全文检索，支持添加自定义词库，自定义同义词和同音词，搜索分词默认支持同音词搜索。可以轻松应用于任何基于EntityFrameworkCore的实体框架数据库。  
**`注意：该项目仅适用于单体项目，不适用于分布式应用，分布式应用请考虑使用大型的搜索引擎中间件做支撑，如：ElasticSearch`**

[官网页面](http://masuit.com/1437) | [实际应用案例体验](https://masuit.com/s?wd=会声会影+TeamViewer)

项目开发模式：日常代码积累+网络搜集

[![LICENSE](https://img.shields.io/badge/license-Anti%20996-blue.svg)](https://github.com/996icu/996.ICU/blob/master/LICENSE) [![nuget](https://img.shields.io/nuget/v/Masuit.LuceneEFCore.SearchEngine_string.svg)](https://www.nuget.org/packages/Masuit.LuceneEFCore.SearchEngine_string) [![nuget](https://img.shields.io/nuget/dt/Masuit.LuceneEFCore.SearchEngine_string.svg)](https://www.nuget.org/packages/Masuit.LuceneEFCore.SearchEngine_string) ![codeSize](https://img.shields.io/github/languages/code-size/ldqk/Masuit.LuceneEFCore.SearchEngine.svg) ![language](https://img.shields.io/github/languages/top/ldqk/Masuit.LuceneEFCore.SearchEngine.svg) 

请注意：一旦使用本开源项目以及引用了本项目或包含本项目代码的公司因为违反劳动法（包括但不限定非法裁员、超时用工、雇佣童工等）在任何法律诉讼中败诉的，项目作者有权利追讨本项目的使用费，或者直接不允许使用任何包含本项目的源代码！

⭐⭐⭐喜欢这个项目的话就Star、Fork、Follow素质三连关♂注一下吧⭐⭐⭐

## Stargazers over time  
<img src="https://starchart.cc/ldqk/Masuit.LuceneEFCore.SearchEngine.svg">    

### 项目特点  
1. 基于原生Lucene实现，轻量高效，毫秒级响应  
2. 与EFCore无缝接入，配置代码少，可轻松接入现有项目  
3. 支持添加自定义词库，支持同义词和同音词检索，支持添加自定义同义词和同音词  
4. 不支持分布式应用，若你能解决分布式场景中索引库的同步问题，可以选择  

### 为什么没有集成到Masuit.Tools这个库？
因为这个项目又引入了几个Lucene相关的库，如果集成到[Masuit.Tools](https://github.com/ldqk/Masuit.Tools "Masuit.Tools")，这必将给原来的项目增加了更多的引用包，使用过程中也有可能没有使用Lucene的场景，这就造成了项目更加的臃肿，所以做了个新的项目。
### 为什么有这个库？现成的ElasticSearch不好么？
ES确实很好用，但我想的是还有很多的小站没必要上那么重量级的中间件，于是原生lucene库不失为一种好的选择，然而原生LuceneAPI的学习成本也相对较高，所以专门封装了这个库。
### 快速开始
#### EntityFrameworkCore基架搭建
新建项目，并安装EntityFrameworkCore相关库以及全文检索包：

根据你的项目情况，选择对应的后缀版本，提供了4个主键版本的库，后缀为int的代表主键是基于int自增类型的，后缀为Guid的代表主键是基于Guid类型的...
```shell
PM> Install-Package Masuit.LuceneEFCore.SearchEngine_int
PM> Install-Package Masuit.LuceneEFCore.SearchEngine_long
PM> Install-Package Masuit.LuceneEFCore.SearchEngine_string
PM> Install-Package Masuit.LuceneEFCore.SearchEngine_Guid
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
#### 为什么实体类要继承LuceneIndexableBaseEntity？
LuceneIndexableBaseEntity源代码如下：
```csharp
/// <summary>
/// 需要被索引的实体基类
/// </summary>
public abstract class LuceneIndexableBaseEntity : ILuceneIndexable
{
    /// <summary>
    /// 主键id
    /// </summary>
    [LuceneIndex(Name = "Id", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED), Key]
    public int Id { get; set; }

    /// <summary>
    /// 索引唯一id
    /// </summary>
    [LuceneIndex(Name = "IndexId", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED)]
    [NotMapped]
    public string IndexId
    {
        get => GetType().Name + ":" + Id;
        set
        {
        }
    }

    /// <summary>
    /// 转换成Lucene文档
    /// </summary>
    /// <returns></returns>
    public virtual Document ToDocument()
    {
        // 将实体对象转换成Lucene文档的逻辑
    }
}
```
实体继承自LuceneIndexableBaseEntity后，方便封装的Lucene可以直接调用ToDocument方法进行存储，同时，主键Id和IndexId需要参与Lucene索引文档的唯一标识(但IndexId不会生成到数据库)。
#### 搜索引擎配置、创建索引、导入自定义词库等
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

public void Configure(IApplicationBuilder app, IHostingEnvironment env, ISearchEngine searchEngine, LuceneIndexerOptions luceneIndexerOptions)
{
    // ...
    // 导入自定义词库
    KeywordsManager.AddWords("男娼起义");
    KeywordsManager.AddWords("阳物运动");
    KeywordsManager.AddWords("弟大勿勃");
        
    // 导入自定义同义词
    KeywordsManager.AddSynonyms("地大物博","弟大勿勃");
    KeywordsManager.AddSynonyms("难上加难","男上夹男");
    KeywordsManager.AddSynonyms("小心地滑","小心弟滑");
        
    // 初始化索引库，建议结合定时任务使用，定期刷新索引库
    string lucenePath = Path.Combine(env.ContentRootPath, luceneIndexerOptions.Path);
    if (!Directory.Exists(lucenePath) || Directory.GetFiles(lucenePath).Length < 1)
    {
        // 创建索引
        Console.WriteLine("索引库不存在，开始自动创建Lucene索引库...");
        searchEngine.CreateIndex(new List<string>()
        {
           nameof(DataContext.Post),
        });
        var list = searchEngine.Context.Post.Where(i => i.Status != Status.Pended).ToList(); // 删除不需要被索引的数据
        searchEngine.LuceneIndexer.Delete(list);
        Console.WriteLine("索引库创建完成！");
    }    
    // ...
}

```
**同义词支持正向和反向查找，如配置了：`KeywordsManager.AddSynonyms("地大物博","弟大勿勃")`和`KeywordsManager.AddSynonyms("弟大勿勃","地大物博")`是等效的，只需要其中一条即可**  
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
### 推荐项目
.NET万能框架：[Masuit.Tools](https://github.com/ldqk/Masuit.Tools "Masuit.Tools")

开源博客系统：[Masuit.MyBlogs](https://github.com/ldqk/Masuit.MyBlogs "Masuit.MyBlogs")
