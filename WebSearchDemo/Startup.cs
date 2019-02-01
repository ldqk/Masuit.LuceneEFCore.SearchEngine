using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
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
                c.SwaggerDoc("v1", new Info
                {
                    Title = "API文档",
                    Version = "v1"
                });
                c.DescribeAllEnumsAsStrings();
                c.IncludeXmlComments(AppContext.BaseDirectory + "WebSearchDemo.xml");
            }); //配置swagger
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataContext db, ISearchEngine<DataContext> searchEngine)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            db.Post.AddRange(JsonConvert.DeserializeObject<List<Post>>(File.ReadAllText(AppContext.BaseDirectory + "Posts.json")));
            searchEngine.CreateIndex(new List<string>()
            {
                nameof(Post)
            });
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "懒得勤快的博客，搜索引擎测试");
            }); //配置swagger
            app.UseMvcWithDefaultRoute();
        }
    }
}