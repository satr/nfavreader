using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NFavReader {
    public class PdfEntityParser {
        public static AbstractPdfDocumentObject GetObjectByRef(string value, IDictionary<int, AbstractPdfDocumentObject> contentObjects) {
            var match = new Regex(PdfConstants.Object.REF_PATTERN).Match(value);
            GroupCollection groups = match.Groups;
            var idValue = groups[PdfConstants.Object.ID_GROUP].Value;
            var valValue = groups[PdfConstants.Object.VAL_GROUP].Value;
            var typeValue = groups[PdfConstants.Object.TYPE_GROUP].Value;
            return GetObjectByRef(idValue, valValue, typeValue, contentObjects);
        }

        private static AbstractPdfDocumentObject GetObjectByRef(string idValue, string valValue, string typeValue, IDictionary<int, AbstractPdfDocumentObject> contentObjects){
            int objectId;
            int.TryParse(idValue, out objectId);
            int val;
            int.TryParse(valValue, out val);
            return typeValue.ToUpper() == PdfConstants.Object.REF_TYPE || contentObjects.ContainsKey(objectId)
                       ? contentObjects[objectId]
                       : null;
        }

        public static List<PdfDocumentScalarObject> GetArrayOfObject(string value, IDictionary<int, AbstractPdfDocumentObject> contentObjects){
            var refContentObjects = new List<PdfDocumentScalarObject>();
            MatchCollection matches = new Regex(PdfConstants.Array.OBJECTS_PATTERN).Matches(value);
            foreach (Match match in matches) {
                if (match.Groups.Count == 5) {
                    var idValues = match.Groups[PdfConstants.Object.ID_GROUP].Captures;
                    var valValues = match.Groups[PdfConstants.Object.VAL_GROUP].Captures;
                    var typeValues = match.Groups[PdfConstants.Object.TYPE_GROUP].Captures;
                    for (int i = 0; i < idValues.Count; i++){
                        var contentObject = GetObjectByRef(idValues[i].Value, valValues[i].Value, typeValues[i].Value, 
                                                           contentObjects);
                        if(contentObject != null)
                            refContentObjects.Add(contentObject);
                    }
                }
            }
            return refContentObjects;
        }

        public static long GetXRefOffset(string value){
            long numVal = 0;
            long.TryParse(value.Substring(0, value.IndexOf(" ")), out numVal);
            return numVal;
        }

        public static bool IsXRefOffsetUsed(string value){
            return value.Trim().ToLower().EndsWith("n");
        }

        public static int GetXRefStartObjectNum(string value){
            int numVal = 0;
            int.TryParse(value.Substring(0, value.IndexOf(" ")), out numVal);
            return numVal;
        }

        public static int GetXRefObjectCount(string value){
            int numVal = 0;
            int startIndex = value.IndexOf(" ") + 1;
            int.TryParse(value.Substring(startIndex, value.Length - startIndex), out numVal);
            return numVal;
        }
    }
}
