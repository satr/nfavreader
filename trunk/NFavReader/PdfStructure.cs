using System.Linq;
using System.Collections.Generic;
using NFavReader.Validation;

namespace NFavReader{
    public class PdfStructure{
        private readonly object _sync = new object();

        public PdfStructure(){
            Trailers = new List<IDictionary<string, object>>();
            PdfObjects = new Dictionary<int, AbstractPdfDocumentObject>();
        }

        public IList<IDictionary<string, object>> Trailers { get; private set; }

        public IDictionary<int, AbstractPdfDocumentObject> PdfObjects { get; set; }

        public bool TrailerHasPrevValue{
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

        public void ValidatePdfObjects(){
            foreach (var pdfObject in PdfObjects.Values.OfType<PdfDictionaryObject>())
                pdfObject.Validate(PdfObjects);
            foreach (var trailer in Trailers)
                PdfDictionaryValidator.Validate(trailer, PdfObjects);
            Root = GetTrailerObject<PdfCatalogObject>(PdfConstants.Names.Root, true);
//            PopulatePages(Root.Pages);
        }

        private void PopulatePages(List<PdfPagesObject> pdfDocumentPagesObjects){
//            parentContentObject.Pages = parentContentObject.GetObject<IList<PdfScalarObject>>(PdfConstants.Names.Kids);
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

        private PdfCatalogObject Root { get; set; }

    }
}