using System.Collections.Generic;
using NFavReader.Validation;

namespace NFavReader.Validation{
    public static class PdfDictionaryValidator{
        public static void Validate(IDictionary<string, object> dictionary, IDictionary<int, PdfContentObject> contentObjects) {
            foreach (var key in new List<string>(dictionary.Keys))
                PdfDictionaryValidatorStrategy.GetStrategyFor(key, dictionary, contentObjects).Validate();
        }
    }
}