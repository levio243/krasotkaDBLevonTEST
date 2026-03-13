using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace krasotkaDBLevonTEST
{
    public partial class Masters : Window
    {
        private const string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";

        public ObservableCollection<Master> MastersList { get; set; }
        public ObservableCollection<ServType> ServiceTypesList { get; set; }

        public Masters()
        {
            InitializeComponent();
            MastersList = new ObservableCollection<Master>();
            ServiceTypesList = new ObservableCollection<ServType>();
            mastersGrid.ItemsSource = MastersList;
            serviceTypeComboBox.ItemsSource = ServiceTypesList;
            LoadServiceTypes();
            LoadActiveMasters();
        }

        private void LoadActiveMasters()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT m.masterCode, m.masterName, m.masterTel, m.servTypeCode, s.servType, m.masterActivity
                                    FROM masters m
                                    LEFT JOIN servTypes s ON m.servTypeCode = s.servTypeCode
                                    WHERE m.masterActivity = 'да'
                                    ORDER BY m.masterCode";

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
                            ServTypeName = reader.IsDBNull(4) ? "Не указан" : reader.GetString("servType"),
                            Activity = reader.GetString("masterActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadAllMasters()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT m.masterCode, m.masterName, m.masterTel, m.servTypeCode, s.servType, m.masterActivity
                                    FROM masters m
                                    LEFT JOIN servTypes s ON m.servTypeCode = s.servTypeCode
                                    ORDER BY m.masterCode";

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
                            ServTypeName = reader.IsDBNull(4) ? "Не указан" : reader.GetString("servType"),
                            Activity = reader.GetString("masterActivity")
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

        private void SearchMasters()
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                ShowWarning("Введите имя или телефон для поиска");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT m.masterCode, m.masterName, m.masterTel, m.servTypeCode, s.servType, m.masterActivity
                                    FROM masters m
                                    LEFT JOIN servTypes s ON m.servTypeCode = s.servTypeCode
                                    WHERE (m.masterName LIKE @search OR m.masterTel LIKE @search)
                                    ORDER BY m.masterCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@search", $"%{searchTextBox.Text.Trim()}%");

                    MySqlDataReader reader = command.ExecuteReader();

                    MastersList.Clear();
                    bool found = false;

                    while (reader.Read())
                    {
                        found = true;
                        MastersList.Add(new Master
                        {
                            MasterCode = reader.GetInt32("masterCode"),
                            MasterName = reader.GetString("masterName"),
                            MasterTel = reader.GetString("masterTel"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServTypeName = reader.IsDBNull(4) ? "Не указан" : reader.GetString("servType"),
                            Activity = reader.GetString("masterActivity")
                        });
                    }

                    if (!found)
                    {
                        ShowInfo("Мастера по вашему запросу не найдены");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка поиска: {ex.Message}");
            }
        }

        private void AddMaster()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                string.IsNullOrWhiteSpace(phoneTextBox.Text) ||
                serviceTypeComboBox.SelectedItem == null)
            {
                ShowWarning("Заполните все обязательные поля: Имя, Телефон и выберите тип услуги");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    int newMasterCode = GetNextMasterCode(connection);
                    var selectedServiceType = (ServType)serviceTypeComboBox.SelectedItem;

                    string activity = "да";
                    if (activityComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        activity = selectedItem.Content.ToString();
                    }

                    string query = @"INSERT INTO masters (masterCode, masterName, masterTel, servTypeCode, masterActivity)
                                    VALUES (@code, @name, @phone, @servType, @activity)";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@code", newMasterCode);
                    command.Parameters.AddWithValue("@name", nameTextBox.Text.Trim());
                    command.Parameters.AddWithValue("@phone", phoneTextBox.Text.Trim());
                    command.Parameters.AddWithValue("@servType", selectedServiceType.ServTypeCode);
                    command.Parameters.AddWithValue("@activity", activity);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowInfo("Мастер успешно добавлен");
                        ClearInputFields();
                        LoadActiveMasters();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления мастера: {ex.Message}");
            }
        }

        private void EditMaster()
        {
            if (mastersGrid.SelectedItem == null)
            {
                ShowWarning("Выберите мастера для изменения");
                return;
            }

            var selectedMaster = (Master)mastersGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите изменить данные мастера {selectedMaster.MasterName}?"))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = @"UPDATE masters SET
                                        masterName = @name,
                                        masterTel = @phone,
                                        servTypeCode = @servType,
                                        masterActivity = @activity
                                        WHERE masterCode = @code";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedMaster.MasterCode);
                        command.Parameters.AddWithValue("@name",
                            string.IsNullOrWhiteSpace(nameTextBox.Text) ? selectedMaster.MasterName : nameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@phone",
                            string.IsNullOrWhiteSpace(phoneTextBox.Text) ? selectedMaster.MasterTel : phoneTextBox.Text.Trim());

                        int servTypeCode = serviceTypeComboBox.SelectedItem != null ?
                            ((ServType)serviceTypeComboBox.SelectedItem).ServTypeCode : selectedMaster.ServTypeCode;
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
                            ShowInfo("Данные мастера успешно обновлены");
                            ClearInputFields();
                            LoadActiveMasters();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка изменения данных: {ex.Message}");
                }
            }
        }

        private void DeleteMaster()
        {
            if (mastersGrid.SelectedItem == null)
            {
                ShowWarning("Выберите мастера для удаления");
                return;
            }

            var selectedMaster = (Master)mastersGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите удалить мастера {selectedMaster.MasterName}?\n\n" +
                                "Мастер будет помечен как неактивный и скрыт из списка."))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        // Вместо физического удаления меняем активность на "нет"
                        string query = "UPDATE masters SET masterActivity = 'нет' WHERE masterCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedMaster.MasterCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Мастер успешно удален (деактивирован)");
                            ClearInputFields();
                            LoadActiveMasters();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ActivateMaster()
        {
            if (mastersGrid.SelectedItem == null)
            {
                ShowWarning("Выберите мастера для активации");
                return;
            }

            var selectedMaster = (Master)mastersGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите активировать мастера {selectedMaster.MasterName}?"))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE masters SET masterActivity = 'да' WHERE masterCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedMaster.MasterCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Мастер успешно активирован");
                            ClearInputFields();
                            LoadActiveMasters();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка активации: {ex.Message}");
                }
            }
        }

        private int GetNextMasterCode(MySqlConnection connection)
        {
            string maxCodeQuery = "SELECT MAX(masterCode) FROM masters";
            MySqlCommand maxCommand = new MySqlCommand(maxCodeQuery, connection);
            var maxCode = maxCommand.ExecuteScalar();
            return maxCode == DBNull.Value ? 1 : Convert.ToInt32(maxCode) + 1;
        }

        private void ClearInputFields()
        {
            codeTextBox.Clear();
            nameTextBox.Clear();
            phoneTextBox.Clear();
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
        private void SearchButton_Click(object sender, RoutedEventArgs e) => SearchMasters();
        private void AddButton_Click(object sender, RoutedEventArgs e) => AddMaster();
        private void EditButton_Click(object sender, RoutedEventArgs e) => EditMaster();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteMaster();
        private void ActivateButton_Click(object sender, RoutedEventArgs e) => ActivateMaster();

        private void ShowActiveButton_Click(object sender, RoutedEventArgs e) => LoadActiveMasters();
        private void ShowAllButton_Click(object sender, RoutedEventArgs e) => LoadAllMasters();

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActiveMasters();
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

        private void MastersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mastersGrid.SelectedItem is Master selectedMaster)
            {
                codeTextBox.Text = selectedMaster.MasterCode.ToString();
                nameTextBox.Text = selectedMaster.MasterName;
                phoneTextBox.Text = selectedMaster.MasterTel;

                // Устанавливаем соответствующий тип услуги в ComboBox
                foreach (ServType serviceType in serviceTypeComboBox.Items)
                {
                    if (serviceType.ServTypeCode == selectedMaster.ServTypeCode)
                    {
                        serviceTypeComboBox.SelectedItem = serviceType;
                        break;
                    }
                }

                // Устанавливаем соответствующее значение активности в ComboBox
                foreach (ComboBoxItem item in activityComboBox.Items)
                {
                    if (item.Content.ToString() == selectedMaster.Activity)
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