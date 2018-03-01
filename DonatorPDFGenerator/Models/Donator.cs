using System.Collections.Generic;
using System.Linq;

namespace DonatorPDFGenerator.Models
{
    public class Donator
    {
        public List<DonationRow> Donations { get; private set; }
        public string Name { get; private set; }
        public decimal TotalAmountDonated { get; private set; }

        public Donator(IEnumerable<DonationRow> donations)
        {
            Donations = donations.ToList();
            Name = donations.FirstOrDefault()?.Name;
            TotalAmountDonated = Donations?.Sum(d => d.Amount) ?? decimal.Zero;
        }

        public override string ToString()
        {
            return string.Format("Name: {0}\nTotal Donations: {1}\nTotal Amount Donated: {2}", Name, Donations?.Count ?? 0, TotalAmountDonated.ToString("C"));
        }
    }
}
