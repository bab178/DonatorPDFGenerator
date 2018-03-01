using DonatorPDFGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DonatorPDFGenerator
{
    class Program
    {
        private static int _currentYear;
        private static string _currentDirectory;
        private static string _filePath;

        private static void Main(string[] args)
        {
            _currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine(string.Format("Running PDF exporter in {0}\r\n", _currentDirectory));
            Console.WriteLine("This program accepts files with .csv file extension in the following format:\r\nName, Date, Dollar Amount, Donation Type, Notes");
            _filePath = PromptUserForPath();
            _currentYear = PromptUserForDonationYear();

            List<DonationRow> rowsFromFile = ParseRowsFromFile();
            List<Donator> donators = rowsFromFile
                .GroupBy(r => r.Name)
                .Select(g => new Donator(g.Select(s => s)))
                .ToList();

            Console.WriteLine(string.Format("\r\nIndividual Donations: {0}\r\nUnique Donators: {1}\r\nTotal Donated: {2}\r\n", donators.Count, rowsFromFile.Count, rowsFromFile.Sum(r => r.Amount).ToString("C")));

            Console.WriteLine("Creating Donator PDFs...");
            WriteDonatorsToPdf(donators);

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static string PromptUserForPath()
        {
            string fileName = string.Empty;
            string filePath = string.Empty;
            while (string.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine(string.Format("\nEnter file name in directory {0}:", _currentDirectory));
                fileName = Console.ReadLine();
                if (!fileName.EndsWith(".csv"))
                {
                    Console.WriteLine("File is in wrong format.");
                }
                else
                {
                    filePath = Path.Combine(_currentDirectory, fileName);
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("File does not exist.");
                        fileName = string.Empty;
                    }
                }
            }
            return filePath;
        }

        private static int PromptUserForDonationYear()
        {
            int outputYear = 0;
            string inputYear = string.Empty;
            while (string.IsNullOrWhiteSpace(inputYear))
            {
                Console.WriteLine("\r\nEnter the donation year to be printed on the PDFs and titles of PDFs:");
                inputYear = Console.ReadLine();
                if (!int.TryParse(inputYear, out outputYear))
                {
                    Console.WriteLine("Invalid year, must be an integer.");
                    inputYear = string.Empty;
                }
            }
            return outputYear;
        }

        private static void WriteDonatorsToPdf(List<Donator> donators)
        {
            if (!string.IsNullOrEmpty(_currentDirectory))
            {
                var outputDirectory = Path.Combine(_currentDirectory + "\\Output");
                PDFManager.TryCreateDirectory(outputDirectory);
                int successCount = 0;
                foreach (Donator donator in donators)
                {
                    try
                    {
                        string fileName = string.Format("{0} {1} Donation.pdf", donator.Name, _currentYear);
                        string filePath = Path.Combine(outputDirectory, CleanFileName(fileName));

                        PDFManager.TryWritePdf(donator, filePath, _currentYear);

                        Console.WriteLine(string.Format("Successfully created {0}", Path.GetFileName(filePath)));
                        ++successCount;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(string.Format("There was a problem creating PDF for: {0}", donator?.Name ?? "NO NAME"));
                    }
                }
                Console.WriteLine(string.Format("\r\nSuccessfully wrote {0}/{1} PDFs to: {2}\r\n", successCount, donators.Count, outputDirectory));
            }
            else
                Console.WriteLine("\r\nError: Current Directory is invalid\r\n");
        }

        private static List<DonationRow> ParseRowsFromFile()
        {
            int row = 0;
            List<DonationRow> donationRowList = new List<DonationRow>();
            foreach (string readAllLine in File.ReadAllLines(_filePath))
            {
                try
                {
                    // Replace commas in values with '¶', to be replaced back later
                    char[] charArray = readAllLine.Replace("$", "").ToCharArray();
                    for (int i = 0; i < charArray.Length - 1; ++i)
                    {
                        if ((int)charArray[i] == 34)
                        {
                            int num = 0;
                            for (int j = i + 1; j < charArray.Length - 1 && (int)charArray[j] != 34; ++j)
                            {
                                if ((int)charArray[j] == 44)
                                    charArray[j] = '¶';
                                ++num;
                            }
                            i += num + 1;
                        }
                    }
                    string[] pieces = string.Join("", charArray).Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pieces.Length > 0)
                    {
                        string name = pieces[0];
                        DateTime date = DateTime.Parse(pieces[1]);
                        decimal amount = decimal.Parse(pieces[2].Replace('¶'.ToString(), "").Replace("\"", ""));
                        string donationType = pieces[3];
                        string notes = string.Empty;
                        if (pieces.Length == 5)
                        {
                            notes = pieces[4];
                        }
                        donationRowList.Add(new DonationRow(name, date, amount, donationType, notes));
                        ++row;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"Row {row + 1} failed to be parsed.");
                }
            }
            return donationRowList;
        }

        public static string CleanFileName(string name)
        {
            // Remove invalid characters
            name = name.Trim(' ', '"', '\'');
            name = Regex.Replace(name, "[^\\u0000-\\u007F]+", string.Empty);
            string pattern = string.Format("([{0}]*\\.+$)|([{0}]+)", Regex.Escape(new string(Path.GetInvalidFileNameChars())));
            return Regex.Replace(name, pattern, "-"); // Replace anything else with -
        }
    }
}
