using System;
using System.Collections.Generic;

namespace NFavReader{
    public class PdfStructure{
        private readonly object _sync = new object();
        private readonly IList<string> _percentMarkerValues = new List<string>();
        private readonly IDictionary<int, PdfContentObject> _contentObjects = new Dictionary<int, PdfContentObject>();

        public void AddPercentMarkerValue(string value){
            lock(_sync){
                if(!_percentMarkerValues.Contains(value))
                    _percentMarkerValues.Add(value);
            }
        }

        public IDictionary<int, PdfContentObject> ContentObjects{
            get { return _contentObjects; }
        }

        public PdfContentObject CreateObject(string startObjectLine, long position){
            string[] objectProperties = startObjectLine.Split(' ');
            var contentObject = new PdfContentObject(int.Parse(objectProperties[0]), int.Parse(objectProperties[1]), position);
            lock(_sync){
                if(!_contentObjects.ContainsKey(contentObject.Id))
                    _contentObjects.Add(contentObject.Id, contentObject);
            }
            return contentObject;
        }
    }
}