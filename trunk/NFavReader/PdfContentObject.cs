using System.Collections.Generic;
using System.IO;

namespace NFavReader{
    public class PdfContentObject{
        public PdfContentObject(int id, int value, long position){
            Id = id;
            Value = value;
            Position = position;
            Dictionary = new Dictionary<string, object>();
        }

        public int Id { get; private set; }
        public int Value { get; private set; }
        public long Position { get; private set; }

        public MemoryStream Stream { get; set; }

        public IDictionary<string, object> Dictionary { get; set; }
    }
}