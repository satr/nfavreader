using System.Collections.Generic;

namespace NFavReader{
    public class PdfPageObject : PdfDictionaryObject{
        public PdfPageObject(int id, long position, IDictionary<string, object> dictionary) : base(id, position, dictionary){
        }

        public List<PdfDictionaryObject> Content { get; set; }

        public override void Validate(IDictionary<int, AbstractPdfDocumentObject> pdfObjects) {
            base.Validate(pdfObjects);
            if (!Dictionary.ContainsKey(PdfConstants.Names.Contents))
                throw new PdfException("Page object doesn't contain Content object reference");
            Content = new List<PdfDictionaryObject>();
            var content = Dictionary[PdfConstants.Names.Contents];
            if (content is PdfDictionaryObject)
                Content.Add((PdfDictionaryObject)content);
            else
                ((List<AbstractPdfDocumentObject>)content).ForEach(obj => Content.Add((PdfDictionaryObject) obj));
        }

        public override string ToString() {
            return string.Format("{0}|{1}|{2}|Content:({3})", "Page", Id, Position, Content);
        }
    }
}