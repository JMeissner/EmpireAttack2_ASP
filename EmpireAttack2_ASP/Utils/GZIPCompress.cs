using System;
using System.IO;
using System.IO.Compression;

namespace EmpireAttack2_ASP.Utils
{
    public static class GZIPCompress
    {
        public static string Compress(string input)
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            string outputbase64;

            using (var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);

                var outputBytes = outputStream.ToArray();

                outputbase64 = Convert.ToBase64String(outputBytes);
            }
            return outputbase64;
        }
    }
}
