using System.Collections.Generic;

namespace NFavReader.Validation{
    internal class PdfDictionarySubDictionaryValidator : AbstractPdfDictionaryValidator {
        private IDictionary<int, PdfContentObject> ContentObjects { get; set; }

        public PdfDictionarySubDictionaryValidator(IDictionary<string, object> subDictionary, 
                                                    IDictionary<int, PdfContentObject> contentObjects)
            : base(subDictionary, string.Empty){
            ContentObjects = contentObjects;
        }

        public override void Validate(){
            PdfDictionaryValidator.Validate(Dictionary, ContentObjects);
        }
    }
}