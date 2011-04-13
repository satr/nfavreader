using System.Collections.Generic;

namespace NFavReader{
    public class PdfDocumentCatalogObject : PdfDocumentPagesObject{
        public PdfDocumentCatalogObject(int id, long position, IDictionary<string, object> dictionary) 
            : base(id, position, dictionary){
            Pages = new List<PdfDocumentPagesObject>();
        }

        public List<PdfDocumentPagesObject> Pages { get; set; }
    }
}