using DonatorPDFGenerator.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DonatorPDFGenerator
{
    public static class PDFManager
    {

        private static BaseFont _baseFont = BaseFont.CreateFont("Helvetica", "Cp1252", false);
        private static Font _normalFont = new Font(_baseFont, 12f, 1, BaseColor.BLACK);
        private static Image _scaledLogo;

        static PDFManager()
        {
            _scaledLogo = GetScaledImage();
        }

        public static void TryWritePdf(Donator donator, string filePath, int currentYear)
        {
            using (Document document = new Document())
            {
                using (PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create)))
                {
                    document.Open();
                    document.Add(new Paragraph(" ")); // Initialize new page

                    if (_scaledLogo != null)
                    {
                        document.Add(_scaledLogo);
                    }

                    var website = new Paragraph(_websiteString, new Font(_baseFont, 14f, 1, BaseColor.BLUE))
                    {
                        Alignment = 1
                    };
                    document.Add(website);

                    Paragraph address = new Paragraph(_addressString, _normalFont)
                    {
                        Alignment = 1
                    };
                    document.Add(address);

                    Paragraph subtitle = new Paragraph(string.Format("{0} Official Donation Receipt", currentYear), new Font(_baseFont, 20f, 1, BaseColor.BLACK))
                    {
                        Alignment = 1
                    };

                    document.Add(subtitle);

                    document.Add(new Paragraph(_donationThankYouMessage, _normalFont));
                    document.Add(new Paragraph(string.Format(_nonProfitMessage, donator.Name), _normalFont));
                    foreach (var donationRow in donator.Donations.OrderBy(d => d.Date))
                    {
                        string str = string.Format("{0} via {1} on {2}", donationRow.Amount.ToString("C"), donationRow.DonationType, donationRow.Date.ToShortDateString());
                        document.Add(new Paragraph(str, _normalFont));
                    }
                    document.Add(new Paragraph(string.Format("\r\nTotal Amount Donated: {0}\r\n", donator.TotalAmountDonated.ToString("C")), _normalFont));
                    document.Add(new Paragraph(_tradeMarkAndDisclaimer, new Font(_baseFont, 10f, 1, BaseColor.GRAY)));
                    document.Close();
                }
            }
        }

        public static void TryCreateDirectory(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
        }

        private static Image GetScaledImage()
        {
            try
            {
                var logoPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _logoPath);
                var image = Image.GetInstance(logoPath);

                if (image == null)
                {
                    System.Console.WriteLine($"Invalid image at {logoPath}.");
                }
                else
                {
                    image.Alignment = 1;
                    image.ScalePercent(50f);
                    return image;
                }
            }
            catch(Exception)
            {
                Console.WriteLine($"Failed to load image at {_logoPath}.");
            }

            return null;
        }
    }
}
