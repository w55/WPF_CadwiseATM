using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CadwiseATM
{
    public class Banknote : DependencyObject
    {
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty MaxAmountProperty;
        public static readonly DependencyProperty CurAmountProperty;

        static Banknote()
        {
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Banknote));
            MaxAmountProperty = DependencyProperty.Register("MaxAmount", typeof(int), typeof(Banknote));
            CurAmountProperty = DependencyProperty.Register("CurAmount", typeof(int), typeof(Banknote));
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public int MaxAmount
        {
            get { return (int)GetValue(MaxAmountProperty); }
            set { SetValue(MaxAmountProperty, value); }
        }
        public int CurAmount
        {
            get { return (int)GetValue(CurAmountProperty); }
            set { SetValue(CurAmountProperty, value); }
        }

    }
}

