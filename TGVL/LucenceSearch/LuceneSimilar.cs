using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

using Version = Lucene.Net.Util.Version;
using TGVL.Models;
using Lucene.Net.Analysis;
using SpellChecker.Net.Search.Spell;

namespace TGVL.LucenceSearch
{
    public class LuceneSimilar
    {
        // properties
        public static string _luceneDir =
            Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "lucene_similar");
        private static FSDirectory _directoryTemp;
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(_luceneDir, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }

        public static void AddUpdateLuceneIndex(LuceneRequest data)
        {
            AddUpdateLuceneIndex(new List<LuceneRequest> { data });
        }

        public static void AddUpdateLuceneIndex(ICollection<LuceneRequest> datas)
        {
            // init lucene
            var analyzer = new CustomAnalyzer(new StandardAnalyzer(Version.LUCENE_30));

            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entries if any)
                foreach (var data in datas) _addToLuceneIndex(data, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        private static void _addToLuceneIndex(LuceneRequest data, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("Id", data.Id.ToString()));
            writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields
            doc.Add(new Field("Id", data.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("ListProduct", data.ListProduct, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Avatar", data.Avatar, Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("Image", data.Image, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Title", data.Title, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("CustomerName", data.CustomerName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("StartDate", data.StartDate.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("DueDate", data.DueDate.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Flag", data.Flag.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            // add entry to index
            writer.AddDocument(doc);
        }

        public static LuceneRequestResult Search(string input, int hits_limit)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new LuceneRequestResult { SimilarResult = new List<LuceneRequest>() };
            }

            var terms = input.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            input = string.Join(" ", terms);

            return _search(input, hits_limit);
        }

        public static LuceneRequestResult SearchDefault(string input, int hits_limit)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new LuceneRequestResult { SimilarResult = new List<LuceneRequest>() };
            }
            else
            {
                return _search(input, hits_limit);
            }
            //return string.IsNullOrEmpty(input) ? new List<ProductSearchResult>() : _search(input, fieldName);

        }

        // main search method
        private static LuceneRequestResult _search(string searchQuery, int hits_limit)
        {
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
            {

                return new LuceneRequestResult { SimilarResult = new List<LuceneRequest>() };

            }

            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory, false))
            {
                //var hits_limit = 3;

                //var analyzer = new StandardAnalyzer(Version.LUCENE_30);

                var analyzer = new CustomAnalyzer(new StandardAnalyzer(Version.LUCENE_30));

                // search by single field
                //if (!string.IsNullOrEmpty(searchField))
                //{
                //    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
                //    var query = parseQuery(searchQuery, parser);
                //    var hits = searcher.Search(query, hits_limit).ScoreDocs;
                //    var results = _mapLuceneToDataList(hits, searcher);
                //    analyzer.Close();
                //    searcher.Dispose();
                //    //return results;
                //    return new LuceneRequestResult { SimilarResult = results };
                //}
                //// search by multiple fields (ordered by RELEVANCE)
                //else
                //{
                //MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { "ListProduct" }, analyzer);
                var parser = new QueryParser(Version.LUCENE_30, "ListProduct", analyzer);
                //parser.DefaultOperator = QueryParser.Operator.OR;
                parser.DefaultOperator = QueryParser.Operator.AND;

                //var parser = new QueryParser(Version.LUCENE_30, "Name", analyzer);
                //parser.DefaultOperator = QueryParser.Operator.AND;






                var query = parseQuery(searchQuery, parser);


                var hits = searcher.Search(query, null, hits_limit).ScoreDocs;

                var results = _mapLuceneToDataList(hits, searcher);

                if (results.Count() == 0)
                {
                    var newparser = new QueryParser(Version.LUCENE_30, "ListProduct", analyzer); 
                    newparser.DefaultOperator = QueryParser.Operator.OR;
                    var newquery = parseQuery(searchQuery, newparser);
                    var newhits = searcher.Search(newquery, null, hits_limit).ScoreDocs;
                    var newresults = _mapLuceneToDataList(newhits, searcher);

                    

                   
                    return new LuceneRequestResult { SimilarResult = newresults };
                }
                analyzer.Close();
                    searcher.Dispose();
                    //return results;
                    return new LuceneRequestResult { SimilarResult = results };
                //}
            }
        }

        
        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;

            try
            {
                searchQuery = searchQuery.Trim();
                query = parser.Parse(searchQuery);
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        // map Lucene search index to data
        private static ICollection<LuceneRequest> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }
        private static ICollection<LuceneRequest> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            // v 2.9.4: use 'hit.doc'
            // v 3.0.3: use 'hit.Doc'
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }
        private static LuceneRequest _mapLuceneDocumentToData(Document doc)
        {
            return new LuceneRequest
            {
                Id = Convert.ToInt32(doc.Get("Id")),
                ListProduct = doc.Get("ListProduct"),
                Avatar = doc.Get("Avatar"),
                //Image = doc.Get("Image"),
                CustomerName = doc.Get("CustomerName"),
                Title = doc.Get("Title"),
                StartDate = Convert.ToDateTime(doc.Get("StartDate")),
                Flag = Convert.ToInt32(doc.Get("Flag")),
                DueDate = Convert.ToDateTime(doc.Get("DueDate"))
            };
        }

        public static void ClearLuceneIndexRecord(int record_id)
        {
            // init lucene
            var analyzer = new CustomAnalyzer(new StandardAnalyzer(Version.LUCENE_30));
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("Id", record_id.ToString()));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
    }
}