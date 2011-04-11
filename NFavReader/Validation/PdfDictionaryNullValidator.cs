using System.Collections.Generic;

namespace NFavReader.Validation{
    internal class PdfDictionaryNullValidator : AbstractPdfDictionaryValidator {
        public PdfDictionaryNullValidator()
            : base(new Dictionary<string, object>(), string.Empty) {
        }

        public override void Validate() {
        }
    }
}