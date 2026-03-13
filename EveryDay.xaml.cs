using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace krasotkaDBLevonTEST
{
    public partial class EveryDay : Window
    {
        private const string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";

        public ObservableCollection<Master> MastersList { get; set; }
        public ObservableCollection<Service> HairServicesList { get; set; }
        public ObservableCollection<Service> ManicureServicesList { get; set; }
        public ObservableCollection<Service> PedicureServicesList { get; set; }
        public ObservableCollection<Service> MassageServicesList { get; set; }
        public ObservableCollection<Service> CosmeticServicesList { get; set; }

        private int selectedMasterCode = 0;
        private int selectedClientCode = 0;
        private int selectedServiceCode = 0;

        public EveryDay()
        {
            InitializeComponent();

            MastersList = new ObservableCollection<Master>();
            HairServicesList = new ObservableCollection<Service>();
            ManicureServicesList = new ObservableCollection<Service>();
            PedicureServicesList = new ObservableCollection<Service>();
            MassageServicesList = new ObservableCollection<Service>();
            CosmeticServicesList = new ObservableCollection<Service>();

            mastersListBox.ItemsSource = MastersList;
            hairServicesListBox.ItemsSource = HairServicesList;
            manicureServicesListBox.ItemsSource = ManicureServicesList;
            pedicureServicesListBox.ItemsSource = PedicureServicesList;
            massageServicesListBox.ItemsSource = MassageServicesList;
            cosmeticServicesListBox.ItemsSource = CosmeticServicesList;

            appointmentDatePicker.SelectedDate = DateTime.Today;

            LoadData();
        }

        private void LoadData()
        {
            LoadMasters();
            LoadServices();
        }

        private void LoadMasters()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT m.masterCode, m.masterName, m.masterTel, m.servTypeCode, s.servType
                                   FROM masters m
                                   LEFT JOIN servTypes s ON m.servTypeCode = s.servTypeCode
                                   WHERE m.masterActivity = 'да'
                                   ORDER BY m.masterName";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    MastersList.Clear();

                    while (reader.Read())
                    {
                        MastersList.Add(new Master
                        {
                            MasterCode = reader.GetInt32("masterCode"),
                            MasterName = reader.GetString("masterName"),
                            MasterTel = reader.GetString("masterTel"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServTypeName = reader.IsDBNull(4) ? "Не указан" : reader.GetString("servType")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки мастеров: {ex.Message}");
            }
        }

        private void LoadServices()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT s.servCode, s.servName, s.servPrice, s.servDuration, 
                                           s.servTypeCode, st.servType
                                   FROM services s
                                   LEFT JOIN servTypes st ON s.servTypeCode = st.servTypeCode
                                   WHERE s.servActivity = 'да'
                                   ORDER BY st.servType, s.servName";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    HairServicesList.Clear();
                    ManicureServicesList.Clear();
                    PedicureServicesList.Clear();
                    MassageServicesList.Clear();
                    CosmeticServicesList.Clear();

                    while (reader.Read())
                    {
                        var service = new Service
                        {
                            ServiceCode = reader.GetInt32("servCode"),
                            ServiceName = reader.GetString("servName"),
                            ServicePrice = reader.GetInt32("servPrice"),
                            ServiceDuration = reader.GetInt32("servDuration"),
                            ServiceTypeCode = reader.GetInt32("servTypeCode"),
                            ServiceTypeName = reader.IsDBNull(5) ? "Не указан" : reader.GetString("servType")
                        };

                        // Распределяем услуги по соответствующим спискам
                        switch (service.ServiceTypeCode)
                        {
                            case 1: // Парикмахерские
                                HairServicesList.Add(service);
                                break;
                            case 2: // Маникюр
                                ManicureServicesList.Add(service);
                                break;
                            case 3: // Косметические
                                CosmeticServicesList.Add(service);
                                break;
                            case 4: // Массаж
                                MassageServicesList.Add(service);
                                break;
                            case 5: // Педикюр
                                PedicureServicesList.Add(service);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки услуг: {ex.Message}");
            }
        }

        private void FindClientByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                ShowWarning("Введите телефон клиента для поиска");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT clientCode, clientName, clientTel 
                                   FROM clients 
                                   WHERE clientTel LIKE @phone AND clientActivity = 'да'
                                   LIMIT 1";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@phone", $"%{phone.Trim()}%");

                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        selectedClientCode = reader.GetInt32("clientCode");
                        string clientName = reader.GetString("clientName");
                        string clientTel = reader.GetString("clientTel");

                        clientInfoTextBlock.Text = $"Найден клиент:\n{clientName}\nТел: {clientTel}";
                        tempClientPhoneTextBox.Text = clientTel;
                    }
                    else
                    {
                        selectedClientCode = 0;
                        clientInfoTextBlock.Text = "Клиент не найден";
                        ShowInfo("Клиент с указанным телефоном не найден. Используйте анонимного клиента или проверьте телефон.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка поиска клиента: {ex.Message}");
            }
        }

        private void UseAnonymousClient()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT clientCode, clientName, clientTel FROM clients WHERE clientName = 'Анонимный клиент'";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        selectedClientCode = reader.GetInt32("clientCode");
                        string clientName = reader.GetString("clientName");
                        string clientTel = reader.GetString("clientTel");

                        clientInfoTextBlock.Text = $"Выбран: {clientName}";
                        clientPhoneTextBox.Text = clientTel;
                        tempClientPhoneTextBox.Text = clientTel;

                        ShowInfo("Выбран анонимный клиент");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка выбора анонимного клиента: {ex.Message}");
            }
        }

        private void CreateAppointment()
        {
            if (selectedMasterCode == 0)
            {
                ShowWarning("Выберите мастера");
                return;
            }

            if (selectedClientCode == 0)
            {
                ShowWarning("Выберите или найдите клиента");
                return;
            }

            if (selectedServiceCode == 0)
            {
                ShowWarning("Выберите услугу");
                return;
            }

            if (appointmentDatePicker.SelectedDate == null)
            {
                ShowWarning("Выберите дату записи");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Получаем следующий код записи
                    int newAppCode = GetNextAppointmentCode(connection);

                    // Получаем servTypeCode для выбранной услуги
                    int servTypeCode = GetServiceTypeCode(connection, selectedServiceCode);

                    // Определяем время записи (queueFrom и queueTo)
                    // В реальном приложении здесь должна быть логика расчета времени
                    int queueFrom = 1;
                    int queueTo = 2;

                    string query = @"INSERT INTO appointments (appCode, masterCode, clientCode, servTypeCode, servCode, queueFrom, queueTo, appDate)
                                   VALUES (@appCode, @masterCode, @clientCode, @servTypeCode, @servCode, @queueFrom, @queueTo, @appDate)";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@appCode", newAppCode);
                    command.Parameters.AddWithValue("@masterCode", selectedMasterCode);
                    command.Parameters.AddWithValue("@clientCode", selectedClientCode);
                    command.Parameters.AddWithValue("@servTypeCode", servTypeCode);
                    command.Parameters.AddWithValue("@servCode", selectedServiceCode);
                    command.Parameters.AddWithValue("@queueFrom", queueFrom);
                    command.Parameters.AddWithValue("@queueTo", queueTo);
                    command.Parameters.AddWithValue("@appDate", appointmentDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowInfo("Запись успешно создана!");
                        ClearSelection();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка создания записи: {ex.Message}");
            }
        }

        private int GetNextAppointmentCode(MySqlConnection connection)
        {
            string maxCodeQuery = "SELECT MAX(appCode) FROM appointments";
            MySqlCommand maxCommand = new MySqlCommand(maxCodeQuery, connection);
            var maxCode = maxCommand.ExecuteScalar();
            return maxCode == DBNull.Value ? 1 : Convert.ToInt32(maxCode) + 1;
        }

        private int GetServiceTypeCode(MySqlConnection connection, int serviceCode)
        {
            string query = "SELECT servTypeCode FROM services WHERE servCode = @serviceCode";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@serviceCode", serviceCode);
            var result = command.ExecuteScalar();
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private void ClearSelection()
        {
            mastersListBox.SelectedIndex = -1;
            hairServicesListBox.SelectedIndex = -1;
            manicureServicesListBox.SelectedIndex = -1;
            pedicureServicesListBox.SelectedIndex = -1;
            massageServicesListBox.SelectedIndex = -1;
            cosmeticServicesListBox.SelectedIndex = -1;
            clientPhoneTextBox.Clear();
            tempClientPhoneTextBox.Clear();
            clientInfoTextBlock.Text = "";
            selectedMasterCode = 0;
            selectedClientCode = 0;
            selectedServiceCode = 0;
        }

        // Обработчики событий
        private void MastersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mastersListBox.SelectedItem is Master selectedMaster)
            {
                selectedMasterCode = selectedMaster.MasterCode;
            }
        }

        private void ServiceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox?.SelectedItem is Service selectedService)
            {
                selectedServiceCode = selectedService.ServiceCode;

                // Снимаем выделение с других ListBox'ов
                if (listBox != hairServicesListBox) hairServicesListBox.SelectedIndex = -1;
                if (listBox != manicureServicesListBox) manicureServicesListBox.SelectedIndex = -1;
                if (listBox != pedicureServicesListBox) pedicureServicesListBox.SelectedIndex = -1;
                if (listBox != massageServicesListBox) massageServicesListBox.SelectedIndex = -1;
                if (listBox != cosmeticServicesListBox) cosmeticServicesListBox.SelectedIndex = -1;
            }
        }

        private void FindClientButton_Click(object sender, RoutedEventArgs e)
        {
            FindClientByPhone(clientPhoneTextBox.Text);
        }

        private void AnonymousClientButton_Click(object sender, RoutedEventArgs e)
        {
            UseAnonymousClient();
        }

        private void CreateAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            CreateAppointment();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            ShowInfo("Данные обновлены");
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Owner.Show();
            }
            Close();
        }

        // Вспомогательные методы для сообщений
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Owner != null && !Owner.IsVisible)
            {
                Owner.Show();
            }
        }
    }
}