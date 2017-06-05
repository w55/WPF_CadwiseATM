using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadwiseATM
{
    public class ScreenInfo
    {
        public EnScreens CurrentEnScreen { get; set; }
        public string Screen1 { get; set; }
        public string Screen2 { get; set; }
        public string Screen3 { get; set; }
        public string Screen4 { get; set; }
        public string Screen5 { get; set; }
        public string Screen6 { get; set; }
        public string Screen7 { get; set; }
        public string Screen8 { get; set; }

        public string ScreenTop { get; set; }
        public string HelpInfo { get; set; }
    }

    /// <summary>
    /// Перечисление - экран, к которому собираемся перейти
    /// </summary>
    public enum EnScreens
    {
        Initial = 0,
        Password = 1,
        Actions = 2,
        OutputBanknote = 3,
        OutputSum = 4,
        OutputResult = 5,
        InputQuantity = 6,
        InputBanknote = 7,
        InputResult = 8
    }
}
