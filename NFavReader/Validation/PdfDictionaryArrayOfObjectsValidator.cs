using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NFavReader.Validation{
    internal class PdfDictionaryArrayOfObjectsValidator : AbstractPdfDictionaryValidator{
        private string Value { get; set; }
        private IDictionary<int, AbstractPdfDocumentObject> ContentObjects { get; set; }

        public PdfDictionaryArrayOfObjectsValidator(IDictionary<string, object> dictionary, string key, string value, IDictionary<int, AbstractPdfDocumentObject> contentObjects)
            : base(dictionary, key){
            Value = value;
            ContentObjects = contentObjects;
        }

        public override void Validate(){
            Dictionary[Key] = PdfEntityParser.GetArrayOfObject(Value, ContentObjects);
        }
    }
}