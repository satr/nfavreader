using System.Collections.Generic;

namespace NFavReader{
    public abstract class AbstractPdfDocumentExtendedObject : AbstractPdfDocumentObject{
        protected AbstractPdfDocumentExtendedObject(int id, long position, IDictionary<string, object> dictionary) 
            : base(id, position){
            Dictionary = dictionary;
        }

        public IDictionary<string, object> Dictionary { get; set; }
    }
}