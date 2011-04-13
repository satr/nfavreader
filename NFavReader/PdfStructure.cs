using System.Linq;
using System.Collections.Generic;
using NFavReader.Validation;

namespace NFavReader{
    public class PdfStructure{
        private readonly object _sync = new object();
        private readonly IDictionary<int, AbstractPdfDocumentObject> _contentObjects = new Dictionary<int, AbstractPdfDocumentObject>();

        public PdfStructure(){
            ObjectOffsets = new Dictionary<int, long>();
            Trailers = new List<IDictionary<string, object>>();
        }

        public IDictionary<int, long> ObjectOffsets { get; private set; }
        public IList<IDictionary<string, object>> Trailers { get; private set; }

        public IDictionary<int, AbstractPdfDocumentObject> ContentObjects{
            get { return _contentObjects; }
        }

        public bool HasPrevValue{
            get{
                return TrailerPrevValue != null;
            }
        }

        public string TrailerPrevValue{
            get{
                foreach (var trailer in Trailers){
                    if (trailer.ContainsKey(PdfConstants.Names.Prev))
                        return trailer[PdfConstants.Names.Prev].ToString();
                }
                return null;
            }
        }

        public void Add(AbstractPdfDocumentObject pdfDocumentObject) {
            lock(_sync){
                if (!_contentObjects.ContainsKey(pdfDocumentObject.Id))
                    _contentObjects.Add(pdfDocumentObject.Id, pdfDocumentObject);
            }
        }

        public void Validate(){
            var contentObjects = ContentObjects;
            foreach (var pdfContentObject in contentObjects.Values.OfType<AbstractPdfDocumentExtendedObject>())
                PdfDictionaryValidator.Validate(pdfContentObject.Dictionary, contentObjects);
            foreach (var trailer in Trailers)
                PdfDictionaryValidator.Validate(trailer, contentObjects);
            Root = GetTrailerObject<PdfDocumentCatalogObject>(PdfConstants.Names.Root, true);
            PopulatePages(Root.Pages);
        }

        private void PopulatePages(List<PdfDocumentPagesObject> pdfDocumentPagesObjects){
//            parentContentObject.Pages = parentContentObject.GetObject<IList<PdfDocumentScalarObject>>(PdfConstants.Names.Kids);
//            foreach (var contentObject in parentContentObject.Pages)
//                PopulatePages(contentObject);
        }

        private T GetTrailerObject<T>(string key, bool isMandatory){
            foreach (var dictionary in Trailers){
                if (dictionary.ContainsKey(key))
                    return (T)dictionary[key];
            }
            if (isMandatory)
                throw new PdfException("Entity not found in trailer by key \"{0}\"", key);
            return default(T);
        }

        private PdfDocumentCatalogObject Root { get; set; }

    }
}