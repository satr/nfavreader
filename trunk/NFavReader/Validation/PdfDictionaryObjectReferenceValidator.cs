using System.Collections.Generic;

namespace NFavReader.Validation{
    internal class PdfDictionaryObjectReferenceValidator : AbstractPdfDictionaryValidator {
        private string Value { get; set; }
        private IDictionary<int, PdfContentObject> ContentObjects { get; set; }

        public PdfDictionaryObjectReferenceValidator(IDictionary<string, object> dictionary, string key, string value, IDictionary<int, PdfContentObject> contentObjects)
            : base(dictionary, key){
            Value = value;
            ContentObjects = contentObjects;
        }

        public override void Validate(){
            var contentObject = PdfEntityParser.GetObjectByRef(Value, ContentObjects);
            if (contentObject == null) 
                return;
            Dictionary[Key] = contentObject;
        }
    }
}