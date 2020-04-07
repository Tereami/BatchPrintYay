using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;


namespace BatchPrintYay.pdf
{
    public class PdfResourceDictionary : PdfDictionary
    {

        #region Private variables

        private List<PdfDictionary> _resourceStack = new List<PdfDictionary>();

        #endregion

        #region Push

        public void Push(PdfDictionary resources)
        {
            _resourceStack.Add(resources);
        }

        #endregion

        #region Pop

        public void Pop()
        {
            _resourceStack.RemoveAt(_resourceStack.Count - 1);
        }

        #endregion

        #region GetDirectObject

        public override PdfObject GetDirectObject(PdfName key)
        {
            for (int index = _resourceStack.Count - 1; index >= 0; index--)
            {
                PdfDictionary subResource = _resourceStack[index];

                if (subResource != null)
                {
                    PdfObject obj = subResource.GetDirectObject(key);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            return base.GetDirectObject(key);
        }

        #endregion

    }
}
