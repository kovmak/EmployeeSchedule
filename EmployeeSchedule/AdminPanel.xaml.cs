using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace EmployeeSchedule
{
    public partial class AdminPanel : Window
    {
        private ScheduleViewModel _viewModel;
        private const int DaysInMonth = 31;
        private List<SalaryRate> _salaryRates;

        public AdminPanel(ScheduleViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _salaryRates = SalaryRate.LoadRates();
            EmployeeDataGrid.ItemsSource = _viewModel.Employees;
            SalaryDataGrid.ItemsSource = _salaryRates;

            CalculateSalaries();
        }

        // Перерахунок зарплат
        private void CalculateSalaries()
        {
            foreach (var employee in _viewModel.Employees)
            {
                var rate = _salaryRates.FirstOrDefault(r => r.Position == employee.Position)?.Rate ?? 0;
                employee.Salary = Math.Round(employee.TotalShifts * rate, 2); // Округлення до 2 знаків після коми
            }

            EmployeeDataGrid.Items.Refresh();
        }


        // Збереження ставок у XML
        private void SaveRates_Click(object sender, RoutedEventArgs e)
        {
            SalaryRate.SaveRates(_salaryRates);
            MessageBox.Show("Ставки збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            CalculateSalaries(); // Перераховуємо зарплати після зміни ставок
        }

        // Додавання нового працівника
        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = EmployeeNameTextBox.Text.Trim();
            string newPosition = (PositionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(newPosition))
            {
                var newEmployee = new EmployeeShift(newName, newPosition, DaysInMonth);
                _viewModel.Employees.Add(newEmployee);
                EmployeeShift.SaveToXml("schedule.xml", _viewModel.Employees);

                MessageBox.Show($"Працівника {newName} додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                EmployeeNameTextBox.Clear();
                PositionComboBox.SelectedIndex = -1;
                EmployeeDataGrid.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Введіть ім'я та виберіть посаду!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Вибір працівника для редагування
        private void EmployeeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem is EmployeeShift selectedEmployee)
            {
                EmployeeNameTextBox.Text = selectedEmployee.Name;
                PositionComboBox.SelectedItem = PositionComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == selectedEmployee.Position);
            }
        }

        // Збереження змін у працівнику
        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem is EmployeeShift selectedEmployee)
            {
                selectedEmployee.Name = EmployeeNameTextBox.Text.Trim();
                selectedEmployee.Position = (PositionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                EmployeeShift.SaveToXml("schedule.xml", _viewModel.Employees);
                EmployeeDataGrid.Items.Refresh();
                MessageBox.Show("Зміни збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                AdminPanel adminPanel = new AdminPanel(_viewModel);
                adminPanel.Show();
                this.Close();
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Excel файли (*.xlsx)|*.xlsx",
                Title = "Зберегти список працівників",
                FileName = "Працівники.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Працівники");

                    // Заголовки
                    worksheet.Cell(1, 1).Value = "Працівник";
                    worksheet.Cell(1, 2).Value = "Посада";
                    worksheet.Cell(1, 3).Value = "Кількість змін";
                    worksheet.Cell(1, 4).Value = "Зарплата"; // Додаємо колонку зарплати

                    // Встановлення ширини колонок (в Excel ширина ≈ пікселі / 7)
                    worksheet.Column(1).Width = 21.4; // 150 пікселів
                    worksheet.Column(2).Width = 35.7; // 250 пікселів
                    worksheet.Column(3).Width = 21.4; // 150 пікселів
                    worksheet.Column(4).Width = 21.4; // 150 пікселів (зарплата)

                    // Стиль заголовків
                    var headerRange = worksheet.Range("A1:D1"); // Додаємо колонку D
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    int row = 2;
                    foreach (var employee in _viewModel.Employees)
                    {
                        worksheet.Cell(row, 1).Value = employee.Name;
                        worksheet.Cell(row, 2).Value = employee.Position;
                        worksheet.Cell(row, 3).Value = employee.TotalShifts;
                        worksheet.Cell(row, 4).Value = Math.Round(employee.Salary, 2); // Додаємо зарплату з округленням

                        // Додаємо границі для всіх комірок рядка
                        for (int col = 1; col <= 4; col++)
                        {
                            worksheet.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        row++;
                    }

                    // Включаємо автофільтр
                    worksheet.RangeUsed().SetAutoFilter();

                    try
                    {
                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show("Дані працівників успішно експортовано!", "Експорт", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Помилка при збереженні файлу: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        // Видалення працівника
        private void RemoveEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeDataGrid.SelectedItem is EmployeeShift selectedEmployee)
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете звільнити {selectedEmployee.Name}?",
                                             "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.Employees.Remove(selectedEmployee);
                    EmployeeShift.SaveToXml("schedule.xml", _viewModel.Employees);
                    EmployeeDataGrid.Items.Refresh();

                    // Очищаємо поля після видалення
                    EmployeeNameTextBox.Clear();
                    PositionComboBox.SelectedIndex = -1;

                    MessageBox.Show("Працівника звільнено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Виберіть працівника для звільнення!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}