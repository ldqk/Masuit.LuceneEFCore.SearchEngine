using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.LuceneEFCore.SearchEngine
{
    public class LuceneIndexer : ILuceneIndexer
    {
        /// <summary>
        /// 索引目录
        /// </summary>
        private readonly Directory _directory;

        /// <summary>
        /// 索引分析器
        /// </summary>
        private readonly Analyzer _analyzer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="analyzer"></param>
        public LuceneIndexer(Directory directory, Analyzer analyzer)
        {
            _directory = directory;
            _analyzer = analyzer;
        }

        /// <summary>
        /// 添加到索引
        /// </summary>
        /// <param name="entity">实体</param>
        public void Add(ILuceneIndexable entity)
        {
            Update(new LuceneIndexChange(entity, LuceneIndexState.Added));
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="entities">实体集</param>
        /// <param name="recreate">是否需要覆盖</param>
        public void CreateIndex(IEnumerable<ILuceneIndexable> entities, bool recreate = true)
        {
            var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, _analyzer);

            using var writer = new IndexWriter(_directory, config);
            // 删除重建
            if (recreate)
            {
                writer.DeleteAll();
                writer.Commit();
            }

            // 遍历实体集，添加到索引库
            foreach (var entity in entities)
            {
                writer.AddDocument(entity.ToDocument());
            }
            writer.Flush(true, true);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="entity">实体</param>
        public void Delete(ILuceneIndexable entity)
        {
            Update(new LuceneIndexChange(entity, LuceneIndexState.Removed));
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="entries">实体集</param>
        public void Delete<T>(List<T> entries) where T : ILuceneIndexable
        {
            var set = new LuceneIndexChangeset
            {
                Entries = entries.Select(e => new LuceneIndexChange(e, LuceneIndexState.Removed)).ToList()
            };
            Update(set);
        }

        /// <summary>
        /// 删除所有索引
        /// </summary>
        /// <param name="commit">是否提交</param>
        public void DeleteAll(bool commit = true)
        {
            var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, _analyzer);
            using var writer = new IndexWriter(_directory, config);
            try
            {
                writer.DeleteAll();
                if (commit)
                {
                    writer.Commit();
                }
                writer.Flush(true, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="entity">实体</param>
        public void Update(ILuceneIndexable entity)
        {
            Update(new LuceneIndexChange(entity, LuceneIndexState.Updated));
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="change">实体</param>
        public void Update(LuceneIndexChange change)
        {
            var changeset = new LuceneIndexChangeset(change);
            Update(changeset);
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="changeset">实体</param>
        public void Update(LuceneIndexChangeset changeset)
        {
            var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, _analyzer);
            using var writer = new IndexWriter(_directory, config);
            foreach (var change in changeset.Entries)
            {
                switch (change.State)
                {
                    case LuceneIndexState.Added:
                        writer.AddDocument(change.Entity.ToDocument());
                        break;
                    case LuceneIndexState.Removed:
                        writer.DeleteDocuments(new Term("IndexId", change.Entity.IndexId));
                        break;
                    case LuceneIndexState.Updated:
                        writer.UpdateDocument(new Term("IndexId", change.Entity.IndexId), change.Entity.ToDocument());
                        break;
                }
            }

            writer.Flush(true, changeset.HasDeletes);
        }

        /// <summary>
        /// 索引库数量
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            try
            {
                IndexReader reader = DirectoryReader.Open(_directory);
                return reader.NumDocs;
            }
            catch (IndexNotFoundException ex)
            {
                _directory.ClearLock("write.lock");
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}