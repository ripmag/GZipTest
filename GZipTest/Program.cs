using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
       static GZip zipper;

        static int Main(string[] args)
        {
            ShowInfo();
            try
            {
                Validation.StringReadValidation(args);

                switch (args[0].ToLower())
                {
                    case "compress":
                        zipper = new Compressor(args[1], args[2]);
                    break;
                    case "decompress":
                        zipper = new Decompressor(args[1], args[2]);
                    break;
                }

                zipper.Launch();
                
                return zipper.CallBackResult();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error is occured!\n Method: {0}\n Error description {1}", ex.TargetSite, ex.Message);
                return 1;
            }
        }

       static void ShowInfo()
        {
            Console.WriteLine("To zip or unzip files please proceed with the following pattern to type in:\n" + 
                              "Zip: GZipTest.exe compress [Source file path] [Destination file path]\n" +
                              "Unzip: GZipTest.exe decompress [Compressed file path] [Destination file path]");
        }
    }
}
