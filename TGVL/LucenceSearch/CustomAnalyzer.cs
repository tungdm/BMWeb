using Lucene.Net.Analysis.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis;
using System.IO;
using Lucene.Net.Analysis.Shingle;

namespace TGVL.LucenceSearch
{
    public class CustomAnalyzer : Analyzer
    {
        private readonly Analyzer subAnalyzer;
        public CustomAnalyzer(Analyzer subAnalyzer)
        {
            this.subAnalyzer = subAnalyzer;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            //TokenStream result = new StandardTokenizer(matchVersion, reader);

            var result = subAnalyzer.TokenStream(fieldName, reader);

            //result = new StandardFilter(result);
            result = new ASCIIFoldingFilter(result);
            result = new ShingleFilter(result);


            return result;
        }
    }
}