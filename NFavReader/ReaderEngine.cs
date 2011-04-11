using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NFavReader.Validation;

namespace NFavReader {
    public class ReaderEngine {
        readonly Regex _xrefObjectCounterRegex = new Regex(PdfConstants.XRef.Couner);
        readonly Regex _xrefOffsetRegex = new Regex(PdfConstants.XRef.Row);
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
            Validate(pdfStructure);
            return pdfStructure;
        }

        private static void Validate(PdfStructure pdfStructure){
            var contentObjects = pdfStructure.ContentObjects;
            foreach (var pdfContentObject in contentObjects.Values)
                PdfDictionaryValidator.Validate(pdfContentObject.Dictionary, contentObjects);
        }


        private static void ReadPdfDocument(PdfStructure pdfStructure, StreamReader reader){
            string line;
            long position = 0;
            while ((line = reader.ReadLine()) != null) {
                if (line.StartsWith(PdfConstants.Markers.Percent))
                    pdfStructure.AddPercentMarkerValue(line.Substring(1));
                if (line.EndsWith(PdfConstants.Markers.SpaceObj))
                    ReadPdfContentObject(pdfStructure, reader, position, line);
                position = reader.BaseStream.Position;
            }
        }

        private static void ReadPdfContentObject(PdfStructure pdfStructure, StreamReader reader, long position, string startObjectLine){
            string line;
            var pdfContentObject = pdfStructure.CreateObject(startObjectLine, position);
            while ((line = reader.ReadLine()) != null) {
                if (line.Equals(PdfConstants.Markers.EndObj))
                    break;
                if (line.Equals(PdfConstants.Markers.Stream))
                    ReadPdfContentObjectStream(pdfContentObject, reader);
                if (line.StartsWith(PdfConstants.Markers.StartDictionary))
                    pdfContentObject.Dictionary = ReadPdfDictionary(reader, line);
            }
        }

        private static Dictionary<string, object> ReadPdfDictionary(TextReader reader, string startLine){
            var dictionary = new Dictionary<string, object>();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(startLine);
            if (!startLine.EndsWith(PdfConstants.Markers.EndDictionary)){
                string line;
                while ((line = reader.ReadLine()) != null){
                    if (line.Equals(PdfConstants.Markers.EndDictionary))
                        break;
                    stringBuilder.AppendFormat("{0}\n", line);
                }
            }
            var match = new Regex(PdfConstants.Dictionary.PATTERN).Match(stringBuilder.ToString());
            if (match.Groups.Count == 4) {
                var keys = match.Groups[2].Captures;
                var values = match.Groups[3].Captures;
                int index = 0;
                AddDictionaryEntries(dictionary, keys, values, ref index);
            }
            return dictionary;
        }

        private static void AddDictionaryEntries(IDictionary<string, object> dictionary, CaptureCollection keys, CaptureCollection values, ref int index){
            while(index < keys.Count) {
                if (values.Count < index + 1)
                    break;
                var key = keys[index].Value;
                var value = values[index].Value;
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

        private static void ReadPdfContentObjectStream(PdfContentObject pdfContentObject, StreamReader reader){
            long startStreamPosition = reader.BaseStream.Position;
            long endStreamPosition = startStreamPosition;
            pdfContentObject.Stream = new MemoryStream();
            string line;
            while ((line = reader.ReadLine()) != null) {
                if (line.Equals(PdfConstants.Markers.EndStream)){
                    reader.BaseStream.Seek(startStreamPosition - endStreamPosition, SeekOrigin.Current);
                    const int BUFFER_SIZE = 1024;
                    var buffer = new byte[BUFFER_SIZE];
                    while (startStreamPosition < endStreamPosition) {
                        long bytesLeft = endStreamPosition - startStreamPosition;
                        int bytesToRead = bytesLeft > BUFFER_SIZE ? BUFFER_SIZE : (int)bytesLeft;
                        var readBytes = reader.BaseStream.Read(buffer, 0, bytesToRead);
                        pdfContentObject.Stream.Write(buffer, 0, readBytes);
                    }
                    break;
                }
                endStreamPosition = reader.BaseStream.Position;
            }
        }

        public void PopulateObjectOffsets(IDictionary<int, long> offsets){
            using (var reader = new StreamReader(FileStream, true)) {
                reader.BaseStream.Seek(-100, SeekOrigin.End);
                var trailer = PopulateObjectOffsetsAndGetTrailer(reader, offsets);
                if (!trailer.ContainsKey(PdfConstants.Names.Prev))
                    return;
                long xrefOffset;
                long.TryParse(trailer[PdfConstants.Names.Prev].ToString(), out xrefOffset);
                reader.DiscardBufferedData();
                reader.BaseStream.Seek(xrefOffset, SeekOrigin.Begin);
                trailer = PopulateObjectOffsetsAndGetTrailer(reader, offsets);
            }
        }

        public IDictionary<string, object> PopulateObjectOffsetsAndGetTrailer(StreamReader reader, IDictionary<int, long> offsets){
            long xRefOffset = GetXRefOffset(reader);
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(xRefOffset, SeekOrigin.Begin);
            string line = reader.ReadLine();
            if(line != PdfConstants.Markers.XREF)
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
                    offsets.Add(objectOverallNum++, PdfEntityParser.GetXRefOffset(line));
                    objectNum++;
                    continue;
                }
                if (!line.StartsWith(PdfConstants.Markers.TRAILER))
                    throw new PdfException("{0} was expected", PdfConstants.Markers.TRAILER);
                break;
            }
            return ReadPdfDictionary(reader, line);
        }

        private static long GetXRefOffset(TextReader reader){
            string line;
            while ((line = reader.ReadLine()) != null){
                if (line != PdfConstants.Markers.STARTXREF)
                    continue;
                long xrefOffset = 0;
                if (!long.TryParse(reader.ReadLine(), out xrefOffset))
                    throw new PdfException(string.Format("Invalid {0} value", PdfConstants.Markers.STARTXREF));
                return xrefOffset;
            }
            throw new PdfException(string.Format("{0} value not found", PdfConstants.Markers.STARTXREF));
        }
    }
}

