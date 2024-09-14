using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Текстовый_редактор
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            fontCombobox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            fontSizeBox.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            // Для шрифта
            fontCombobox.SelectedItem = textEditor.FontFamily;
            // Для размера букв
            fontSizeBox.SelectedItem = textEditor.FontSize;

            // Для изменения цвета
            ColourBrush.ItemsSource = typeof(Colors).GetProperties();
            ColourBrush.SelectedItem = typeof(Colors).GetProperty("Black");
            // Проверка орфографии
            textEditor.SpellCheck.IsEnabled = true;

        }

        // Кнопка открытия файлов
        private async void openBtn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(0);
            try
            {

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "RichText Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";

                if (ofd.ShowDialog() == true)
                {
                    TextRange doc = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        if (System.IO.Path.GetExtension(ofd.FileName).ToLower() == ".txt")
                            doc.Load(fs, DataFormats.Text);

                        if (System.IO.Path.GetExtension(ofd.FileName).ToLower() == ".rtf")
                            doc.Load(fs, DataFormats.Rtf);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // Кнопка сохранения текстового документа
        private async void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(0);
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "RichText Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
                if (sfd.ShowDialog() == true)
                {
                    TextRange doc = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
                    using (FileStream fs = File.Create(sfd.FileName))
                    {
                        if (System.IO.Path.GetExtension(sfd.FileName).ToLower() == ".txt")
                            doc.Save(fs, DataFormats.Text);

                        if (System.IO.Path.GetExtension(sfd.FileName).ToLower() == ".rtf")
                            doc.Save(fs, DataFormats.Rtf);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Кнопка для печати текста
        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog pd = new PrintDialog();

                if ((pd.ShowDialog() == true))
                {
                    pd.PrintVisual(textEditor as Visual, "Print Visual");
                    pd.PrintDocument((((IDocumentPaginatorSource)textEditor.Document).DocumentPaginator), "Print Document");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Изменение стиля шрифта
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fontCombobox.SelectedItem != null && textEditor != null)
            {
                textEditor.Selection.ApplyPropertyValue(RichTextBox.FontFamilyProperty, fontCombobox.SelectedItem);
                textEditor.Focus();
            }
        }

        // Изменение размера шрифта
        private void ComboBox_TextChanged(object sender, RoutedEventArgs e)
        {
            double size;

            if (textEditor != null)
                if (Double.TryParse(fontSizeBox.Text, out size)) {
                    textEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, size);
                    textEditor.Focus();
                }
        }

        // Форматирование в жирный, курсив и подчеркнут + горячие клавиши
        private void textEditor_SelectionChanged(object sender, RoutedEventArgs e) {
            try {
                object temp = textEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
                btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
                temp = textEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
                btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
                temp = textEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
                btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

                temp = textEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
                fontCombobox.SelectedItem = temp;
                temp = textEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
                fontSizeBox.Text = temp.ToString();

            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        // Изменение цвета текста
        private void ComboBox_ColourFonts(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Color selectedColor = (Color)(ColourBrush.SelectedItem as PropertyInfo).GetValue(null, null);

                if (textEditor == null) return;

                ApplyColorToSelectedText(selectedColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextEditor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (textEditor == null || ColourBrush.SelectedItem == null) return;

                // Получаем текущий цвет из ComboBox
                Color selectedColor = (Color)(ColourBrush.SelectedItem as PropertyInfo).GetValue(null, null);

                // Применяем цвет только к вновь введенному тексту
                TextPointer position = textEditor.CaretPosition;
                if (position != null)
                {
                    TextRange range = new TextRange(position.GetPositionAtOffset(-1), position);
                    range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(selectedColor));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyColorToSelectedText(Color color)
        {
            if (textEditor == null) return;

            if (textEditor.Selection != null && !textEditor.Selection.IsEmpty)
            {
                // Применяем цвет к выделенному тексту
                textEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
            else
            {
                // Получаем текущую позицию в RichTextBox
                TextPointer position = textEditor.CaretPosition;

                // Применяем цвет только к вновь введенному тексту
                TextRange range = new TextRange(position.GetPositionAtOffset(-1), position);
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
        }

        // Кнопка поиска слова
        int prev_pos       = 0;
        string prev_search = "";
        private void searchBtn_Click(object sender, RoutedEventArgs e) {
            string searchText = searchTextBox.Text.ToLower();

            if (prev_search != searchText) prev_pos = 0;
            if (string.IsNullOrEmpty(searchText)) {
                MessageBox.Show("Введите слово для поиска.");
                return;
            }

            var textRange       = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
            var currentPosition = textRange.Start;
            while (currentPosition != null) {
                string textRun = currentPosition.GetTextInRun(LogicalDirection.Forward).ToLower();
                if (!string.IsNullOrWhiteSpace(textRun)) {
                    var index = textRun.IndexOf(searchText, prev_pos);
                    if (index != -1) {
                        prev_pos    = index + 1;
                        prev_search = searchText;

                        TextPointer start = currentPosition.GetPositionAtOffset(index);
                        TextPointer end   = start.GetPositionAtOffset(searchText.Length);

                        textEditor.Selection.Select(start, end);
                        textEditor.ScrollToVerticalOffset(textEditor.VerticalOffset);
                        textEditor.Focus();
                        return;
                    }
                }

                currentPosition = currentPosition.GetNextContextPosition(LogicalDirection.Forward);
            }

            MessageBox.Show("Слово не найдено.");
        }
        // Кнопка возврата действия
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (textEditor.CanUndo)
            {
                textEditor.Undo();
            }
        }

    }

}

