using System.Collections.Generic;

namespace NFavReader{
    public class PdfPagesObject : PdfDictionaryObject{
        public PdfPagesObject(int id, long position, IDictionary<string, object> dictionary) 
            : base(id, position, dictionary){
            Kids = new List<PdfDictionaryObject>();
        }

        public IList<PdfDictionaryObject> Kids { get; set; }

        public override void Validate(IDictionary<int, AbstractPdfDocumentObject> pdfObjects){
            base.Validate(pdfObjects);
            if(!Dictionary.ContainsKey(PdfConstants.Names.Kids))
                throw new PdfException("Pages object doesn't contain Kids collection");
            Kids = new List<PdfDictionaryObject>();
            ((List<AbstractPdfDocumentObject>)Dictionary[PdfConstants.Names.Kids]).ForEach(obj => Kids.Add((PdfDictionaryObject) obj));
        }

        public override string ToString() {
            return string.Format("{0}|{1}|{2}|Kids:{3}", "Pages", Id, Position, Kids.Count);
        }
    }
}