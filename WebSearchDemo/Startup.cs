using JiebaNet.Segmenter;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WebSearchDemo.Database;

namespace WebSearchDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(db =>
            {
                db.UseInMemoryDatabase("test");

                //db.UseSqlServer("Data Source=.;Initial Catalog=MyBlogs;Integrated Security=True");
            });
            services.AddSearchEngine<DataContext>(new LuceneIndexerOptions()
            {
                Path = "lucene"
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = $"接口文档",
                    Description = $"HTTP API ",
                    Contact = new OpenApiContact { Name = "懒得勤快", Email = "admin@masuit.com", Url = new Uri("https://masuit.coom") },
                    License = new OpenApiLicense { Name = "懒得勤快", Url = new Uri("https://masuit.com") }
                });
                c.IncludeXmlComments(AppContext.BaseDirectory + "WebSearchDemo.xml");
            }); //配置swagger
            services.AddControllers();
            services.AddControllersWithViews().SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataContext db, ISearchEngine<DataContext> searchEngine)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            new JiebaSegmenter().AddWord("会声会影"); //添加自定义词库
            new JiebaSegmenter().AddWord("思杰马克丁"); //添加自定义词库
            new JiebaSegmenter().AddWord("TeamViewer"); //添加自定义词库
            db.Post.AddRange(JsonConvert.DeserializeObject<List<Post>>(File.ReadAllText(AppContext.BaseDirectory + "Posts.json")));
            db.SaveChanges();
            searchEngine.DeleteIndex();
            searchEngine.CreateIndex(new List<string>()
            {
                nameof(Post)
            });
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "懒得勤快的博客，搜索引擎测试");
            }); //配置swagger
            app.UseRouting().UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // 属性路由
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); // 默认路由
            });
        }
    }
}
