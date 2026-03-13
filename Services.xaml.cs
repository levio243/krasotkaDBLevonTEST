using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace krasotkaDBLevonTEST
{
    public partial class Services : Window
    {
        private const string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";

        public ObservableCollection<Service> ServicesList { get; set; }
        public ObservableCollection<ServType> ServiceTypesList { get; set; }

        public Services()
        {
            InitializeComponent();
            ServicesList = new ObservableCollection<Service>();
            ServiceTypesList = new ObservableCollection<ServType>();
            servicesGrid.ItemsSource = ServicesList;
            serviceTypeComboBox.ItemsSource = ServiceTypesList;
            LoadServiceTypes();
            LoadActiveServices();
        }

        private void LoadActiveServices()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT s.servCode, s.servName, s.servPrice, s.servDuration,
                                    s.servTypeCode, st.servType, s.servActivity
                                    FROM services s
                                    LEFT JOIN servTypes st ON s.servTypeCode = st.servTypeCode
                                    WHERE s.servActivity = 'да'
                                    ORDER BY s.servCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    ServicesList.Clear();
                    while (reader.Read())
                    {
                        ServicesList.Add(new Service
                        {
                            ServiceCode = reader.GetInt32("servCode"),
                            ServiceName = reader.GetString("servName"),
                            ServicePrice = reader.GetInt32("servPrice"),
                            ServiceDuration = reader.GetInt32("servDuration"),
                            ServiceTypeCode = reader.GetInt32("servTypeCode"),
                            ServiceTypeName = reader.IsDBNull(5) ? "Не указан" : reader.GetString("servType"),
                            Activity = reader.GetString("servActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadAllServices()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT s.servCode, s.servName, s.servPrice, s.servDuration,
                                    s.servTypeCode, st.servType, s.servActivity
                                    FROM services s
                                    LEFT JOIN servTypes st ON s.servTypeCode = st.servTypeCode
                                    ORDER BY s.servCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    ServicesList.Clear();
                    while (reader.Read())
                    {
                        ServicesList.Add(new Service
                        {
                            ServiceCode = reader.GetInt32("servCode"),
                            ServiceName = reader.GetString("servName"),
                            ServicePrice = reader.GetInt32("servPrice"),
                            ServiceDuration = reader.GetInt32("servDuration"),
                            ServiceTypeCode = reader.GetInt32("servTypeCode"),
                            ServiceTypeName = reader.IsDBNull(5) ? "Не указан" : reader.GetString("servType"),
                            Activity = reader.GetString("servActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadServiceTypes()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT servTypeCode, servType FROM servTypes WHERE servTypeActivity = 'да' ORDER BY servType";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    ServiceTypesList.Clear();
                    while (reader.Read())
                    {
                        ServiceTypesList.Add(new ServType
                        {
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            TypeName = reader.GetString("servType")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки типов услуг: {ex.Message}");
            }
        }

        private void SearchServices()
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                ShowWarning("Введите название услуги для поиска");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT s.servCode, s.servName, s.servPrice, s.servDuration,
                                    s.servTypeCode, st.servType, s.servActivity
                                    FROM services s
                                    LEFT JOIN servTypes st ON s.servTypeCode = st.servTypeCode
                                    WHERE s.servName LIKE @search
                                    ORDER BY s.servCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@search", $"%{searchTextBox.Text.Trim()}%");

                    MySqlDataReader reader = command.ExecuteReader();

                    ServicesList.Clear();
                    bool found = false;

                    while (reader.Read())
                    {
                        found = true;
                        ServicesList.Add(new Service
                        {
                            ServiceCode = reader.GetInt32("servCode"),
                            ServiceName = reader.GetString("servName"),
                            ServicePrice = reader.GetInt32("servPrice"),
                            ServiceDuration = reader.GetInt32("servDuration"),
                            ServiceTypeCode = reader.GetInt32("servTypeCode"),
                            ServiceTypeName = reader.IsDBNull(5) ? "Не указан" : reader.GetString("servType"),
                            Activity = reader.GetString("servActivity")
                        });
                    }

                    if (!found)
                    {
                        ShowInfo("Услуги по вашему запросу не найдены");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка поиска: {ex.Message}");
            }
        }

        private void AddService()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                string.IsNullOrWhiteSpace(priceTextBox.Text) ||
                string.IsNullOrWhiteSpace(durationTextBox.Text) ||
                serviceTypeComboBox.SelectedItem == null)
            {
                ShowWarning("Заполните все обязательные поля: Название, Цена, Длительность и выберите тип услуги");
                return;
            }

            if (!int.TryParse(priceTextBox.Text, out int price) || price <= 0)
            {
                ShowWarning("Цена должна быть положительным числом");
                return;
            }

            if (!int.TryParse(durationTextBox.Text, out int duration) || duration <= 0)
            {
                ShowWarning("Длительность должна быть положительным числом");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    int newServiceCode = GetNextServiceCode(connection);
                    var selectedServiceType = (ServType)serviceTypeComboBox.SelectedItem;

                    string activity = "да";
                    if (activityComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        activity = selectedItem.Content.ToString();
                    }

                    string query = @"INSERT INTO services (servCode, servName, servPrice, servDuration, servTypeCode, servActivity)
                                    VALUES (@code, @name, @price, @duration, @servType, @activity)";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@code", newServiceCode);
                    command.Parameters.AddWithValue("@name", nameTextBox.Text.Trim());
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@duration", duration);
                    command.Parameters.AddWithValue("@servType", selectedServiceType.ServTypeCode);
                    command.Parameters.AddWithValue("@activity", activity);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowInfo("Услуга успешно добавлена");
                        ClearInputFields();
                        LoadActiveServices();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления услуги: {ex.Message}");
            }
        }

        private void EditService()
        {
            if (servicesGrid.SelectedItem == null)
            {
                ShowWarning("Выберите услугу для изменения");
                return;
            }

            var selectedService = (Service)servicesGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите изменить данные услуги {selectedService.ServiceName}?"))
            {
                if (!int.TryParse(priceTextBox.Text, out int price) || price <= 0)
                {
                    ShowWarning("Цена должна быть положительным числом");
                    return;
                }

                if (!int.TryParse(durationTextBox.Text, out int duration) || duration <= 0)
                {
                    ShowWarning("Длительность должна быть положительным числом");
                    return;
                }

                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = @"UPDATE services SET
                                        servName = @name,
                                        servPrice = @price,
                                        servDuration = @duration,
                                        servTypeCode = @servType,
                                        servActivity = @activity
                                        WHERE servCode = @code";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedService.ServiceCode);
                        command.Parameters.AddWithValue("@name",
                            string.IsNullOrWhiteSpace(nameTextBox.Text) ? selectedService.ServiceName : nameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@duration", duration);

                        int servTypeCode = serviceTypeComboBox.SelectedItem != null ?
                            ((ServType)serviceTypeComboBox.SelectedItem).ServTypeCode : selectedService.ServiceTypeCode;
                        command.Parameters.AddWithValue("@servType", servTypeCode);

                        string activity = "да";
                        if (activityComboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            activity = selectedItem.Content.ToString();
                        }
                        command.Parameters.AddWithValue("@activity", activity);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Данные услуги успешно обновлены");
                            ClearInputFields();
                            LoadActiveServices();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка изменения данных: {ex.Message}");
                }
            }
        }

        private void DeleteService()
        {
            if (servicesGrid.SelectedItem == null)
            {
                ShowWarning("Выберите услугу для удаления");
                return;
            }

            var selectedService = (Service)servicesGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите удалить услугу {selectedService.ServiceName}?\n\n" +
                                "Услуга будет помечена как неактивная и скрыта из списка."))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        // Вместо физического удаления меняем активность на "нет"
                        string query = "UPDATE services SET servActivity = 'нет' WHERE servCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedService.ServiceCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Услуга успешно удалена (деактивирована)");
                            ClearInputFields();
                            LoadActiveServices();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ActivateService()
        {
            if (servicesGrid.SelectedItem == null)
            {
                ShowWarning("Выберите услугу для активации");
                return;
            }

            var selectedService = (Service)servicesGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите активировать услугу {selectedService.ServiceName}?"))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE services SET servActivity = 'да' WHERE servCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedService.ServiceCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Услуга успешно активирована");
                            ClearInputFields();
                            LoadActiveServices();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка активации: {ex.Message}");
                }
            }
        }

        private int GetNextServiceCode(MySqlConnection connection)
        {
            string maxCodeQuery = "SELECT MAX(servCode) FROM services";
            MySqlCommand maxCommand = new MySqlCommand(maxCodeQuery, connection);
            var maxCode = maxCommand.ExecuteScalar();
            return maxCode == DBNull.Value ? 1 : Convert.ToInt32(maxCode) + 1;
        }

        private void ClearInputFields()
        {
            codeTextBox.Clear();
            nameTextBox.Clear();
            priceTextBox.Clear();
            durationTextBox.Clear();
            searchTextBox.Clear();
            serviceTypeComboBox.SelectedIndex = -1;
            activityComboBox.SelectedIndex = 0;
        }

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

        private bool ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        // Обработчики событий
        private void SearchButton_Click(object sender, RoutedEventArgs e) => SearchServices();
        private void AddButton_Click(object sender, RoutedEventArgs e) => AddService();
        private void EditButton_Click(object sender, RoutedEventArgs e) => EditService();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteService();
        private void ActivateButton_Click(object sender, RoutedEventArgs e) => ActivateService();

        private void ShowActiveButton_Click(object sender, RoutedEventArgs e) => LoadActiveServices();
        private void ShowAllButton_Click(object sender, RoutedEventArgs e) => LoadAllServices();

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActiveServices();
            LoadServiceTypes();
            ClearInputFields();
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

        private void ServicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (servicesGrid.SelectedItem is Service selectedService)
            {
                codeTextBox.Text = selectedService.ServiceCode.ToString();
                nameTextBox.Text = selectedService.ServiceName;
                priceTextBox.Text = selectedService.ServicePrice.ToString();
                durationTextBox.Text = selectedService.ServiceDuration.ToString();

                // Устанавливаем соответствующий тип услуги в ComboBox
                foreach (ServType serviceType in serviceTypeComboBox.Items)
                {
                    if (serviceType.ServTypeCode == selectedService.ServiceTypeCode)
                    {
                        serviceTypeComboBox.SelectedItem = serviceType;
                        break;
                    }
                }

                // Устанавливаем соответствующее значение активности в ComboBox
                foreach (ComboBoxItem item in activityComboBox.Items)
                {
                    if (item.Content.ToString() == selectedService.Activity)
                    {
                        activityComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
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