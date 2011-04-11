using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFavReader;

namespace NFavReaderTests {
    /// <summary>
    /// Summary description for ReaderTestCases
    /// </summary>
    [TestClass]
    public class ReaderTestCases {
        private const string PDF_MULTILINE_FILE = "pdf-multiline.pdf";
        private const string PDF_ONE_LINE_FILE = "pdf-one-line.pdf";
        private const string PDF_EMPTY_FILE = "pdf-empty.pdf";

        private TestContext testContextInstance;
        private ReaderEngine _engine;
        private static string _locationPath;
        private static string _pdfOneLineFilePath;
        private static string _pdfMultiLineFilePath;
        private static string _pdfEmptyFilePath;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // Use ClassInitialize to run code before running the first test in the class
         [ClassInitialize()]
         public static void MyClassInitialize(TestContext testContext){
             Assembly assembly = Assembly.GetAssembly(typeof(ReaderEngine));
             if (assembly == null)
                 throw new InvalidOperationException("Reader assembly not found");
             _locationPath = new FileInfo(assembly.Location).DirectoryName;
             _pdfEmptyFilePath = Path.Combine(_locationPath, PDF_EMPTY_FILE);
             _pdfOneLineFilePath = Path.Combine(_locationPath, PDF_ONE_LINE_FILE);
             _pdfMultiLineFilePath = Path.Combine(_locationPath, PDF_MULTILINE_FILE);
         }

        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
         [TestInitialize()]
         public void MyTestInitialize(){
             _engine = new ReaderEngine();
         }

        // Use TestCleanup to run code after each test has run
         [TestCleanup()]
         public void MyTestCleanup() { }
        
        #endregion

        [TestMethod]
        public void TestFileExists(){
             _engine.FileName = _pdfEmptyFilePath;
             Assert.IsTrue(_engine.FileExists);
        }

        [TestMethod]
        public void TestOpenPdfFile() {
            _engine.FileName = _pdfOneLineFilePath;
            using(var reader = new StreamReader(_engine.FileStream, true)){
                Assert.IsTrue(reader.ReadToEnd().Length > 0);
            }
        }

        [TestMethod]
        public void TestGetMarkerAttributes(){
            _engine.FileName = _pdfEmptyFilePath;
            var pdfStructure = _engine.LoadPdfStructure();
        }

        [TestMethod]
        public void TestArrayOfObjectsParser(){
            var contentObjects = new Dictionary<int, PdfContentObject>(){{7, new PdfContentObject(7,8,30)},
                                                                         {3, new PdfContentObject(3,4,20)},
                                                                         {1, new PdfContentObject(1,2,10)},
                                                                        };
            var list = PdfEntityParser.GetArrayOfObject(@"[ 1 2 R 3 4 R 7 8 R]", contentObjects);
            Assert.IsNotNull(list);
            Assert.AreEqual(contentObjects.Count, list.Count);
        }

        [TestMethod, Ignore]
        public void TestReadFromStream(){
//            var stream = new MemoryStream(buffer);
            const int BUFFER_SIZE = 1024;
            const string test1 = "test1";
            const string test2 = "test2";
            char[] targetCharArray = "test4".ToCharArray();
            var targetTextBuffer = new byte[BUFFER_SIZE];
            int targetTextByteCount = Encoding.ASCII.GetBytes(targetCharArray, 0, targetCharArray.Length, targetTextBuffer, 0);
            string text = string.Format("{0}\n\n\n{1}\n{2}%", test1, test2, "test3");
//            string text = ("012");
            var buffer = new byte[BUFFER_SIZE];
            char[] textCharArray = text.ToCharArray();
            int textByteCount = Encoding.ASCII.GetBytes(textCharArray, 0, textCharArray.Length, buffer, 0);
            int i = 0;
            int maxIndex = buffer.Length - 1;
            bool found = false;
            while(++i <= maxIndex){
                byte prevByte = buffer[i - 1];
                if (prevByte != '\n' && prevByte != '\r')
                    continue;
                found = true;
                for (int j = 0; j < targetTextByteCount; j++){
                    if (++i <= maxIndex && targetTextBuffer[j] == buffer[i])
                        continue;
                    found = false;
                    break;
                }
            }
        }

        [TestMethod]
        public void TestReadXRefTables(){
            _engine.FileName = _pdfOneLineFilePath;
            using (var reader = new StreamReader(_engine.FileStream, true)){
                var offsets = new Dictionary<int, long>();
                _engine.PopulateObjectOffsets(offsets);
            }
        }

    }
}
