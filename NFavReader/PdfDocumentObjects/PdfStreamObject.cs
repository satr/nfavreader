using System.Collections.Generic;
using System.IO;

namespace NFavReader{
    public class PdfStreamObject : PdfDictionaryObject{
        public PdfStreamObject(int id, long position, IDictionary<string, object> dictionary, long streamPosition)
            : base(id, position, dictionary){
            StreamPosition = streamPosition;
        }

        public PdfScalarObject Length { get; set; }

        public long StreamPosition { get; set; }

        public MemoryStream Stream { get; set; }

        public override void Validate(IDictionary<int, AbstractPdfDocumentObject> pdfObjects) {
            base.Validate(pdfObjects);
            if (!Dictionary.ContainsKey(PdfConstants.Names.Length))
                throw new PdfException("Stream object doesn't contain Length entry");
            Length = (PdfScalarObject)Dictionary[PdfConstants.Names.Length];
        }
        
        public override string ToString(){
            var length = Length == null? 0: Length.Value;
            return string.Format("{0}|{1}|{2}|Length:{3}", "Stream", Id, Position, length);
        }
    }
}