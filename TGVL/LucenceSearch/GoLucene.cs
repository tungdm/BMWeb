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
    public static class GoLucene
    {
        // properties
        public static string _luceneDir =
            Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "lucene_index");
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

        // search methods
        public static ICollection<ProductSearchResult> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<ProductSearchResult>();

            // set up lucene searcher
            var searcher = new IndexSearcher(_directory, false);
            var reader = IndexReader.Open(_directory, false);
            var docs = new List<Document>();
            var term = reader.TermDocs();
            // v 2.9.4: use 'hit.Doc()'
            // v 3.0.3: use 'hit.Doc'
            while (term.Next()) docs.Add(searcher.Doc(term.Doc));
            reader.Dispose();
            searcher.Dispose();
            return _mapLuceneToDataList(docs);
        }

        public static LuceneResult Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return new LuceneResult { SearchResult = new List<ProductSearchResult>() };
            }

            var terms = input.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }

        public static LuceneResult SearchDefault(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return new LuceneResult { SearchResult = new List<ProductSearchResult>() };
            } else
            {
                return _search(input, fieldName);
            }
            //return string.IsNullOrEmpty(input) ? new List<ProductSearchResult>() : _search(input, fieldName);
           
        }

        // main search method
        private static LuceneResult _search(string searchQuery, string searchField = "")
        {
            // validation
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
            {

                return new LuceneResult { SearchResult = new List<ProductSearchResult>() };
                    
            }

            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory, false))
            {
                var hits_limit = 10;

                //var analyzer = new StandardAnalyzer(Version.LUCENE_30);

                var analyzer = new CustomAnalyzer(new StandardAnalyzer(Version.LUCENE_30));

                // search by single field
                if (!string.IsNullOrEmpty(searchField))
                {
                    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, hits_limit).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    //return results;
                    return new LuceneResult { SearchResult = results };
                }
                // search by multiple fields (ordered by RELEVANCE)
                else
                {
                    MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { "Name", "Description", "ManufactureName" }, analyzer);
                    parser.DefaultOperator = QueryParser.Operator.AND;

                    //var parser = new QueryParser(Version.LUCENE_30, "Name", analyzer);
                    //parser.DefaultOperator = QueryParser.Operator.AND;

                    var query = parseQuery(searchQuery, parser);

                   
                    var hits = searcher.Search(query, null, hits_limit).ScoreDocs;
                
                    var results = _mapLuceneToDataList(hits, searcher);
                    if (results.Count() == 0)
                    {
                        var suggestWords = FindSuggestions(searchQuery);

                        return new LuceneResult { SearchResult = results, SuggestWords = suggestWords };
                    }

                    analyzer.Close();
                    searcher.Dispose();
                    //return results;
                    return new LuceneResult { SearchResult = results };
                }
            }
        }

        public static string[] FindSuggestions(string searchQuery)
        {
            
            var spellChecker = new SpellChecker.Net.Search.Spell.SpellChecker(_directory);

            var similarWords = spellChecker.SuggestSimilar(searchQuery, 3);
            string[] substring = searchQuery.Split(null);

            return similarWords;
        }
        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        // map Lucene search index to data
        private static ICollection<ProductSearchResult> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }
        private static ICollection<ProductSearchResult> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            // v 2.9.4: use 'hit.doc'
            // v 3.0.3: use 'hit.Doc'
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }
        private static ProductSearchResult _mapLuceneDocumentToData(Document doc)
        {
            return new ProductSearchResult
            {
                Id = Convert.ToInt32(doc.Get("Id")),
                Name = doc.Get("Name"),
                Description = doc.Get("Description"),
                UnitPrice = Convert.ToDecimal(doc.Get("UnitPrice")),
                UnitType = doc.Get("UnitType"),
                ManufactureName = doc.Get("ManufactureName"),
                Image = doc.Get("Image")
            };
        }

        // add/update/clear search index data 
        public static void AddUpdateLuceneIndex(ProductSearchResult data)
        {
            AddUpdateLuceneIndex(new List<ProductSearchResult> { data });
        }
        public static void AddUpdateLuceneIndex(ICollection<ProductSearchResult> datas)
        {
            // init lucene
            //var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var analyzer = new CustomAnalyzer(new StandardAnalyzer(Version.LUCENE_30));

            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entries if any)
                foreach (var data in datas) _addToLuceneIndex(data, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
            createSpellChekerIndex();
        }

        public static void createSpellChekerIndex()
        {
            var indexReader = IndexReader.Open(_directory, true);
            
            var spellChecker = new SpellChecker.Net.Search.Spell.SpellChecker(_directory);
            
            spellChecker.IndexDictionary(new LuceneDictionary(indexReader, "Name"));

            indexReader.Dispose();
        }

        public static void ClearLuceneIndexRecord(int record_id)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
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
        public static bool ClearLuceneIndex()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }
        private static void _addToLuceneIndex(ProductSearchResult data, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("Id", data.Id.ToString()));
            writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields
            doc.Add(new Field("Id", data.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Name", data.Name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Description", data.Description, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("UnitPrice", data.UnitPrice.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("ManufactureName", data.ManufactureName, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("UnitType", data.UnitType, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Image", data.Image, Field.Store.YES, Field.Index.NOT_ANALYZED));

            // add entry to index
            writer.AddDocument(doc);
        }

    }
}