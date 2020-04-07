using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;

namespace BatchPrintYay.pdf
{
    public delegate List<PdfObject> PdfContentOperatorHandler(PdfLiteral oper, List<PdfObject> operands);
}
