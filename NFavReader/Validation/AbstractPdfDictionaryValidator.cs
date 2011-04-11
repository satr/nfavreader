using System.Collections.Generic;

namespace NFavReader.Validation{
    internal abstract class AbstractPdfDictionaryValidator {
        protected IDictionary<string, object> Dictionary { get; set; }
        protected string Key { get; set; }

        protected AbstractPdfDictionaryValidator(IDictionary<string, object> dictionary, string key) {
            Dictionary = dictionary;
            Key = key;
        }

        public abstract void Validate();
    }
}