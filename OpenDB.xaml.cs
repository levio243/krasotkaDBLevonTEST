using System.Windows;

namespace krasotkaDBLevonTEST
{
    public partial class OpenDB : Window
    {
        public OpenDB()
        {
            InitializeComponent();
        }

        private void OpenWindow(Window window)
        {
            window.Owner = this;
            window.Show();
            this.Hide();
        }

        private void ReturnToMainMenu()
        {
            if (Owner != null)
            {
                Owner.Show();
            }
            Close();
        }

        private void b_client_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new Clients());
        }

        private void b_typeService_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new ServTypes());
        }

        private void b_master_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new Masters());
        }

        private void b_service_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(new Services());
        }

        // private void b_appoint_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenWindow(new Appointments());
        //}

        private void b_returnO_Click(object sender, RoutedEventArgs e)
        {
            ReturnToMainMenu();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);

            if (Owner != null && !Owner.IsVisible)
            {
                Owner.Show();
            }
        }
    }
}