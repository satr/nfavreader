using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NFavReader {
    public class PdfReaderEngine {
        readonly Regex _xrefObjectCounterRegex = new Regex(PdfConstants.XRef.Counter);
        readonly Regex _xrefOffsetRegex = new Regex(PdfConstants.XRef.Row);
        readonly Regex _xrefIntegerRegex = new Regex(PdfConstants.Integer.PATTERN);
        readonly Regex _endOfLineRegex = new Regex(PdfConstants.Patterns.EndOfLine);
        public string FileName { get; set; }

        public Stream FileStream{
            get { return FileExists ? new StreamReader(FileName).BaseStream : new MemoryStream(); }
        }

        public bool FileExists{
            get { return new FileInfo(FileName).Exists; }
        }

        public PdfStructure LoadPdfStructure(){
            var pdfStructure = new PdfStructure();
            if(!FileExists)
                return pdfStructure;
            using (var reader = new StreamReader(FileStream, true))
                ReadPdfDocument(pdfStructure, reader);
            pdfStructure.ValidatePdfObjects();
            return pdfStructure;
        }

        private void ReadPdfDocument(PdfStructure pdfStructure, StreamReader reader){
            var objectOffsets = GetObjectOffsets(pdfStructure, reader);
            pdfStructure.PdfObjects = GetPdfObjects(objectOffsets, reader);
        }

        private IDictionary<int, AbstractPdfDocumentObject> GetPdfObjects(IEnumerable<KeyValuePair<int, long>> objectOffsets, StreamReader reader){
            var objects = new Dictionary<int, AbstractPdfDocumentObject>();
            foreach (var keyValuePair in objectOffsets) {
                var objectId = keyValuePair.Key;
                var objectPosition = keyValuePair.Value;
                objects.Add(objectId, ReadPdfDocumentObject(objectId, objectPosition, reader));
            }
            return objects;
        }

        private AbstractPdfDocumentObject ReadPdfDocumentObject(int objectId, long objectPosition, StreamReader reader){
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(objectPosition, SeekOrigin.Begin);
            string line = reader.ReadLine();
            if(line == null || !line.EndsWith(PdfConstants.Markers.Obj))
                throw new PdfException("Invalid format for object #{0}", objectId);
            long position = objectPosition;
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            while ((line = reader.ReadLine()) != null) {
                if (_xrefIntegerRegex.IsMatch(line))
                    return new PdfScalarObject(objectId, position, long.Parse(line));
                if (line.Equals(PdfConstants.Markers.EndObj)){
                    if(ValidateType(PdfConstants.Names.Pages, dictionary))
                        return new PdfPagesObject(objectId, position, dictionary);
                    if(ValidateType(PdfConstants.Names.Page, dictionary))
                        return new PdfPageObject(objectId, position, dictionary);
                    if(ValidateType(PdfConstants.Names.Catalog, dictionary))
                        return new PdfCatalogObject(objectId, position, dictionary);
                    return new PdfDictionaryObject(objectId, position, dictionary);
//                    throw new PdfException("Unexpected end of a PDF-object");
                }
                if (line.Equals(PdfConstants.Markers.Stream))
                    return new PdfStreamObject(objectId, objectPosition, dictionary, position);
                if (line.StartsWith(PdfConstants.Markers.StartDictionary))
                    dictionary = ReadPdfDictionary(reader, line);
                position = reader.BaseStream.Position;
            }
            throw new PdfException("End of PDF-object was not found");
        }

        private Dictionary<string, object> ReadPdfDictionary(TextReader reader, string startLine){
            var dictionary = new Dictionary<string, object>();
            if(startLine == null)
                return dictionary;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(startLine);
            if (!startLine.EndsWith(PdfConstants.Markers.EndDictionary)){
                string line;
                while ((line = reader.ReadLine()) != null){
                    stringBuilder.AppendFormat("{0}\n", line);
                    if (line.EndsWith(PdfConstants.Markers.EndDictionary))
                        break;
                }
            }
            var dictionaryText = stringBuilder.ToString();
            var match = new Regex(PdfConstants.Dictionary.PATTERN).Match(dictionaryText);
            if (match.Groups[PdfConstants.Dictionary.ENTRY_GROUP].Captures.Count > 0) {
                var keys = match.Groups[PdfConstants.Dictionary.KEY_GROUP].Captures;
                var values = match.Groups[PdfConstants.Dictionary.VALUE_GROUP].Captures;
                int index = 0;
                AddDictionaryEntries(dictionary, keys, values, ref index);
            }
            return dictionary;
        }

        private void AddDictionaryEntries(IDictionary<string, object> dictionary, CaptureCollection keys, CaptureCollection values, ref int index){
            while(index < keys.Count) {
                if (values.Count < index + 1)
                    break;
                var key = keys[index].Value;
                var value = values[index].Value;
                value = _endOfLineRegex.Replace(value, string.Empty);
                index++;
                if (!value.Equals(PdfConstants.Markers.StartDictionary)) {
                    if (value.EndsWith(PdfConstants.Markers.EndDictionary)){
                        dictionary.Add(key, value.Substring(0, value.Length - PdfConstants.Markers.EndDictionary.Length));
                        return;
                    }
                    dictionary.Add(key, value);
                    continue;
                }
                var innerDictionary = new Dictionary<string, object>();
                dictionary.Add(key, innerDictionary);
                AddDictionaryEntries(innerDictionary, keys, values, ref index);
            }
        }

//        private static void ReadPdfContentObjectStream(PdfScalarObject pdfContentObject, StreamReader reader){
//            long startStreamPosition = reader.BaseStream.Position;
//            long endStreamPosition = startStreamPosition;
//            pdfContentObject.Stream = new MemoryStream();
//            string line;
//            while ((line = reader.ReadLine()) != null) {
//                if (line.Equals(PdfConstants.Markers.EndStream)){
//                    reader.BaseStream.Seek(startStreamPosition - endStreamPosition, SeekOrigin.Current);
//                    const int BUFFER_SIZE = 1024;
//                    var buffer = new byte[BUFFER_SIZE];
//                    while (startStreamPosition < endStreamPosition) {
//                        long bytesLeft = endStreamPosition - startStreamPosition;
//                        int bytesToRead = bytesLeft > BUFFER_SIZE ? BUFFER_SIZE : (int)bytesLeft;
//                        var readBytes = reader.BaseStream.Read(buffer, 0, bytesToRead);
//                        pdfContentObject.Stream.Write(buffer, 0, readBytes);
//                    }
//                    break;
//                }
//                endStreamPosition = reader.BaseStream.Position;
//            }
//        }

        public Dictionary<int, long> GetObjectOffsets(PdfStructure pdfStructure, StreamReader reader){
            var objectOffsets = new Dictionary<int, long>();
            long xrefOffset = GetStartXRefOffset(reader);
            PopulateObjectOffsetsAndTrailer(pdfStructure, reader, xrefOffset, objectOffsets);
            if (!pdfStructure.TrailerHasPrevValue)
                return objectOffsets;
            xrefOffset = 0;
            if(!long.TryParse(pdfStructure.TrailerPrevValue, out xrefOffset))
                throw new PdfException("Invalid \\Prev value \"{0}\"", pdfStructure.TrailerPrevValue);
            PopulateObjectOffsetsAndTrailer(pdfStructure, reader, xrefOffset, objectOffsets);
            return objectOffsets;
        }

        public void PopulateObjectOffsetsAndTrailer(PdfStructure pdfStructure, StreamReader reader, long xrefOffset, 
                                                    Dictionary<int, long> objectOffsets){
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(xrefOffset, SeekOrigin.Begin);
            string line = reader.ReadLine();
            if (line != PdfConstants.Markers.XREF)
                throw new PdfException("{0} not found", PdfConstants.Markers.XREF);
            line = reader.ReadLine();
            if (line == null || !_xrefObjectCounterRegex.IsMatch(line))
                throw new PdfException("Invalid object ref counter");
            int objectOverallNum = PdfEntityParser.GetXRefStartObjectNum(line);
            int objectCount = PdfEntityParser.GetXRefObjectCount(line);
            int objectNum = 0;
            while ((line = reader.ReadLine()) != null && objectNum < objectCount) {
                if (_xrefObjectCounterRegex.IsMatch(line)) {
                    objectOverallNum = PdfEntityParser.GetXRefStartObjectNum(line);
                    objectCount = PdfEntityParser.GetXRefObjectCount(line);
                    objectNum = 0;
                    continue;
                }
                if (_xrefOffsetRegex.IsMatch(line)) {
                    if(PdfEntityParser.IsXRefOffsetUsed(line))
                        objectOffsets.Add(objectOverallNum, PdfEntityParser.GetXRefOffset(line));
                    objectOverallNum++;
                    objectNum++;
                    continue;
                }
                if (!line.StartsWith(PdfConstants.Markers.TRAILER))
                    throw new PdfException("{0} was expected", PdfConstants.Markers.TRAILER);
                break;
            }
            line = reader.ReadLine();
            pdfStructure.Trailers.Add(ReadPdfDictionary(reader, line));
        }

        private static long GetStartXRefOffset(StreamReader reader){
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(-100, SeekOrigin.End);
            string line;
            while ((line = reader.ReadLine()) != null
                    && line != PdfConstants.Markers.STARTXREF)
                    continue;
            if (line != PdfConstants.Markers.STARTXREF)
                throw new PdfException(string.Format("{0} value not found", PdfConstants.Markers.STARTXREF));
            long xrefOffset = 0;
            if (!long.TryParse(reader.ReadLine(), out xrefOffset))
                throw new PdfException(string.Format("Invalid {0} value", PdfConstants.Markers.STARTXREF));
            return xrefOffset;
        }

        private bool ValidateType(string type, IDictionary<string, object> dictionary) {
            return GetObject<string>(PdfConstants.Names.Type, dictionary) == type;
        }

        public T GetObject<T>(string key, IDictionary<string, object> dictionary) {
            return GetObject<T>(key, dictionary, false);
        }

        public T GetMandaryObject<T>(string key, IDictionary<string, object> dictionary) {
            return GetObject<T>(key, dictionary, true);
        }

        private static T GetObject<T>(string key, IDictionary<string, object> dictionary, bool isMandatory) {
            if (dictionary.ContainsKey(key))
                return (T)dictionary[key];
            if (isMandatory)
                throw new PdfException("Entity not found by key \"{0}\"", key);
            return default(T);
        }
    }
}

