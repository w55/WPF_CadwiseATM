using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CadwiseATM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region class members

        /// <summary>
        /// Список с информацией для текстовых полей всех отображаемых экранов
        /// </summary>
        List<ScreenInfo> listScreenInfo;

        /// <summary>
        /// Список с информацией об держателях карт
        /// </summary>
        List<Cardholder> listCardholders;

        /// <summary>
        /// Список с информацией о номиналах и количестве находящихся в банкомате купюр
        /// </summary>
        List<Banknote> listATM;

        /// <summary>
        /// Список - для отображения информации о последних проведенных транзакциях
        /// </summary>
        ObservableCollection<LastTransInfo> listLastTransInfo;

        /// <summary>
        /// Вспомогательное поле: длина текущего ПИН-кода
        /// </summary>
        int curPinCodeLength = 0;

        /// <summary>
        /// Запрошенная к выдаче текущим пользователем сумма (для внесения - это количество вносимых купюр)
        /// </summary>
        int requestedAmount = 0;

        /// <summary>
        /// Запрошенный к выдаче (или внесению) текущим пользователем тип купюр
        /// </summary>
        int requestedBanknote = 0;

        /// <summary>
        /// Реальная сумма выданная (или внесенная) пользователю текущими купюрами
        /// </summary>
        int outputAmount = 0;

        #endregion class members

        #region dependency for Cardholder

        /// <summary>
        /// Свойство зависимостей для выбранного в данный момент времени Cardholder
        /// </summary>
        public static readonly DependencyProperty CardholderProperty;

        /// <summary>
        /// Выбранный в данный момент времени Cardholder
        /// </summary>
        public Cardholder CurrentCardholder
        {
            get { return (Cardholder)GetValue(CardholderProperty); }
            set { SetValue(CardholderProperty, value); }
        }

        /// <summary>
        /// Индекс выбранного держателя карты
        /// </summary>
        int curCardholderIndex = 0;

        #endregion dependency for Cardholder

        #region dependency for ScreenInfo

        /// <summary>
        /// Свойство зависимостей для выбранного в данный момент времени ScreenInfo
        /// </summary>
        public static readonly DependencyProperty ScreenInfoProperty;

        /// <summary>
        /// Выбранный в данный момент времени ScreenInfo
        /// </summary>
        public ScreenInfo CurrentScreenInfo
        {
            get { return (ScreenInfo)GetValue(ScreenInfoProperty); }
            set { SetValue(ScreenInfoProperty, value); }
        }

        /// <summary>
        /// Экран, отображаемый в текущий момент времени
        /// </summary>
        EnScreens curScreen = EnScreens.Initial;

        /// <summary>
        ///Ээкран, запрошенный к переходу (используем только для целей анимации)
        /// </summary>
        EnScreens nextScreen = EnScreens.Initial;

        /// <summary>
        /// Обработчик завершения транзакции уже отработал 1 раз ??? (использкем в обработчике анимации только)
        /// </summary>
        bool IsAnimationCompleted = false;


        #endregion dependency for ScreenInfo

        #region static & class c-tors

        static MainWindow()
        {
            CardholderProperty = DependencyProperty.Register(
                "CurrentCardholder",
                typeof(Cardholder),
                typeof(MainWindow));

            ScreenInfoProperty = DependencyProperty.Register(
                "CurrentScreenInfo",
                typeof(ScreenInfo),
                typeof(MainWindow));
        }

        public MainWindow()
        {
            InitializeComponent();

            // инициализатор списка держателей карт и  списка полей экранов
            InitializeScreenInfo();

            // Переходим на начальный экран
            NextScreenInfo(0);
        }

        #endregion static & class c-tors

        #region Initializers for used lists

        /// <summary>
        /// Инициализатор списка держателей карт и списка полей экранов
        /// </summary>
        /// <returns></returns>
        bool InitializeScreenInfo()
        {
            // Инициализатор списка кол-ва и типов купюр в банкомате 
            //
            listATM = new List<Banknote>();

            Banknote note0 = new Banknote() { Title = "10", MaxAmount = 2000, CurAmount = 1950 };
            Banknote note1 = new Banknote() { Title = "50", MaxAmount = 2000, CurAmount = 10 };
            Banknote note2 = new Banknote() { Title = "100", MaxAmount = 1000, CurAmount = 980 };
            Banknote note3 = new Banknote() { Title = "500", MaxAmount = 1000, CurAmount = 12 };
            Banknote note4 = new Banknote() { Title = "1000", MaxAmount = 500, CurAmount = 492 };
            Banknote note5 = new Banknote() { Title = "5000", MaxAmount = 100, CurAmount = 1 };

            listATM.AddRange(new Banknote[] { note0, note1, note2, note3, note4, note5 });

            // Устанавливаем у lvATM источник данных
            //
            lvATM.ItemsSource = listATM;



            // Инициализируем список с информацией о последних проведенных транзакциях
            //
            listLastTransInfo = new ObservableCollection<LastTransInfo>();

            // Устанавливаем у lvLastTransaction источник данных
            //
            lvLastTransaction.ItemsSource = listLastTransInfo;




            // Инициализатор списка держателей карт 
            //
            listCardholders = new List<Cardholder>();

            Cardholder holder0 = new Cardholder() { Name = "Иванов И.И.", Balans = 3000 };
            Cardholder holder1 = new Cardholder() { Name = "Петров П.П.", Balans = 9000 };
            Cardholder holder2 = new Cardholder() { Name = "Сидоров С.С.", Balans = 14000 };
            Cardholder holder3 = new Cardholder() { Name = "Кузнецов К.К.", Balans = 17000 };

            listCardholders.AddRange(new Cardholder[] { holder0, holder1, holder2, holder3 });


            // Инициализатор списка полей экранов
            //
            listScreenInfo = new List<ScreenInfo>();

            ScreenInfo screen0 = new ScreenInfo() { CurrentEnScreen = EnScreens.Initial, Screen1 = "", Screen2 = "", Screen3 = "", Screen4 = "", Screen5 = holder0.Name, Screen6 = holder1.Name, Screen7 = holder2.Name, Screen8 = holder3.Name, ScreenTop = "Добро пожаловать!\r\nВставьте Вашу карту в банкомат", HelpInfo = "Чтобы имитировать вставку карты - нажмите кнопку рядом с владельцем карты" };

            ScreenInfo screen1 = new ScreenInfo() { CurrentEnScreen = EnScreens.Password, Screen1 = "", Screen2 = "", Screen3 = "", Screen4 = "", Screen5 = "", Screen6 = "", Screen7 = "", Screen8 = "Вернуть карту", ScreenTop = "Используя кнопки от 0 до 9 ведите пин-код: ", HelpInfo = "Чтобы имитировать ввод реального ПИН-кода - введите любой четырехзначный код с цифровой клавиатуры" };

            ScreenInfo screen2 = new ScreenInfo() { CurrentEnScreen = EnScreens.Actions, Screen1 = "", Screen2 = "", Screen3 = "", Screen4 = "", Screen5 = "Внесение наличных", Screen6 = "Выдача наличных", Screen7 = "", Screen8 = "Вернуть карту", ScreenTop = "Добро пожаловать, выберите требуемую операцию:", HelpInfo = "Чтобы выбрать требуемую операцию - просто нажмите на кнопку справа от нее" };

            ScreenInfo screen3 = new ScreenInfo() { CurrentEnScreen = EnScreens.OutputSum, Screen1 = "500", Screen2 = "1000", Screen3 = "3000", Screen4 = "5000", Screen5 = "10000", Screen6 = "15000", Screen7 = "20000", Screen8 = "Вернуть карту", ScreenTop = "Выберите сумму для снятия", HelpInfo = "Чтобы выбрать необходимую сумму для снятия - нажмите кнопку рядом с требуемой суммой" };

            ScreenInfo screen4 = new ScreenInfo() { CurrentEnScreen = EnScreens.OutputBanknote, Screen1 = "10", Screen2 = "50", Screen3 = "100", Screen4 = "500", Screen5 = "1000", Screen6 = "5000", Screen7 = "Назад", Screen8 = "Вернуть карту", ScreenTop = "Выберите купюрами какого достоинства произвести выдачу", HelpInfo = "Чтобы выбрать необходимый тип купюр - нажмите кнопку рядом с требуемым типом купюр" };

            ScreenInfo screen5 = new ScreenInfo() { CurrentEnScreen = EnScreens.OutputResult, Screen1 = "", Screen2 = "", Screen3 = "", Screen4 = "", Screen5 = "", Screen6 = "", Screen7 = "Другие операции", Screen8 = "Вернуть карту", ScreenTop = "Заберите Ваши деньги из окна выдачи", HelpInfo = "Производится операция выдачи наличных из банкомата" };

            ScreenInfo screen6 = new ScreenInfo() { CurrentEnScreen = EnScreens.InputBanknote, Screen1 = "10", Screen2 = "50", Screen3 = "100", Screen4 = "500", Screen5 = "1000", Screen6 = "5000", Screen7 = "", Screen8 = "Вернуть карту", ScreenTop = "Выберите купюры какого достоинства будут внесены на Ваш счет", HelpInfo = "Чтобы выбрать необходимый тип купюр - нажмите кнопку рядом с требуемым типом купюр" };

            ScreenInfo screen7 = new ScreenInfo() { CurrentEnScreen = EnScreens.InputQuantity, Screen1 = "5", Screen2 = "10", Screen3 = "15", Screen4 = "20", Screen5 = "25", Screen6 = "30", Screen7 = "Назад", Screen8 = "Вернуть карту", ScreenTop = "Выберите количество вносимых купюр", HelpInfo = "Чтобы выбрать вносимое количество купюр - нажмите кнопку рядом с вносимым количеством купюр" };

            ScreenInfo screen8 = new ScreenInfo() { CurrentEnScreen = EnScreens.InputResult, Screen1 = "", Screen2 = "", Screen3 = "", Screen4 = "", Screen5 = "", Screen6 = "", Screen7 = "Другие операции", Screen8 = "Вернуть карту", ScreenTop = "Купюры приняты банкоматом", HelpInfo = "Производится операция внесения наличных в банкомат" };

            listScreenInfo.AddRange(new ScreenInfo[] { screen0, screen1, screen2, screen3, screen4, screen5, screen6, screen7, screen8 });
            return true;
        }

        #endregion Initializers for used lists

        //
        //-----------    Обработчик нажатий любых кнопок  ---------------
        //
        #region Button_Click()

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string key = "";
            string tag = "";
            int number = -1;

            if (clickedButton.Content != null)
            {
                // Keyboard buttons
                //
                if (int.TryParse(clickedButton.Content.ToString(), out number))
                {
                    key = number.ToString();
                }
                else
                {
                    key = clickedButton.Name;
                }
            }
            else
            {
                // Screen buttons
                //
                key = clickedButton.Name;
            }

            // Button Tag => tag
            //
            tag = clickedButton.Tag?.ToString() ?? "";
            //txtDebug.Text = $"key = {key}\r\ntag = {tag}";


            //
            //----------   Сбрасываем текст для всех текстовых полей (т.е. показываем начальный экран)   -------------
            //
            if (key.Equals("btnReset") || key.Equals("btnCancel"))
            {
                //  Производим сброс всех учавствующих в экранных операциях полей
                ResetStates();

                // Переходим на начальный экран
                NextScreenInfo(0);
                return;
            }

            // Вспомогательная функция - переход между экранами в зависимости от текущего экрана
            //
            SwitchScreens(number, key, tag);
        }

        #endregion Button_Click()

        //
        //-----------    Обработчик команды HELP   ---------------
        //
        #region Обработчик команды Help

        /// <summary>
        /// Метод, который будет выполняться при вызове команды Help
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowCommandBinding_Help(object sender, ExecutedRoutedEventArgs e)
        {
            bool helpShown = false;
            foreach (Window window in this.OwnedWindows)
            {
                if (window is HelpWindow)
                {
                    window.Show();
                    window.Focus();
                    helpShown = true;
                }
            }
            if (!helpShown)
            {
                HelpWindow helpWindow = new HelpWindow();
                helpWindow.Owner = this;
                helpWindow.Show();
            }
        }

        #endregion Обработчик команды Help

        //
        //-----------    Вспомогательные функции  ---------------
        //

        #region NextScreenInfo()

        /// <summary>
        /// Переходим на указанный экран
        /// </summary>
        /// <param name="nextScreen">Требуемый экран, к которому хотим перейти</param>
        /// <returns></returns>
        bool NextScreenInfo(EnScreens screen)
        {
            if (screen == EnScreens.OutputResult)
            {
                //
                //----------   Экран выдачи денег     ------
                //

                // Запоминаем индекс требуемого к переходу экрана и запускаем анимацию
                //
                nextScreen = screen;


                // Показываем анимацию
                //
                // imgLoading
                DoubleAnimation imgAnimation = new DoubleAnimation();
                imgAnimation.From = 0; //  imgLoading.Opacity;
                imgAnimation.To = 1;
                imgAnimation.Duration = TimeSpan.FromSeconds(1);
                imgAnimation.AutoReverse = true;
                imgAnimation.Completed += OutputAnimation_Completed;

                txtScreenTop.Visibility = Visibility.Collapsed;

                // Выставляем флаг отработки транзакции - чтобы она отработала только 1 раз
                //
                IsAnimationCompleted = false;

                imgLoading.Visibility = Visibility.Visible;
                imgLoading.BeginAnimation(Image.OpacityProperty, imgAnimation);

                //// Дополнительно показываем анимацию в блоке состояния банкомата
                ////
                //imgLoading2.Visibility = Visibility.Visible;
                //imgLoading2.BeginAnimation(Image.OpacityProperty, imgAnimation);
            }
            else if (screen == EnScreens.InputResult)
            {
                //
                //----------   Экран внесения денег     ------
                //

                // Запоминаем индекс требуемого к переходу экрана и запускаем анимацию
                //
                nextScreen = screen;


                // Показываем анимацию
                //
                // imgLoading
                DoubleAnimation imgAnimation = new DoubleAnimation();
                imgAnimation.From = 0; //  imgLoading.Opacity;
                imgAnimation.To = 1;
                imgAnimation.Duration = TimeSpan.FromSeconds(1);
                imgAnimation.AutoReverse = true;
                imgAnimation.Completed += InputAnimation_Completed;

                txtScreenTop.Visibility = Visibility.Collapsed;

                // Выставляем флаг отработки транзакции - чтобы она отработала только 1 раз
                //
                IsAnimationCompleted = false;

                imgLoading.Visibility = Visibility.Visible;
                imgLoading.BeginAnimation(Image.OpacityProperty, imgAnimation);

                //// Дополнительно показываем анимацию в блоке состояния банкомата
                ////
                //imgLoading2.Visibility = Visibility.Visible;
                //imgLoading2.BeginAnimation(Image.OpacityProperty, imgAnimation);
            }
            else
            {
                // Переходим на указанный экран
                //
                curScreen = screen;
                CurrentScreenInfo = listScreenInfo.FirstOrDefault(s => s.CurrentEnScreen == curScreen);
            }
            return true;

        }

        #endregion NextScreenInfo()



        #region OutputAnimation_Completed()

        private void OutputAnimation_Completed(object sender, EventArgs e)
        {
            // MessageBox.Show("Анимация завершена");

            // Выставляем флаг отработки транзакции - чтобы она отработала только 1 раз
            //
            if (IsAnimationCompleted)
                return;
            IsAnimationCompleted = true;

            imgLoading.Visibility = Visibility.Collapsed;
            txtScreenTop.Visibility = Visibility.Visible;

            // imgLoading2.Visibility = Visibility.Collapsed;


            // пробуем выдать запрошенную сумму запрошенными купюрами
            //
            string status = OutputTransaction();
            if (status.Length > 0)
            {
                txtErrorInfo.Text = status; // "Банкомат не имеет запрошенных купюр.";
            }
            else
            {
                txtErrorInfo.Text = "";
            }

            // Переходим на указанный экран
            //
            curScreen = nextScreen;
            CurrentScreenInfo = listScreenInfo.FirstOrDefault(s => s.CurrentEnScreen == curScreen);
        }

        #endregion OutputAnimation_Completed()

        #region OutputTransaction()

        /// <summary>
        /// Пробуем выдать запрошенную сумму запрошенными купюрами
        /// </summary>
        /// <returns>Удобочитаемый статус запрошенной операции</returns>
        string OutputTransaction()
        {
            string result = "";

            // Проверяем - Можно ли выдать сумму полностью указанными купюрами
            //
            int quantity = requestedAmount / requestedBanknote;

            int amount = 0;
            int amountInATM = 0;
            foreach (Banknote b in listATM)
            {
                int nominal = 0;
                if (int.TryParse(b.Title, out nominal))
                {
                    if (nominal == requestedBanknote)
                    {
                        amountInATM = b.CurAmount;
                        amount = amountInATM - quantity;
                        break;
                    }
                }
            }
            if (amount <= 0)
            {
                // Если необходимых купюр недостаточно - то выдаем все купюры этого типа
                //
                quantity = amountInATM;
            }

            // Устанавливаем ту сумму, которую можем реально выдать держателю карты
            //
            outputAmount = requestedBanknote * quantity;

            // Списываем эту сумму со счета держателя карты
            //
            CurrentCardholder.Balans -= outputAmount;

            // Удаляем выданные купюры из банкомата
            //
            foreach (Banknote b in listATM)
            {
                int nominal = 0;
                if (int.TryParse(b.Title, out nominal))
                {
                    if (nominal == requestedBanknote)
                    {
                        b.CurAmount -= quantity;

                        // Запоминаем  для целей вывода информации о проведенной транзакции
                        amount = b.CurAmount;
                        break;
                    }
                }
            }
            result = $"Выдано: {quantity} купюр(-а) по {requestedBanknote} руб.";

            // Выводим информацию о 3-х последних успешных транзакциях в соответствующий список
            //

            // listLastTransInfo.Clear();
            if (listLastTransInfo.Count > 2)
                listLastTransInfo.RemoveAt(0);

            LastTransInfo li = new LastTransInfo { Title = requestedBanknote, BeforeAmount = amountInATM, Amount = -quantity, AfterAmount = amount };
            listLastTransInfo.Add(li);

            return result;
        }

        #endregion OutputTransaction()


        #region InputAnimation_Completed()

        private void InputAnimation_Completed(object sender, EventArgs e)
        {
            // MessageBox.Show("Анимация завершена");

            // Выставляем флаг отработки транзакции - чтобы она отработала только 1 раз
            //
            if (IsAnimationCompleted)
                return;
            IsAnimationCompleted = true;

            imgLoading.Visibility = Visibility.Collapsed;
            txtScreenTop.Visibility = Visibility.Visible;

            // imgLoading2.Visibility = Visibility.Collapsed;


            // пробуем внести указанное кол-во указанных купюр
            //
            string status = InputTransaction();
            if (status.Length > 0)
            {
                txtErrorInfo.Text = status; // "Банкомат не имеет возможности принять все указанные купюры.";
            }
            else
            {
                txtErrorInfo.Text = "";
            }

            // Переходим на указанный экран
            //
            curScreen = nextScreen;
            CurrentScreenInfo = listScreenInfo.FirstOrDefault(s => s.CurrentEnScreen == curScreen);
        }

        #endregion InputAnimation_Completed()

        #region InputTransaction()

        /// <summary>
        /// Пробуем внести указанное кол-во указанных купюр
        /// </summary>
        /// <returns>Удобочитаемый статус запрошенной операции</returns>
        string InputTransaction()
        {
            string result = "";

            // Проверяем - Можно ли принять все купюры полностью
            //
            int quantity = requestedAmount;

            int amount = 0;
            int amountInATM = 0;
            int maxAmountATM = 0;

            foreach (Banknote b in listATM)
            {
                int nominal = 0;
                if (int.TryParse(b.Title, out nominal))
                {
                    if (nominal == requestedBanknote)
                    {
                        amountInATM = b.CurAmount;
                        maxAmountATM = b.MaxAmount;

                        amount = maxAmountATM - amountInATM;
                        break;
                    }
                }
            }
            if (amount < quantity)
            {
                // Если банкомат почти заполнен - то вносим разницу до заполнения купюрами этого типа
                //
                quantity = amount;
            }

            // Устанавливаем ту сумму, которую пользователь может реально внести в банкомат
            //
            outputAmount = requestedBanknote * quantity;

            // Списываем эту сумму со счета держателя карты
            //
            CurrentCardholder.Balans += outputAmount;

            // Удаляем выданные купюры из банкомата
            //
            foreach (Banknote b in listATM)
            {
                int nominal = 0;
                if (int.TryParse(b.Title, out nominal))
                {
                    if (nominal == requestedBanknote)
                    {
                        b.CurAmount += quantity;

                        // Запоминаем  для целей вывода информации о проведенной транзакции
                        amount = b.CurAmount;
                        break;
                    }
                }
            }
            result = $"Внесено: {quantity} купюр(-а) по {requestedBanknote} руб.";


            // Выводим информацию о 3-х последних успешных транзакциях в соответствующий список
            //

            // listLastTransInfo.Clear();
            if (listLastTransInfo.Count > 2)
                listLastTransInfo.RemoveAt(0);

            LastTransInfo li = new LastTransInfo { Title = requestedBanknote, BeforeAmount = amountInATM, Amount = quantity, AfterAmount = amount };
            listLastTransInfo.Add(li);

            return result;
        }

        #endregion InputTransaction()



        #region ResetStates()

        /// <summary>
        /// Вспомогательная функция - производит сброс всех учавствующих в экранных операциях полей
        /// </summary>
        private void ResetStates()
        {
            requestedBanknote = 0;
            requestedAmount = 0;
            txtErrorInfo.Text = "";

            curPinCodeLength = 0;
            txtScreenPass.Text = "";

            // Делаем сброс выбранного Cardholder
            curCardholderIndex = -1;
            CurrentCardholder = null;
        }

        #endregion ResetStates()

        #region ReturnToFirstScreenAnimation_Completed()

        private void ReturnToFirstScreenAnimation_Completed(object sender, EventArgs e)
        {
            // MessageBox.Show("Анимация завершена");

            imgLoading.Visibility = Visibility.Collapsed;
            txtScreenTop.Visibility = Visibility.Visible;

            // Переходим на указанный экран
            //
            curScreen = nextScreen;
            CurrentScreenInfo = listScreenInfo.FirstOrDefault(s => s.CurrentEnScreen == curScreen);
        }

        #endregion ReturnToFirstScreenAnimation_Completed()


        #region SwitchScreens()

        /// <summary>
        /// Вспомогательная функция - переход между экранами в зависимости от текущего экрана
        /// </summary>
        /// <param name="number">Цифровой код нажатой кнопки (если это цифра)</param>
        /// <param name="key">Имя нажатой кнопки (если это не цифра)</param>
        /// <param name="tag">Tag нажатой кнопки</param>
        public void SwitchScreens(int number, string key, string tag)
        {
            // Нажата кнопка "Вернуть карту" =>
            //
            if (curScreen != EnScreens.Initial && tag.ToLower().Contains("вернуть"))
            {
                //  Производим сброс всех учавствующих в экранных операциях полей
                ResetStates();

                //  Переходим на начальный экран
                // NextScreenInfo(0);

                // Запоминаем индекс требуемого к переходу экрана и запускаем анимацию
                //
                nextScreen = EnScreens.Initial;

                // Показываем анимацию
                //
                // imgLoading
                DoubleAnimation imgAnimation = new DoubleAnimation();
                imgAnimation.From = 0; //  imgLoading.Opacity;
                imgAnimation.To = 1;
                imgAnimation.Duration = TimeSpan.FromSeconds(1);
                imgAnimation.AutoReverse = true;
                imgAnimation.Completed += ReturnToFirstScreenAnimation_Completed;

                txtScreenTop.Visibility = Visibility.Collapsed;

                imgLoading.Visibility = Visibility.Visible;
                imgLoading.BeginAnimation(Image.OpacityProperty, imgAnimation);
            }

            switch (curScreen)
            {
                //
                //----------- Начальный экран для выбора карты и нажата одна из правых экранных кнопок  -----
                //
                case EnScreens.Initial:
                    if (number < 0)
                    {
                        bool isPlaceholderFound = false;

                        // Запоминаем индекс текущего владельца карты
                        //
                        if (key.StartsWith("btnScreen") && tag.Length > 0)
                        {
                            for (int k = 0; k < listCardholders.Count; k++)
                            {
                                string name = listCardholders[k].Name;
                                if (name.Equals(tag, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    curCardholderIndex = k;
                                    isPlaceholderFound = true;
                                    break;
                                }
                            }
                        }

                        // Переходим на экран для ввода ПИН-кода
                        //
                        if (isPlaceholderFound)
                        {
                            NextScreenInfo(EnScreens.Password);

                            //// Запоминаем индекс требуемого к переходу экрана и запускаем анимацию
                            ////
                            //nextScreenIndex = 1;

                            //// Показываем анимацию
                            ////
                            //// imgLoading
                            //DoubleAnimation imgAnimation = new DoubleAnimation();
                            //imgAnimation.From = 0; //  imgLoading.Opacity;
                            //imgAnimation.To = 1;
                            //imgAnimation.Duration = TimeSpan.FromSeconds(1);
                            //imgAnimation.AutoReverse = true;
                            //imgAnimation.Completed += ReturnToFirstScreenAnimation_Completed;

                            //txtScreenTop.Visibility = Visibility.Collapsed;

                            //imgLoading.Visibility = Visibility.Visible;
                            //imgLoading.BeginAnimation(Image.OpacityProperty, imgAnimation);
                        }
                    }
                    break;

                //
                //-----------    Экран для ввода ПИН-кода + нажата одна из цифр   -----------------
                //
                case EnScreens.Password:
                    if (number >= 0 && !key.Equals("btnEnter"))
                    {
                        // Если введено 4 цифры - то считаем, что ПИН-код принят, запоминаем держателя карты и переходим на следующий экран
                        //
                        if (++curPinCodeLength < 4)
                        {
                            txtScreenPass.Text += "*";
                        }
                        else if (curPinCodeLength >= 3)
                        {
                            curPinCodeLength = 0;
                            txtScreenPass.Text = "";

                            // Запоминаем держателя карты
                            //
                            CurrentCardholder = listCardholders[curCardholderIndex];

                            // Переходим на экран выбора действия (снятие или внесение наличных)
                            //
                            NextScreenInfo(EnScreens.Actions);

                            //// Запоминаем индекс требуемого к переходу экрана и запускаем анимацию
                            ////
                            //nextScreenIndex = 2;

                            //// Показываем анимацию
                            ////
                            //// imgLoading
                            //DoubleAnimation imgAnimation = new DoubleAnimation();
                            //imgAnimation.From = 0; //  imgLoading.Opacity;
                            //imgAnimation.To = 1;
                            //imgAnimation.Duration = TimeSpan.FromSeconds(1);
                            //imgAnimation.AutoReverse = true;
                            //imgAnimation.Completed += ReturnToFirstScreenAnimation_Completed;

                            //txtScreenTop.Visibility = Visibility.Collapsed;

                            //imgLoading.Visibility = Visibility.Visible;
                            //imgLoading.BeginAnimation(Image.OpacityProperty, imgAnimation);
                        }
                    }
                    break;

                //
                //------ Экран для выбора действия (снятие или внесение наличных) и нажата одна из кнопок: btnScreen5 или btnScreen6  -------
                //
                case EnScreens.Actions:
                    if (key.StartsWith("btnScreen") && tag.Length > 0)
                    {
                        if (tag.ToLower().Contains("внесен"))
                        {
                            // Переходим на экран внесения наличных
                            NextScreenInfo(EnScreens.InputBanknote);
                        }
                        else if (tag.ToLower().Contains("выдач"))
                        {
                            // Переходим на экран снятия наличных
                            NextScreenInfo(EnScreens.OutputSum);
                        }
                    }
                    break;

                //
                //------    Экран для выбора суммы снятия наличных + нажата одна из экранных кнопок: btnScreen1 - btnScreen8     ------
                //
                case EnScreens.OutputSum:
                    if (key.StartsWith("btnScreen"))
                    {
                        // Запоминаем запрашиваемую к снятию сумму
                        //
                        if (key.StartsWith("btnScreen") && tag.Length > 0)
                        {
                            int amount = 0;
                            if (int.TryParse(tag, out amount))
                            {
                                requestedAmount = amount;
                            }
                            else
                            {
                                requestedAmount = 0;
                            }
                        }

                        // Проверяем, что наличие средств на карте клиента - позволяет выполнить текущую операцию
                        //
                        if (requestedAmount > CurrentCardholder.Balans)
                        {
                            txtErrorInfo.Text = "Ваш текущий баланс не позволяет выполнить запрошенную операцию.";

                            // Возврат к текущему экрану
                            return;
                        }
                        else if (!CheckAtmOutputTotal())
                        {
                            // есть ли у банкомата возможность выдать указанную сумму
                            //
                            txtErrorInfo.Text = "Банкомат не имеет возможности выдать указанную сумму.";

                            // Возврат к текущему экрану
                            return;
                        }
                        else
                        {
                            txtErrorInfo.Text = "";

                            // Переходим на экран выбора типа выдаваемых купюр
                            NextScreenInfo(EnScreens.OutputBanknote);
                            return;
                        }
                    }
                    break;

                //
                //-------  Экран выбора типа выдаваемых купюр + нажата одна из экранных кнопок: btnScreen1 - btnScreen8     ------
                //
                case EnScreens.OutputBanknote:
                    if (tag.ToLower().Contains("назад"))
                    {
                        // Нажата кнопка "Назад" => Переходим на предыдущий экран (Экран выбора выдаваемой суммы)
                        //
                        requestedAmount = 0;
                        requestedBanknote = 0;

                        txtErrorInfo.Text = "";

                        NextScreenInfo(EnScreens.OutputSum);
                    }
                    else if (key.StartsWith("btnScreen") && tag.Length > 0)
                    {
                        // Запоминаем  запрошенный к выдаче текущим пользователем тип купюр - requestedBanknote
                        //
                        int amount = 0;
                        if (int.TryParse(tag, out amount))
                        {
                            requestedBanknote = amount;
                        }
                        else
                        {
                            requestedBanknote = 0;
                        }


                        // Проверяем, что запрошенная сумма больше запрошенного к выдаче типа купюр
                        //
                        if (requestedAmount < requestedBanknote)
                        {
                            txtErrorInfo.Text = "Запрошенная сумма не может быть выдана указанными купюрами.";

                            // Возврат к текущему экрану
                            return;
                        }
                        else if (!CheckAtmOutputStatus())
                        {
                            // есть ли у банкомата возможность выдать запрошенные купюры
                            //
                            txtErrorInfo.Text = "Банкомат не имеет запрошенных купюр.";

                            // Возврат к текущему экрану
                            return;
                        }
                        else
                        {
                            txtErrorInfo.Text = "";

                            // Переходим на экран выдачи денег
                            NextScreenInfo(EnScreens.OutputResult);
                            return;
                        }
                    }
                    break;


                //
                //-------  Экран выдачи наличных + нажата одна из экранных кнопок: btnScreen1 - btnScreen8     ------
                //
                case EnScreens.OutputResult:
                    if (tag.ToLower().Contains("другие"))
                    {
                        // Нажата кнопка "Другие операции" => Переходим на экран выбора доступных для пользователя операций
                        //
                        requestedBanknote = 0;
                        requestedAmount = 0;
                        txtErrorInfo.Text = "";

                        NextScreenInfo(EnScreens.Actions);
                    }
                    break;


                //
                //-------  Экран выбора типа вносимых купюр + нажата одна из экранных кнопок: btnScreen1 - btnScreen8     ------
                //
                case EnScreens.InputBanknote:
                    if (key.StartsWith("btnScreen") && tag.Length > 0)
                    {
                        // Запоминаем  запрошенный к внесению текущим пользователем тип купюр - requestedBanknote
                        //
                        int amount = 0;
                        if (int.TryParse(tag, out amount))
                        {
                            requestedBanknote = amount;
                        }
                        else
                        {
                            requestedBanknote = 0;
                        }


                        if (!CheckAtmInputStatus())
                        {
                            // есть ли у банкомата возможность принять предлагаемые купюры
                            //
                            txtErrorInfo.Text = "Банкомат не имеет возможности принять такие купюры.";

                            // Возврат к текущему экрану
                            return;
                        }
                        else
                        {
                            txtErrorInfo.Text = "";

                            // Переходим на экран выбора кол-ва вносимих купюр
                            NextScreenInfo(EnScreens.InputQuantity);
                            return;
                        }
                    }
                    break;

                //
                //------    Экран для выбора количества вносимых купюр + нажата одна из экранных кнопок: btnScreen1 - btnScreen8     ------
                //
                case EnScreens.InputQuantity:
                    if (tag.ToLower().Contains("назад"))
                    {
                        // Нажата кнопка "Назад" => Переходим на предыдущий экран (Экран выбора типа вносимых купюр)
                        //
                        requestedAmount = 0;
                        requestedBanknote = 0;

                        txtErrorInfo.Text = "";

                        NextScreenInfo(EnScreens.InputBanknote);
                    }
                    else if (key.StartsWith("btnScreen") && tag.Length > 0)
                    {
                        // Запоминаем кол-во вносимых купюр
                        //
                        int amount = 0;
                        if (int.TryParse(tag, out amount))
                        {
                            requestedAmount = amount;
                        }
                        else
                        {
                            requestedAmount = 0;
                        }


                        if (!CheckAtmInputTotal())
                        {
                            // есть ли у банкомата возможность принять указанное количество купюр
                            //
                            txtErrorInfo.Text = "Банкомат не имеет возможности принять указанное кол-во купюр.";

                            // Возврат к текущему экрану
                            return;
                        }
                        else
                        {
                            txtErrorInfo.Text = "";

                            // Переходим на экран внесения наличных
                            NextScreenInfo(EnScreens.InputResult);
                            return;
                        }
                    }
                    break;


                //
                //-------  Экран приема наличных + нажата одна из экранных кнопок: btnScreen1 - btnScreen8     ------
                //
                case EnScreens.InputResult:
                    if (tag.ToLower().Contains("другие"))
                    {
                        // Нажата кнопка "Другие операции" => Переходим на экран выбора доступных для пользователя операций
                        //
                        requestedBanknote = 0;
                        requestedAmount = 0;
                        txtErrorInfo.Text = "";

                        NextScreenInfo(EnScreens.Actions);
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion SwitchScreens()

        #region CheckAtmOutputTotal()

        /// <summary>
        /// Проверяем - что сумма в банкомате больше запрошенной
        /// </summary>
        /// <returns></returns>
        bool CheckAtmOutputTotal()
        {
            // Запрошенная сумма - requestedAmount
            // Запрошенные купюры - requestedBanknote

            if (requestedAmount == 0)
                return false;

            // Проверяем, что в банкомате хватит денег на выдачу нужной суммы
            //
            int totalCash = 0;
            foreach (Banknote b in listATM)
            {
                int i1 = 0;
                if (int.TryParse(b.Title, out i1))
                {
                    totalCash += i1 * b.CurAmount;
                }
            }
            if (totalCash < requestedAmount)
                return false;

            return true;
        }

        #endregion CheckAtmOutputTotal()

        #region CheckAtmOutputStatus()

        /// <summary>
        /// Проверяем - есть ли у банкомата возможность выдать определенные купюры
        /// </summary>
        /// <returns></returns>
        bool CheckAtmOutputStatus()
        {
            // Запрошенная сумма - requestedAmount
            // Запрошенные купюры - requestedBanknote

            if (requestedAmount == 0 || requestedBanknote == 0)
                return false;

            // Проверяем, что в банкомате хватит денег на выдачу нужной суммы
            //
            int amount = 0;
            foreach (Banknote b in listATM)
            {
                int nominal = 0;
                if (int.TryParse(b.Title, out nominal))
                {
                    if (nominal == requestedBanknote)
                    {
                        amount = b.CurAmount;
                        break;
                    }
                }
            }
            if (amount == 0)
                return false;

            return true;
        }

        #endregion CheckAtmOutputStatus()

        #region CheckAtmInputStatus()

        /// <summary>
        /// Проверяем - есть ли у банкомата возможность принять определенные купюры
        /// </summary>
        /// <returns></returns>
        bool CheckAtmInputStatus()
        {
            // Предлагаемые купюры - requestedBanknote
            // Вносимое кол-во таких купюр - requestedAmount

            //if (requestedAmount == 0 || requestedBanknote == 0)
            //    return false;

            if (requestedBanknote == 0)
                return false;

            // Проверяем, что в банкомате есть место под указанные банкноты
            //
            bool isFull = false;
            foreach (Banknote b in listATM)
            {
                int nominal = 0;
                if (int.TryParse(b.Title, out nominal))
                {
                    if (nominal == requestedBanknote)
                    {
                        // Пять купюр - минимальное кол-во вносимых купюр (через пользовательский интерфейс)
                        //
                        isFull = (b.CurAmount + 5) > b.MaxAmount;
                        break;
                    }
                }
            }
            if (isFull)
                return false;

            return true;
        }

        #endregion CheckAtmInputStatus()

        #region CheckAtmInputTotal()

        /// <summary>
        /// Проверяем - есть ли у банкомата возможность принять указанное кол-во указанных купюр
        /// </summary>
        /// <returns></returns>
        bool CheckAtmInputTotal()
        {
            // Предлагаемые купюры - requestedBanknote
            // Вносимое кол-во таких купюр - requestedAmount

            if (requestedAmount == 0 || requestedBanknote == 0)
                return false;

            // Проверяем, что в банкомате есть место под указанное кол-во указанных  банкнот
            //
            bool isFull = false;
            foreach (Banknote b in listATM)
            {
                int nominal = 0;
                if (int.TryParse(b.Title, out nominal))
                {
                    if (nominal == requestedBanknote)
                    {
                        isFull = b.CurAmount + requestedAmount > b.MaxAmount;
                        break;
                    }
                }
            }
            if (isFull)
                return false;

            return true;
        }

        #endregion CheckAtmInputTotal()



    }
}
