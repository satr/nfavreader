using System.Collections.Generic;
using System.IO;

namespace NFavReader{
    public class PdfDocumentContentObject : AbstractPdfDocumentExtendedObject{
        public PdfDocumentContentObject(int id, long position, IDictionary<string, object> dictionary, long streamPosition)
            : base(id, position, dictionary){
            StreamPosition = streamPosition;
        }

        public long StreamPosition { get; set; }

        public MemoryStream Stream { get; set; }

    }
}