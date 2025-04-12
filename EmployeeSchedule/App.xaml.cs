using System.Globalization;
using System.Threading;
using System.Windows;

namespace EmployeeSchedule
{
    public partial class App : Application
    {
        public App()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }
}
