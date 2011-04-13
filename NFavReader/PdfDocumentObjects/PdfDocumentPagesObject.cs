using System.Collections.Generic;

namespace NFavReader{
    public class PdfDocumentPagesObject : AbstractPdfDocumentExtendedObject{
        public PdfDocumentPagesObject(int id, long position, IDictionary<string, object> dictionary) 
            : base(id, position, dictionary){
            Kids = new List<PdfDocumentPagesObject>();
        }

        public List<PdfDocumentPagesObject> Kids { get; set; }
    }
}