using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NFavReader.Validation{
    internal static class PdfDictionaryValidatorStrategy{
        public static AbstractPdfDictionaryValidator GetStrategyFor(string key, IDictionary<string, object> dictionary, IDictionary<int, PdfContentObject> contentObjects) {
            object value = dictionary[key];
            if (value == null)
                return new PdfDictionaryNullValidator();
            if (value is Dictionary<string, object>)
                return new PdfDictionarySubDictionaryValidator((Dictionary<string, object>)value, contentObjects);
            if (!(value is string))
                return new PdfDictionaryNullValidator();
            var stringValue = ((string) value).Trim();
            if (Regex.IsMatch(stringValue, PdfConstants.Integer.PATTERN))
                return new PdfDictionaryIntegerValidator(dictionary, key, stringValue);
            if (Regex.IsMatch(stringValue, PdfConstants.Object.REF_PATTERN))
                return new PdfDictionaryObjectReferenceValidator(dictionary, key, stringValue, contentObjects);
            if (Regex.IsMatch(stringValue, PdfConstants.Array.PATTERN)){
                if (Regex.IsMatch(stringValue, PdfConstants.Array.OBJECTS_PATTERN))
                    return new PdfDictionaryArrayOfObjectsValidator(dictionary, key, stringValue, contentObjects);
            }
            return new PdfDictionaryNullValidator();
        }
    }
}