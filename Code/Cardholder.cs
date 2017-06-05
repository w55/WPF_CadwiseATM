using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CadwiseATM
{
    public class Cardholder : DependencyObject
    {
        public static readonly DependencyProperty NameProperty;
        public static readonly DependencyProperty BalansProperty;

        static Cardholder()
        {
            NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(Cardholder));
            BalansProperty = DependencyProperty.Register("Balans", typeof(int), typeof(Cardholder));
        }
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public int Balans
        {
            get { return (int)GetValue(BalansProperty); }
            set { SetValue(BalansProperty, value); }
        }
    }
}
