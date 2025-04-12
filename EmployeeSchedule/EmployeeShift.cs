using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Windows.Media;

namespace EmployeeSchedule
{
    public class EmployeeShift : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Position { get; set; }

        public ObservableCollection<double> Shifts { get; set; }
        public ObservableCollection<string> ShiftColors { get; set; }
        public double Salary { get; set; }

        private double _totalShifts;
        public double TotalShifts
        {
            get => _totalShifts;
            private set
            {
                _totalShifts = value;
                OnPropertyChanged(nameof(TotalShifts));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public EmployeeShift(string name, string position, int daysInMonth)
        {
            Name = name;
            Position = position;
            Shifts = new ObservableCollection<double>(Enumerable.Repeat(0.0, daysInMonth)); // Порожній день за замовчуванням
            ShiftColors = new ObservableCollection<string>(Enumerable.Repeat("#000000", daysInMonth)); // Чорний (порожній день)

            Shifts.CollectionChanged += (s, e) => RecalculateTotalShifts();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double GetShiftValue(double shift)
        {
            return shift switch
            {
                1 => 1.0,
                1.2 => 1.2,
                0 => 0.0,
                -1 => 0.0,
                _ => shift
            };
        }

        public void RecalculateTotalShifts()
        {
            TotalShifts = Math.Round(Shifts.Sum(GetShiftValue), 1);
        }

        // === Збереження в XML ===
        public static void SaveToXml(string filePath, ObservableCollection<EmployeeShift> employees)
        {
            var doc = new XDocument(new XElement("Schedule",
                employees.Select(emp => new XElement("Employee",
                    new XAttribute("Name", emp.Name),
                    new XAttribute("Position", emp.Position),
                    new XAttribute("TotalShifts", emp.TotalShifts),
                    new XElement("Shifts", string.Join(",", emp.Shifts)),
                    new XElement("ShiftColors", string.Join(",", emp.ShiftColors))
                ))
            ));
            doc.Save(filePath);
        }

        // === Завантаження з XML ===
        public static ObservableCollection<EmployeeShift> LoadFromXml(string filePath, int daysInMonth)
        {
            var employees = new ObservableCollection<EmployeeShift>();

            if (File.Exists(filePath))
            {
                var doc = XDocument.Load(filePath);
                foreach (var empElem in doc.Descendants("Employee"))
                {
                    var name = empElem.Attribute("Name")?.Value ?? "Невідомий";
                    var position = empElem.Attribute("Position")?.Value ?? "Стажер";
                    var shifts = empElem.Element("Shifts")?.Value.Split(',')
                        .Select(s => double.TryParse(s, out var val) ? val : 0)
                        .ToList();

                    var shiftColors = empElem.Element("ShiftColors")?.Value.Split(',')
                        .ToList() ?? Enumerable.Repeat("#FFFFFF", daysInMonth).ToList();

                    var employee = new EmployeeShift(name, position, daysInMonth);
                    for (int i = 0; i < Math.Min(shifts.Count, daysInMonth); i++)
                    {
                        employee.Shifts[i] = shifts[i];
                        employee.ShiftColors[i] = shiftColors[i];
                    }

                    employee.RecalculateTotalShifts();
                    employees.Add(employee);
                }
            }

            return employees;
        }
    }
}
