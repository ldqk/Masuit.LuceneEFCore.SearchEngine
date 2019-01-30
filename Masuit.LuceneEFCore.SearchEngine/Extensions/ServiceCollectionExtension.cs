using JiebaNet.Segmenter;
using Lucene.Net.Analysis.JieBa;
using Lucene.Net.Store;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Masuit.LuceneEFCore.SearchEngine.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="option"></param>
        public static IServiceCollection AddSearchEngine<TContext>(this IServiceCollection services, LuceneIndexerOptions option) where TContext : DbContext
        {
            services.AddSingleton(s => option);
            services.AddMemoryCache();
            services.AddTransient<Directory>(s => FSDirectory.Open(option.Path));
            services.AddTransient(s => new JieBaAnalyzer(TokenizerMode.Search));
            services.AddTransient<ILuceneIndexer, LuceneIndexer>();
            services.AddTransient<ILuceneIndexSearcher, LuceneIndexSearcher>();
            services.AddTransient<ISearchEngine<TContext>, SearchEngine<TContext>>();
            return services;
        }
    }
}