using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;


namespace BatchPrintYay.pdf
{
    public class PdfContentModifier
    {

        #region Private variables

        private Dictionary<string, PdfContentOperatorHandler> _operators = new Dictionary<string, PdfContentOperatorHandler>();
        private Stack<PdfContentStreamBuilder> _contentStreamBuilderStack = new Stack<PdfContentStreamBuilder>();
        private PdfResourceDictionary _resourceDictionaryStack = new PdfResourceDictionary();

        #endregion

        #region Modify

        public void Modify(PdfReader reader, int pageNumber)
        {
            PdfDictionary pageDictionary = reader.GetPageN(pageNumber);
            PdfDictionary resourcesDictionary = pageDictionary.GetAsDict(PdfName.RESOURCES);

            byte[] contentBytes = reader.GetPageContent(pageNumber);

            contentBytes = this.Modify(contentBytes, resourcesDictionary);

            reader.SetPageContent(pageNumber, contentBytes);
        }

        #endregion

        #region Modify

        public byte[] Modify(byte[] contentBytes, PdfDictionary resourcesDictionary)
        {
            _contentStreamBuilderStack.Push(new PdfContentStreamBuilder());
            _resourceDictionaryStack.Push(resourcesDictionary);
            PRTokeniser tokeniser = new PRTokeniser(new RandomAccessFileOrArray(contentBytes));
            PdfContentParser ps = new PdfContentParser(tokeniser);

            List<PdfObject> operands = new List<PdfObject>();

            while (ps.Parse(operands).Count > 0)
            {
                PdfLiteral oper = (PdfLiteral)operands[operands.Count - 1];

                System.Diagnostics.Debug.WriteLine("[Debug] Opr: " + oper.ToString());

                PdfContentOperatorHandler operHandler = null;

                if (_operators.TryGetValue(oper.ToString(), out operHandler))
                {
                    operands = operHandler(oper, operands);
                }

                _contentStreamBuilderStack.Peek().Push(operands);
            }

            _resourceDictionaryStack.Pop();
            return _contentStreamBuilderStack.Pop().GetBytes();
        }

        #endregion

        #region RegisterOperator

        public void RegisterOperator(string oper, PdfContentOperatorHandler callback)
        {
            _operators.Add(oper, callback);
        }

        #endregion

        #region ResourceDictionary

        public PdfResourceDictionary ResourceDictionary
        {
            get { return _resourceDictionaryStack; }
        }

        #endregion

    }
}
