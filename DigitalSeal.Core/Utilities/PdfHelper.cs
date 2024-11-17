using System.Text;

namespace DigitalSeal.Core.Utilities
{
    public class PdfHelper
    {
        public const string PdfExtension = ".pdf";

        public static bool IsPdfFile(byte[] fileContent)
        {
            var pdfString = "%PDF-";
            var pdfBytes = Encoding.ASCII.GetBytes(pdfString);
            var length = pdfBytes.Length;
            var buffer = new byte[length];
            var remaining = length;
            var position = 0;
            using var stream = new MemoryStream(fileContent);
            while (remaining > 0)
            {
                var amtRead = stream.Read(buffer, position, remaining);
                if (amtRead == 0)
                {
                    return false;
                }

                remaining -= amtRead;
                position += amtRead;
            }
            return pdfBytes.SequenceEqual(buffer);
        }
    }
}
