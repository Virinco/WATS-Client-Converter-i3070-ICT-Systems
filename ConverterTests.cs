using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Virinco.WATS.Interface;

namespace ICTKeysight3070Converter
{
    [TestClass]
    public class ConverterTests : TDM
    {
        [TestMethod]
        public void SetupClient()
        {
            SetupAPI(null, "ICT Keysight i3070 Converter", "ICT Tester", true);
            //RegisterClient("", "", "");
            InitializeAPI(true);
        }

        //[TestMethod]
        //public void ICTKeysight3070Importer()
        //{
        //    InitializeAPI(true);
        //    string fn = "";
        //    Dictionary<string, string> arguments = new ICTKeysight3070Importer().ConverterParameters;
        //    ICTKeysight3070Importer converter = new ICTKeysight3070Importer(arguments);
        //    using (FileStream file = new FileStream(fn, FileMode.Open))
        //    {
        //        SetConversionSource(new FileInfo(fn), converter.ConverterParameters, null);
        //        Report uut = converter.ImportReport(this, file);
        //    }
        //}

        [TestMethod]
        public void ICTKeysight3070ImporterFolder()
        {
            InitializeAPI(true);
            ValidationMode = ValidationModeType.AutoTruncate;
            Dictionary<string, string> arguments = new ICTKeysight3070Importer().ConverterParameters;
            ICTKeysight3070Importer converter = new ICTKeysight3070Importer(arguments);
            foreach (string fn in Directory.GetFiles(@"Data", "*.*", SearchOption.AllDirectories))
            {
                using (FileStream file = new FileStream(fn, FileMode.Open))
                {
                    SetConversionSource(new FileInfo(fn), converter.ConverterParameters, null);
                    Report uut = converter.ImportReport(this, file);
                }
            }
        }
    }
}
