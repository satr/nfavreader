using System.Collections.Generic;

namespace NFavReader{
    public class PdfDocumentPageObject : AbstractPdfDocumentExtendedObject{
        public PdfDocumentPageObject(int id, long position, IDictionary<string, object> dictionary) : base(id, position, dictionary){
        }
    }
}