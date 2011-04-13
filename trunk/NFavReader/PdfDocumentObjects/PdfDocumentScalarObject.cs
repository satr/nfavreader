namespace NFavReader{
    public class PdfDocumentScalarObject : AbstractPdfDocumentObject {
        public PdfDocumentScalarObject(int id, long position, long number)
            : base(id, position){
            Value = number;
        }

        public long Value { get; set; }
    }
}