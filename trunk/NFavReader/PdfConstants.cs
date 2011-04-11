﻿namespace NFavReader {
    public class PdfConstants {
        public static class XRef{
            public const string Row = @"^\d{10} \d{5} [fn][ ]*$";
            public const string Couner = @"^\d+ \d+$";
        }

        public static class Markers {
            public static string Percent = "%";
            public static string SpaceObj = " obj";
            public static string EndObj = "endobj";
            public static string Stream = "stream";
            public static string EndStream = "endstream";
            public static string StartDictionary = "<<";
            public static string EndDictionary = ">>";
            public const string STARTXREF = "startxref";
            public const string XREF = "xref";
            public const string TRAILER = "trailer";
        }

        public static class Names{
            public static string Type = "/Type";
            public static string Prev = "/Prev";
        }

        public static class Dictionary{
            public const string PATTERN = @"\A<<(?<ENTRY>(?<KEY>/\w+)((?<VALUE>\[{1}[^\]]+\]{1})|(?<VALUE>[^\[^w][^/^\]]+))[\n\r]{0,1})+>>";
//            public const string PATTERN = @"\A<<(?<1>(?<2>/\w+)((?<3>\[{1}[^\]]+\]{1})|(?<3>[^\[^w][^/^\]]+))[\n\r]{0,1})+>>";
        }

        public static class Integer{
            public const string PATTERN = @"\A[ ]*\d+[ \r\n]*$";
        }

        public static class Array {
            public const string PATTERN = @"\A\[[^\]]*\]$";
            public const string OBJECTS_PATTERN = @"\A\[([ ]*(?<ID>\d+) (?<VAL>\d+) (?<TYPE>R)[ \r\n]*)*\]$";
        }

        public static class Object {
            public const string ID_GROUP = "ID";
            public const string VAL_GROUP = "VAL";
            public const string TYPE_GROUP = "TYPE";
            public const string REF_PATTERN = @"^(?<" + ID_GROUP + @">\d+) (?<" + VAL_GROUP + @">\d+) (?<" + TYPE_GROUP + @">\w+)$";
            public const string REF_TYPE = "R";
        }
    }
}
