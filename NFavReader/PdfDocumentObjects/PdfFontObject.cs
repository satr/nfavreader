using System.Collections.Generic;

namespace NFavReader{
    public class PdfFontObject : PdfDictionaryObject{
        public PdfFontObject(int id, long position, IDictionary<string, object> dictionary) : base(id, position, dictionary){
        }
        public override string ToString() {
            return string.Format("{0}|{1}|{2}", "Font", Id, Position);
        }
    }
}