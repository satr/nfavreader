namespace NFavReader{
    public abstract class AbstractPdfDocumentObject{
        public int Id { get; private set; }
        public long Position { get; private set; }

        protected AbstractPdfDocumentObject(int id, long position) {
            Id = id;
            Position = position;
        }
    }
}