using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace krasotkaDBLevonTEST
{
    public partial class ServTypes : Window
    {
        private const string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";

        public ObservableCollection<ServType> ServiceTypesList { get; set; }

        public ServTypes()
        {
            InitializeComponent();
            ServiceTypesList = new ObservableCollection<ServType>();
            servTypesGrid.ItemsSource = ServiceTypesList;
            LoadActiveServiceTypes();
        }

        private void LoadActiveServiceTypes()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT servTypeCode, servType, servTypeActivity FROM servTypes WHERE servTypeActivity = 'да' ORDER BY servTypeCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    ServiceTypesList.Clear();
                    while (reader.Read())
                    {
                        ServiceTypesList.Add(new ServType
                        {
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            TypeName = reader.GetString("servType"),
                            Activity = reader.GetString("servTypeActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadAllServiceTypes()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT servTypeCode, servType, servTypeActivity FROM servTypes ORDER BY servTypeCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    ServiceTypesList.Clear();
                    while (reader.Read())
                    {
                        ServiceTypesList.Add(new ServType
                        {
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            TypeName = reader.GetString("servType"),
                            Activity = reader.GetString("servTypeActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void SearchServiceTypes()
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                ShowWarning("Введите название типа услуги для поиска");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT servTypeCode, servType, servTypeActivity FROM servTypes WHERE servType LIKE @search ORDER BY servTypeCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@search", $"%{searchTextBox.Text.Trim()}%");

                    MySqlDataReader reader = command.ExecuteReader();

                    ServiceTypesList.Clear();
                    bool found = false;

                    while (reader.Read())
                    {
                        found = true;
                        ServiceTypesList.Add(new ServType
                        {
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            TypeName = reader.GetString("servType"),
                            Activity = reader.GetString("servTypeActivity")
                        });
                    }

                    if (!found)
                    {
                        ShowInfo("Типы услуг по вашему запросу не найдены");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка поиска: {ex.Message}");
            }
        }

        private void AddServiceType()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                ShowWarning("Заполните название типа услуги");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    int newServiceTypeCode = GetNextServiceTypeCode(connection);

                    string activity = "да";
                    if (activityComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        activity = selectedItem.Content.ToString();
                    }

                    string query = @"INSERT INTO servTypes (servTypeCode, servType, servTypeActivity)
                                    VALUES (@code, @name, @activity)";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@code", newServiceTypeCode);
                    command.Parameters.AddWithValue("@name", nameTextBox.Text.Trim());
                    command.Parameters.AddWithValue("@activity", activity);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowInfo("Тип услуги успешно добавлен");
                        ClearInputFields();
                        LoadActiveServiceTypes();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления типа услуги: {ex.Message}");
            }
        }

        private void EditServiceType()
        {
            if (servTypesGrid.SelectedItem == null)
            {
                ShowWarning("Выберите тип услуги для изменения");
                return;
            }

            var selectedServiceType = (ServType)servTypesGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите изменить данные типа услуги {selectedServiceType.TypeName}?"))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = @"UPDATE servTypes SET
                                        servType = @name,
                                        servTypeActivity = @activity
                                        WHERE servTypeCode = @code";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedServiceType.ServTypeCode);
                        command.Parameters.AddWithValue("@name",
                            string.IsNullOrWhiteSpace(nameTextBox.Text) ? selectedServiceType.TypeName : nameTextBox.Text.Trim());

                        string activity = "да";
                        if (activityComboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            activity = selectedItem.Content.ToString();
                        }
                        command.Parameters.AddWithValue("@activity", activity);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Данные типа услуги успешно обновлены");
                            ClearInputFields();
                            LoadActiveServiceTypes();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка изменения данных: {ex.Message}");
                }
            }
        }

        private void DeleteServiceType()
        {
            if (servTypesGrid.SelectedItem == null)
            {
                ShowWarning("Выберите тип услуги для удаления");
                return;
            }

            var selectedServiceType = (ServType)servTypesGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите удалить тип услуги {selectedServiceType.TypeName}?\n\n" +
                                "Тип услуги будет помечен как неактивный и скрыт из списка."))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        // Вместо физического удаления меняем активность на "нет"
                        string query = "UPDATE servTypes SET servTypeActivity = 'нет' WHERE servTypeCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedServiceType.ServTypeCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Тип услуги успешно удален (деактивирован)");
                            ClearInputFields();
                            LoadActiveServiceTypes();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ActivateServiceType()
        {
            if (servTypesGrid.SelectedItem == null)
            {
                ShowWarning("Выберите тип услуги для активации");
                return;
            }

            var selectedServiceType = (ServType)servTypesGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите активировать тип услуги {selectedServiceType.TypeName}?"))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE servTypes SET servTypeActivity = 'да' WHERE servTypeCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedServiceType.ServTypeCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Тип услуги успешно активирован");
                            ClearInputFields();
                            LoadActiveServiceTypes();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка активации: {ex.Message}");
                }
            }
        }

        private int GetNextServiceTypeCode(MySqlConnection connection)
        {
            string maxCodeQuery = "SELECT MAX(servTypeCode) FROM servTypes";
            MySqlCommand maxCommand = new MySqlCommand(maxCodeQuery, connection);
            var maxCode = maxCommand.ExecuteScalar();
            return maxCode == DBNull.Value ? 1 : Convert.ToInt32(maxCode) + 1;
        }

        private void ClearInputFields()
        {
            codeTextBox.Clear();
            nameTextBox.Clear();
            searchTextBox.Clear();
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
        private void SearchButton_Click(object sender, RoutedEventArgs e) => SearchServiceTypes();
        private void AddButton_Click(object sender, RoutedEventArgs e) => AddServiceType();
        private void EditButton_Click(object sender, RoutedEventArgs e) => EditServiceType();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteServiceType();
        private void ActivateButton_Click(object sender, RoutedEventArgs e) => ActivateServiceType();

        private void ShowActiveButton_Click(object sender, RoutedEventArgs e) => LoadActiveServiceTypes();
        private void ShowAllButton_Click(object sender, RoutedEventArgs e) => LoadAllServiceTypes();

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActiveServiceTypes();
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

        private void ServTypesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (servTypesGrid.SelectedItem is ServType selectedServiceType)
            {
                codeTextBox.Text = selectedServiceType.ServTypeCode.ToString();
                nameTextBox.Text = selectedServiceType.TypeName;

                // Устанавливаем соответствующее значение активности в ComboBox
                foreach (ComboBoxItem item in activityComboBox.Items)
                {
                    if (item.Content.ToString() == selectedServiceType.Activity)
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