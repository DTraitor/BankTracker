using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LogicLayer;
using Xceed.Wpf.Toolkit;

namespace UserInterface;

public partial class DictionaryEditorWindow : Window
{
    public DictionaryEditorWindow()
    {
        InitializeComponent();
        Closing += (sender, args) => { args.Cancel = true; Hide(); depositConditionsHolder = null; onModified = null; };
    }

    public void SetDictionary(Dictionary<Bank.DepositType, int> dictionary, OnModified onModified)
    {
        depositConditionsHolder = dictionary;
        this.onModified = onModified;
        UpdateDictionaryContent();
    }

    private void UpdateDictionaryContent()
    {
        if (depositConditionsHolder == null)
            return;

        MainGrid.Children.Clear();
        MainGrid.RowDefinitions.Clear();
        foreach (Bank.DepositType depositType in Enum.GetValues(typeof(Bank.DepositType)))
        {
            RowDefinition row = new RowDefinition();
            row.Height = new GridLength(1, GridUnitType.Auto);
            MainGrid.RowDefinitions.Add(row);

            TextBlock depositTypeTextBlock = new TextBlock() { Text = depositType.ToString() };
            depositTypeTextBlock.Width = 65;
            Grid.SetColumn(depositTypeTextBlock, 1);
            Grid.SetRow(depositTypeTextBlock, (int)depositType);

            DecimalUpDown percentUpDown = new DecimalUpDown();
            percentUpDown.Minimum = 0;
            percentUpDown.Maximum = 1;
            percentUpDown.Increment = 0.01m;
            percentUpDown.FormatString = "P";
            if (depositConditionsHolder.ContainsKey(depositType))
            {
                percentUpDown.Value = depositConditionsHolder[depositType] / 100m;
            }
            else
            {
                percentUpDown.IsEnabled = false;
                percentUpDown.Value = 0;
            }
            percentUpDown.ValueChanged += (sender, args) =>
            {
                onModified.Invoke(depositType, (int)(percentUpDown.Value.Value * 100));
            };
            Grid.SetColumn(percentUpDown, 2);
            Grid.SetRow(percentUpDown, (int)depositType);

            CheckBox checkBox = new CheckBox();
            checkBox.IsChecked = depositConditionsHolder.ContainsKey(depositType);
            checkBox.Checked += (sender, args) =>
            {
                if (depositConditionsHolder.ContainsKey(depositType))
                    return;
                onModified?.Invoke(depositType, (int)(percentUpDown.Value.Value * 100));
                UpdateDictionaryContent();
            };
            checkBox.Unchecked += (sender, args) =>
            {
                if (!depositConditionsHolder.ContainsKey(depositType))
                    return;
                depositConditionsHolder.Remove(depositType);
                onModified?.Invoke(depositType, -1);
                UpdateDictionaryContent();
            };
            Grid.SetColumn(checkBox, 0);
            Grid.SetRow(checkBox, (int)depositType);

            MainGrid.Children.Add(checkBox);
            MainGrid.Children.Add(depositTypeTextBlock);
            MainGrid.Children.Add(percentUpDown);
        }
    }

    public delegate void OnModified(Bank.DepositType key, int value);

    private OnModified? onModified;
    private Dictionary<Bank.DepositType, int>? depositConditionsHolder;
}