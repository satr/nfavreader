namespace NFavReader{
    public abstract class AbstractPdfDocumentObject{
        public int Id { get; private set; }
        public long Position { get; private set; }

        protected AbstractPdfDocumentObject(int id, long position) {
            Id = id;
            Position = position;
        }

        public override string ToString(){
            return string.Format("{0}|{1}|{2}", "Base", Id, Position);
        }
    }
}