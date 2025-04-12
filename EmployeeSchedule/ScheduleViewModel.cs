using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EmployeeSchedule
{
    public class ScheduleViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EmployeeShift> Employees { get; set; } = new ObservableCollection<EmployeeShift>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadEmployeesFromXml(string filePath, int daysInMonth)
        {
            Employees = EmployeeShift.LoadFromXml(filePath, daysInMonth);
            OnPropertyChanged(nameof(Employees));
        }

        public void SaveEmployeesToXml(string filePath)
        {
            EmployeeShift.SaveToXml(filePath, Employees);
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
