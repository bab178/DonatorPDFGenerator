using System;

namespace DonatorPDFGenerator.Models
{
    public class DonationRow
    {
        public string Name { get; private set; }
        public DateTime Date { get; private set; }
        public decimal Amount { get; private set; }
        public string DonationType { get; set; }
        public string Notes { get; private set; }

        public DonationRow(string name, DateTime date, decimal amount, string donationType, string notes)
        {
            Name = name;
            Date = date;
            Amount = amount;
            DonationType = donationType;
            Notes = notes;
        }
    }
}
