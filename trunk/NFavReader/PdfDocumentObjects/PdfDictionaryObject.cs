using System.Collections.Generic;
using NFavReader.Validation;

namespace NFavReader{
    public class PdfDictionaryObject : AbstractPdfDocumentObject{
        public PdfDictionaryObject(int id, long position, IDictionary<string, object> dictionary) 
            : base(id, position){
            Dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary { get; set; }

        public virtual void Validate(IDictionary<int, AbstractPdfDocumentObject> pdfObjects){
            PdfDictionaryValidator.Validate(Dictionary, pdfObjects);
        }
    }
}