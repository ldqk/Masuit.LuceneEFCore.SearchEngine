using JiebaNet.Segmenter;
using Lucene.Net.Analysis.JieBa;
using Lucene.Net.Store;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
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
                //db.UseInMemoryDatabase();
                db.UseSqlServer("Data Source=.;Initial Catalog=MyBlogs;Integrated Security=True");
            });
            AddSearchEngine(services, new LuceneIndexerOptions()
            {
                Path = "lucene",
                UseRamDirectory = false,
                MaximumFieldLength = 10000
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

        public static void AddSearchEngine(IServiceCollection services, LuceneIndexerOptions option)
        {
            services.AddSingleton(s => option);
            services.AddMemoryCache();
            services.AddTransient<Directory>(s => FSDirectory.Open(option.Path));
            services.AddTransient(s => new JieBaAnalyzer(TokenizerMode.Search));
            services.AddTransient<ILuceneIndexer, LuceneIndexer>();
            services.AddTransient<ILuceneIndexSearcher, LuceneIndexSearcher>();
            services.AddTransient<ISearchEngine<DataContext>, SearchEngine<DataContext>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataContext db, ISearchEngine<DataContext> searchEngine)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //searchEngine.CreateIndex(new List<string>()
            //{
            //    nameof(Post)
            //});
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "懒得勤快的博客");
            }); //配置swagger
            app.UseMvcWithDefaultRoute();
        }
    }
}