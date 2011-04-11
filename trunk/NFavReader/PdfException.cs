using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFavReader {
    public class PdfException: Exception {
        public PdfException(string format, params object[] args) : base(string.Format(format, args)){
        }
    }
}
