using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace EmployeeSchedule
{
    public partial class MainWindow : Window
    {
        private ScheduleViewModel _viewModel;
        private const int DaysInMonth = 31;
        private const string FilePath = "schedule.xml";

        public bool IsAuthenticated { get; private set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new ScheduleViewModel();
            DataContext = _viewModel;
            AdminPanelButton.IsEnabled = false;

            _viewModel.LoadEmployeesFromXml(FilePath, DaysInMonth);

            // Додаємо колонку з іменем працівника
            ScheduleDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Працівник",
                Binding = new Binding("Name"),
                IsReadOnly = true,
                Width = 120
            });

            // Додаємо колонки для кожного дня місяця
            for (int i = 0; i < DaysInMonth; i++)
            {
                ScheduleDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = (i + 1).ToString(),
                    Width = 33,
                    Binding = new Binding($"Shifts[{i}]")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay
                    }
                });
            }

            ScheduleDataGrid.ItemsSource = _viewModel.Employees;
            ScheduleDataGrid.RowHeight = 30;
            ScheduleDataGrid.LoadingRow += ScheduleDataGrid_LoadingRow;
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel файли (*.xlsx)|*.xlsx",
                Title = "Зберегти графік змін",
                FileName = "Графік_змін.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Графік змін");

                    // Заголовки таблиці
                    worksheet.Cell(1, 1).Value = "Працівник";
                    worksheet.Column(1).Width = 25;
                    worksheet.Style.Font.Bold = true;

                    for (int i = 1; i <= DaysInMonth; i++)
                    {
                        var headerCell = worksheet.Cell(1, i + 1);
                        headerCell.Value = i;
                        headerCell.Style.Font.Bold = true; // Жирний шрифт для заголовків
                        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i + 1).Width = 5;
                    }

                    int row = 2;
                    foreach (var employee in _viewModel.Employees)
                    {
                        worksheet.Row(row).Height = 20;
                        var nameCell = worksheet.Cell(row, 1);
                        nameCell.Value = employee.Name;
                        nameCell.Style.Font.Bold = true;
                        nameCell.Style.Fill.BackgroundColor = XLColor.Orange; // Колір фону для імен
                        nameCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        for (int day = 0; day < DaysInMonth; day++)
                        {
                            var cell = worksheet.Cell(row, day + 2);
                            cell.Value = employee.Shifts[day];

                            // Встановлення жирного шрифту для змін
                            cell.Style.Font.Bold = true;

                            // Встановлення кольору фону
                            var colorHex = employee.ShiftColors[day];
                            if (!string.IsNullOrWhiteSpace(colorHex))
                            {
                                try
                                {
                                    var color = XLColor.FromHtml(colorHex);
                                    cell.Style.Fill.BackgroundColor = color;
                                }
                                catch
                                {
                                    // Якщо колір некоректний, ігноруємо його
                                }
                            }

                            // Додаємо границі
                            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        }

                        row++;
                    }

                    workbook.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Графік змін успішно експортовано!", "Експорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }



        private void ScheduleDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is EmployeeShift employee)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    // Фарбуємо колонку "Працівник" у помаранчевий
                    var nameCell = GetCell(e.Row, 0);
                    if (nameCell != null)
                    {
                        nameCell.Background = Brushes.Orange;
                        nameCell.Foreground = Brushes.Black;
                        nameCell.FontWeight = FontWeights.Bold;
                    }

                    for (int i = 0; i < DaysInMonth; i++)
                    {
                        var cell = GetCell(e.Row, i + 1);
                        if (cell != null && i < employee.ShiftColors.Count)
                        {
                            try
                            {
                                var color = (Color)ColorConverter.ConvertFromString(employee.ShiftColors[i]);
                                cell.Background = new SolidColorBrush(color);
                            }
                            catch
                            {
                                cell.Background = Brushes.White;
                            }
                        }
                    }
                });
            }
        }

        private DataGridCell GetCell(DataGridRow row, int columnIndex)
        {
            if (row == null || columnIndex < 0) return null;

            DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null)
            {
                row.ApplyTemplate();
                presenter = FindVisualChild<DataGridCellsPresenter>(row);
            }

            return presenter?.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T tChild)
                    return tChild;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void SaveTable_Click(object sender, RoutedEventArgs e)
        {
            EmployeeShift.SaveToXml(FilePath, _viewModel.Employees);
            MessageBox.Show("Графік змін успішно збережено!", "Збереження", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginText.Visibility = Visibility.Visible;
            LoginTextBox.Visibility = Visibility.Visible;

            PasswordText.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Visible;

            AutorizationButton.Visibility = Visibility.Visible;
        }


        private void OpenAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            if (AdminPanelButton.IsEnabled) // Перевіряємо, чи кнопка активна (авторизація пройдена)
            {
                AdminPanel adminPanel = new AdminPanel(_viewModel);
                adminPanel.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Доступ заборонено. Увійдіть як адміністратор!", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void ChangeShiftValue(double shiftValue, string color)
        {
            if (ScheduleDataGrid.SelectedCells.Count > 0)
            {
                foreach (var cell in ScheduleDataGrid.SelectedCells)
                {
                    if (cell.Item is EmployeeShift employee && cell.Column is DataGridTextColumn column)
                    {
                        int columnIndex = ScheduleDataGrid.Columns.IndexOf(column) - 1;
                        if (columnIndex >= 0 && columnIndex < DaysInMonth)
                        {
                            employee.Shifts[columnIndex] = shiftValue;
                            employee.ShiftColors[columnIndex] = color; // Оновлення кольору
                            employee.RecalculateTotalShifts();
                        }
                    }
                }

                ScheduleDataGrid.Items.Refresh();
            }
        }

        private void ClearShifts_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Ви впевнені, що хочете очистити всі зміни?", "Підтвердження",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (var employee in _viewModel.Employees)
                {
                    for (int i = 0; i < employee.Shifts.Count; i++)
                    {
                        employee.Shifts[i] = 0; // Порожній день
                        employee.ShiftColors[i] = "#000000"; // Чорний колір
                    }
                }

                _viewModel.SaveEmployeesToXml(FilePath);

                _viewModel.OnPropertyChanged(nameof(_viewModel.Employees));

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();

                MessageBox.Show("Всі зміни успішно очищені!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void AutorizationButton_Click(object sender, RoutedEventArgs e)
        {
            const string AdminLogin = "admin";
            const string AdminPassword = "admin";

            if (LoginTextBox.Text == AdminLogin && PasswordBox.Password == AdminPassword)
            {
                IsAuthenticated = true;
                AdminPanelButton.IsEnabled = true; // Розблоковуємо кнопку "Адміністративна панель"
                MessageBox.Show("Вхід виконано успішно!", "Авторизація", MessageBoxButton.OK, MessageBoxImage.Information);

                LoginText.Visibility = Visibility.Hidden;
                LoginTextBox.Visibility = Visibility.Hidden;

                PasswordText.Visibility = Visibility.Hidden;
                PasswordBox.Visibility = Visibility.Hidden;

                AutorizationButton.Visibility = Visibility.Hidden;

                LoginButton.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("Неправильний логін або пароль!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void EmptyDay_Click(object sender, RoutedEventArgs e)
        {
            ChangeShiftValue(0, "#000000");
        }

        private void MorningShift_Click(object sender, RoutedEventArgs e)
        {
            ChangeShiftValue(1, "#FFB6C1");
        }

        private void EveningShift_Click(object sender, RoutedEventArgs e)
        {
            ChangeShiftValue(1, "#90EE90");
        }


        private void FullDayShift_Click(object sender, RoutedEventArgs e)
        {
            ChangeShiftValue(1.2, "#FFFFFF");
        }

        private void DayOff_Click(object sender, RoutedEventArgs e)
        {
            ChangeShiftValue(0, "#FFFF00");
        }
    }
}
