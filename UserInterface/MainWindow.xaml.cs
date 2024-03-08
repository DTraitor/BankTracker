using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LogicLayer;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OwnershipType.ItemsSource = Enum.GetValues(typeof(Bank.OwnershipType));
            OwnershipType.SelectedIndex = 0;
            CreditType.ItemsSource = Enum.GetValues(typeof(Bank.DepositType));
            UpdateBanksList();
            Closed += (sender, args) =>
            {
                dictionaryEditorWindow.Closing -= (sender, args) => { args.Cancel = true; dictionaryEditorWindow.Hide(); };
                dictionaryEditorWindow.Close();
                logic.Dispose();
            };
        }

        private void UpdateBanksList()
        {
            BanksGrid.Children.Clear();
            BanksGrid.RowDefinitions.Clear();
            foreach (Bank bank in logic.GetBanks())
            {
                BanksGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                Grid bankGrid = new Grid();
                bankGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                bankGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                bankGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Pixel) });
                bankGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                bankGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                bankGrid.Margin = new Thickness(2, 0, 2, 3);

                TextBox nameTextBlock = new TextBox() { Text = bank.Name };
                nameTextBlock.Margin = new Thickness(0, 0, 3, 0);
                nameTextBlock.TextChanged += (sender, args) =>
                {
                    logic.ChangeName(bank, nameTextBlock.Text);
                };
                Grid.SetColumn(nameTextBlock, 0);

                TextBox addressTextBlock = new TextBox() { Text = bank.Address };
                addressTextBlock.Margin = new Thickness(0, 0, 3, 0);
                addressTextBlock.TextChanged += (sender, args) =>
                {
                    logic.ChangeAddress(bank, addressTextBlock.Text);
                };
                Grid.SetColumn(addressTextBlock, 1);

                ComboBox ownershipTextBlock = new ComboBox() { Text = bank.Ownership.ToString() };
                ownershipTextBlock.Margin = new Thickness(0, 0, 3, 0);
                ownershipTextBlock.ItemsSource = Enum.GetValues(typeof(Bank.OwnershipType));
                ownershipTextBlock.SelectedIndex = (int)bank.Ownership;
                ownershipTextBlock.SelectionChanged += (sender, args) =>
                {
                    logic.ChangeOwnership(bank, (Bank.OwnershipType)ownershipTextBlock.SelectedIndex);
                };
                Grid.SetColumn(ownershipTextBlock, 2);

                Button editDepositsButton = new Button() { Content = "Умови Депозиту" };
                editDepositsButton.Margin = new Thickness(3, 0, 3, 0);
                editDepositsButton.Click += (sender, args) =>
                {
                    dictionaryEditorWindow.SetDictionary(bank.DepositConditions, (depositType, percent) =>
                    {
                        if (percent == -1)
                        {
                            logic.RemoveDepositCondition(bank, depositType);
                        }
                        else
                        {
                            logic.ChangeDepositCondition(bank, depositType, percent);
                        }
                    });
                    dictionaryEditorWindow.Show();
                };
                Grid.SetColumn(editDepositsButton, 3);

                Button deleteButton = new Button() { Content = "Видалити" };
                deleteButton.Margin = new Thickness(3, 0, 3, 0);
                deleteButton.Click += (sender, args) =>
                {
                    logic.DeleteBank(bank);
                    UpdateBanksList();
                };
                Grid.SetColumn(deleteButton, 4);

                bankGrid.Children.Add(nameTextBlock);
                bankGrid.Children.Add(addressTextBlock);
                bankGrid.Children.Add(ownershipTextBlock);
                bankGrid.Children.Add(editDepositsButton);
                bankGrid.Children.Add(deleteButton);

                Grid.SetRow(bankGrid, BanksGrid.RowDefinitions.Count);
                BanksGrid.Children.Add(bankGrid);
                bankToGrid[bank] = bankGrid;
            }
        }

        private Logic logic = new("database.json");
        private DictionaryEditorWindow dictionaryEditorWindow = new();
        private Dictionary<Bank.DepositType, int> depositConditionsHolder = new();
        private Dictionary<Bank, Grid> bankToGrid = new();

        private void OnShowDepositsEditor(object sender, RoutedEventArgs e)
        {
            dictionaryEditorWindow.SetDictionary(depositConditionsHolder, (depositType, percent) =>
            {
                if (percent == -1)
                {
                    depositConditionsHolder.Remove(depositType);
                }
                else
                {
                    depositConditionsHolder[depositType] = percent;
                }
            });
            dictionaryEditorWindow.Show();
        }

        private void OnAddNewBank(object sender, RoutedEventArgs e)
        {
            logic.CreateBank(NameBox.Text, AddressBox.Text, (Bank.OwnershipType)OwnershipType.SelectedIndex, depositConditionsHolder);
            UpdateBanksList();
        }

        private void OnSearchForBestBank(object sender, RoutedEventArgs e)
        {
            if (CreditType.SelectedIndex == -1)
                return;
            Bank? bestBank = logic.FindBestBank((Bank.DepositType)CreditType.SelectedIndex, Amount.Value.Value, Period.Value.Value);
            if (bestBank == null)
                return;
            bankToGrid[bestBank].Background = Brushes.LightGreen;
            bankToGrid[bestBank].BringIntoView();
            Task.Delay(1000).ContinueWith((task) =>
            {
                Dispatcher.Invoke(() =>
                {
                    bankToGrid[bestBank].Background = Brushes.Transparent;
                });
            });
        }
    }
}