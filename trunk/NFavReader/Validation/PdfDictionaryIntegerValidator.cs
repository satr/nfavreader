using System.Collections.Generic;

namespace NFavReader.Validation{
    class PdfDictionaryIntegerValidator : AbstractPdfDictionaryValidator {
        private string Value { get; set; }

        public PdfDictionaryIntegerValidator(IDictionary<string, object> dictionary, string key, string value)
            : base(dictionary, key) {
            Value = value;
        }

        public override void Validate() {
            int intValue;
            int.TryParse(Value, out intValue);
            Dictionary[Key] = intValue;
        }
    }
}