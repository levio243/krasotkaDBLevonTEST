using System.Windows;

namespace krasotkaDBLevonTEST
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenDatabaseManager()
        {
            OpenDB openDB = new OpenDB();
            openDB.Owner = this;
            openDB.Show();
            this.Hide();
        }

        //private void OpenAdminMode()
        //{
        //    EveryDay everyDay = new EveryDay();
        //    everyDay.Owner = this;
        //    everyDay.Show();
        //    this.Hide();
        // }

        private void ExitApplication()
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти из программы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void b_db_Click(object sender, RoutedEventArgs e)
        {
            OpenDatabaseManager();
        }

        private void b_work_Click(object sender, RoutedEventArgs e)
        {
            OpenAdminMode();
        }

        private void b_exit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void OpenAdminMode()
        {
            EveryDay everyDay = new EveryDay();
            everyDay.Owner = this;
            everyDay.Show();
            this.Hide();
        }
    }
}