namespace NFavReader{
    public class PdfScalarObject : AbstractPdfDocumentObject {
        public PdfScalarObject(int id, long position, long number)
            : base(id, position){
            Value = number;
        }

        public long Value { get; set; }

        public override string ToString() {
            return string.Format("{0}|{1}|{2}|Value:{3}", "Scalar", Id, Position, Value);
        }
    }
}