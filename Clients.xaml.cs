using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace krasotkaDBLevonTEST
{
    public partial class Clients : Window
    {
        private const string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";

        public ObservableCollection<Client> ClientsList { get; set; }

        public Clients()
        {
            InitializeComponent();
            ClientsList = new ObservableCollection<Client>();
            clientsGrid.ItemsSource = ClientsList;
            LoadActiveClients();
        }

        private void LoadActiveClients()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT clientCode, clientName, clientTel, clientActivity 
                                    FROM clients 
                                    WHERE clientActivity = 'да' 
                                    ORDER BY clientCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    ClientsList.Clear();
                    while (reader.Read())
                    {
                        ClientsList.Add(new Client
                        {
                            ClientCode = reader.GetInt32("clientCode"),
                            ClientName = reader.GetString("clientName"),
                            ClientTel = reader.GetString("clientTel"),
                            ClientActivity = reader.GetString("clientActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadAllClients()
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT clientCode, clientName, clientTel, clientActivity 
                                    FROM clients 
                                    ORDER BY clientCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    ClientsList.Clear();
                    while (reader.Read())
                    {
                        ClientsList.Add(new Client
                        {
                            ClientCode = reader.GetInt32("clientCode"),
                            ClientName = reader.GetString("clientName"),
                            ClientTel = reader.GetString("clientTel"),
                            ClientActivity = reader.GetString("clientActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void SearchClients()
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
                    string query = @"SELECT clientCode, clientName, clientTel, clientActivity 
                                    FROM clients 
                                    WHERE (clientName LIKE @search OR clientTel LIKE @search)
                                    ORDER BY clientCode";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@search", $"%{searchTextBox.Text.Trim()}%");

                    MySqlDataReader reader = command.ExecuteReader();

                    ClientsList.Clear();
                    bool found = false;

                    while (reader.Read())
                    {
                        found = true;
                        ClientsList.Add(new Client
                        {
                            ClientCode = reader.GetInt32("clientCode"),
                            ClientName = reader.GetString("clientName"),
                            ClientTel = reader.GetString("clientTel"),
                            ClientActivity = reader.GetString("clientActivity")
                        });
                    }

                    if (!found)
                    {
                        ShowInfo("Клиенты по вашему запросу не найдены");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка поиска: {ex.Message}");
            }
        }

        private void AddClient()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                string.IsNullOrWhiteSpace(phoneTextBox.Text))
            {
                ShowWarning("Заполните все обязательные поля: Имя и Телефон");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    int newClientCode = GetNextClientCode(connection);

                    string activity = "да";
                    if (activityComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        activity = selectedItem.Content.ToString();
                    }

                    string query = @"INSERT INTO clients (clientCode, clientName, clientTel, clientActivity)
                                    VALUES (@code, @name, @phone, @activity)";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@code", newClientCode);
                    command.Parameters.AddWithValue("@name", nameTextBox.Text.Trim());
                    command.Parameters.AddWithValue("@phone", phoneTextBox.Text.Trim());
                    command.Parameters.AddWithValue("@activity", activity);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowInfo("Клиент успешно добавлен");
                        ClearInputFields();
                        LoadActiveClients();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления клиента: {ex.Message}");
            }
        }

        private void EditClient()
        {
            if (clientsGrid.SelectedItem == null)
            {
                ShowWarning("Выберите клиента для изменения");
                return;
            }

            var selectedClient = (Client)clientsGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите изменить данные клиента {selectedClient.ClientName}?"))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = @"UPDATE clients SET
                                        clientName = @name,
                                        clientTel = @phone,
                                        clientActivity = @activity
                                        WHERE clientCode = @code";

                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedClient.ClientCode);
                        command.Parameters.AddWithValue("@name",
                            string.IsNullOrWhiteSpace(nameTextBox.Text) ? selectedClient.ClientName : nameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@phone",
                            string.IsNullOrWhiteSpace(phoneTextBox.Text) ? selectedClient.ClientTel : phoneTextBox.Text.Trim());

                        string activity = "да";
                        if (activityComboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            activity = selectedItem.Content.ToString();
                        }
                        command.Parameters.AddWithValue("@activity", activity);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Данные клиента успешно обновлены");
                            ClearInputFields();
                            LoadActiveClients();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка изменения данных: {ex.Message}");
                }
            }
        }

        private void DeleteClient()
        {
            if (clientsGrid.SelectedItem == null)
            {
                ShowWarning("Выберите клиента для удаления");
                return;
            }

            var selectedClient = (Client)clientsGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите удалить клиента {selectedClient.ClientName}?\n\n" +
                                "Клиент будет помечен как неактивный и скрыт из списка."))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        // Вместо физического удаления меняем активность на "нет"
                        string query = "UPDATE clients SET clientActivity = 'нет' WHERE clientCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedClient.ClientCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Клиент успешно удален (деактивирован)");
                            ClearInputFields();
                            LoadActiveClients();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ActivateClient()
        {
            if (clientsGrid.SelectedItem == null)
            {
                ShowWarning("Выберите клиента для активации");
                return;
            }

            var selectedClient = (Client)clientsGrid.SelectedItem;

            if (ShowConfirmation($"Вы уверены, что хотите активировать клиента {selectedClient.ClientName}?"))
            {
                try
                {
                    using (var connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE clients SET clientActivity = 'да' WHERE clientCode = @code";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@code", selectedClient.ClientCode);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowInfo("Клиент успешно активирован");
                            ClearInputFields();
                            LoadActiveClients();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка активации: {ex.Message}");
                }
            }
        }

        private int GetNextClientCode(MySqlConnection connection)
        {
            string maxCodeQuery = "SELECT MAX(clientCode) FROM clients";
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
        private void SearchButton_Click(object sender, RoutedEventArgs e) => SearchClients();
        private void AddButton_Click(object sender, RoutedEventArgs e) => AddClient();
        private void EditButton_Click(object sender, RoutedEventArgs e) => EditClient();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DeleteClient();
        private void ActivateButton_Click(object sender, RoutedEventArgs e) => ActivateClient();

        private void ShowActiveButton_Click(object sender, RoutedEventArgs e) => LoadActiveClients();
        private void ShowAllButton_Click(object sender, RoutedEventArgs e) => LoadAllClients();

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActiveClients();
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

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (clientsGrid.SelectedItem is Client selectedClient)
            {
                codeTextBox.Text = selectedClient.ClientCode.ToString();
                nameTextBox.Text = selectedClient.ClientName;
                phoneTextBox.Text = selectedClient.ClientTel;

                // Устанавливаем соответствующее значение активности в ComboBox
                foreach (ComboBoxItem item in activityComboBox.Items)
                {
                    if (item.Content.ToString() == selectedClient.ClientActivity)
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