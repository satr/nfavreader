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
            var value = Dictionary[PdfConstants.Names.Length];
            if (value is PdfScalarObject)
                Length = (PdfScalarObject) value;
            else if (value is int)
                Length = new PdfScalarObject(0, 0, (int)value);
            else if (value is long)
                Length = new PdfScalarObject(0, 0, (long)value);
            else
                throw new PdfException("Length entry type was not recognized");
        }
        
        public override string ToString(){
            var length = Length == null? 0: Length.Value;
            return string.Format("{0}|{1}|{2}|Length:{3}", "Stream", Id, Position, length);
        }
    }
}