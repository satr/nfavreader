using System.Collections.Generic;

namespace NFavReader{
    public class PdfCatalogObject : PdfDictionaryObject {
        public PdfCatalogObject(int id, long position, IDictionary<string, object> dictionary) 
            : base(id, position, dictionary){
        }

        public PdfPagesObject Pages { get; set; }

        public override void Validate(IDictionary<int, AbstractPdfDocumentObject> pdfObjects){
            base.Validate(pdfObjects);
            if(!Dictionary.ContainsKey(PdfConstants.Names.OpenAction))
                throw new PdfException("Catalog object doesn't contain OpenAction entry");
            //            OpenActionObject = //TODO
            if(!Dictionary.ContainsKey(PdfConstants.Names.Pages))
                throw new PdfException("Catalog object doesn't contain Pages entry");
            if(!(Dictionary[PdfConstants.Names.Pages] is PdfPagesObject))
                throw new PdfException("Catalog object contain an invalid type of a Pages reference object");
            Pages = (PdfPagesObject)Dictionary[PdfConstants.Names.Pages];
        }

        public override string ToString() {
            var pages = Pages == null ? string.Empty : Pages.ToString();
            return string.Format("{0}|{1}|{2}|Pages:({3})", "Catalog", Id, Position, pages);
        }
    }
}