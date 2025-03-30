using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace tax1.Pages
{
    public class TaxCalculatorModel : PageModel
    {
        [BindProperty]
        public double Income { get; set; }
        [BindProperty]
        public string TaxRegime { get; set; }
        public double? TaxAmount { get; set; }

        public void OnPost()
        {
            if (Income > 0 && !string.IsNullOrEmpty(TaxRegime))
            {
                TaxAmount = TaxRegime.ToLower() == "new" ? CalculateTaxNewRegime(Income) : CalculateTaxOldRegime(Income);
            }
        }

        private double CalculateTaxOldRegime(double income)
        {
            double tax = 0;

            if (income <= 250000)
            {
                tax = 0;
            }
            else if (income <= 500000)
            {
                tax = (income - 250000) * 0.05;
            }
            else if (income <= 1000000)
            {
                tax = (250000 * 0.05) + ((income - 500000) * 0.20);
            }
            else
            {
                tax = (250000 * 0.05) + (500000 * 0.20) + ((income - 1000000) * 0.30);
            }

            return tax + (tax * 0.04); // Adding 4% Cess
        }

        private double CalculateTaxNewRegime(double income)
        {
            double tax = 0;

            if (income <= 250000)
            {
                tax = 0;
            }
            else if (income <= 500000)
            {
                tax = (income - 250000) * 0.05;
            }
            else if (income <= 750000)
            {
                tax = (250000 * 0.05) + ((income - 500000) * 0.10);
            }
            else
            {
                tax = (250000 * 0.05) + (250000 * 0.10) + ((income - 750000) * 0.15);
            }

            return tax + (tax * 0.04); // Adding 4% Cess
        }
    }
}
