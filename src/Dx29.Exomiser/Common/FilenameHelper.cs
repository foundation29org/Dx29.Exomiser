using System;
using System.IO;

namespace Dx29.Exomiser
{
    static public class FilenameHelper
    {
        static public string GetExtension(string filename)
        {
            if (filename != null)
            {
                filename = filename.ToLower();
                if (filename.EndsWith(".vcf.gz"))
                {
                    return ".vcf.gz";
                }
                return Path.GetExtension(filename);
            }
            return null;
        }
    }
}
