using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EmployeeSchedule
{
    public class SalaryRate
    {
        public string Position { get; set; }
        public double Rate { get; set; }

        private static readonly string FilePath = "salary_rates.xml";

        public SalaryRate(string position, double rate)
        {
            Position = position;
            Rate = rate;
        }

        // Завантаження ставок із XML
        public static List<SalaryRate> LoadRates()
        {
            if (!File.Exists(FilePath))
            {
                return new List<SalaryRate>
                {
                    new SalaryRate("Керівник", 1000),
                    new SalaryRate("Адміністратор", 800),
                    new SalaryRate("Старший продавець консультант", 600),
                    new SalaryRate("Продавець консультант", 500),
                    new SalaryRate("Стажер", 300)
                };
            }

            XDocument doc = XDocument.Load(FilePath);
            return doc.Root.Elements("Rate")
                .Select(e => new SalaryRate(
                    e.Element("Position").Value,
                    double.Parse(e.Element("Value").Value)))
                .ToList();
        }

        // Збереження ставок у XML
        public static void SaveRates(List<SalaryRate> rates)
        {
            XDocument doc = new XDocument(
                new XElement("Rates",
                    rates.Select(r =>
                        new XElement("Rate",
                            new XElement("Position", r.Position),
                            new XElement("Value", r.Rate.ToString())))));

            doc.Save(FilePath);
        }
    }
}
