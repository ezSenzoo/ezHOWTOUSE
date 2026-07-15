using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HOWTOUSE
{
    public partial class PopupManualView : UserControl
    {
        private readonly ObservableCollection<PopupManualItem> popupManualItems = new ObservableCollection<PopupManualItem>();
        private readonly ObservableCollection<PopupManualItem> filteredPopupManualItems = new ObservableCollection<PopupManualItem>();
        private readonly ObservableCollection<PopupManualStep> editingSteps = new ObservableCollection<PopupManualStep>();
        private readonly ObservableCollection<string> editingKeywords = new ObservableCollection<string>();
        private PopupManualItem selectedPopupManualItem;
        private bool isNewPopupManual;

        public PopupManualView()
        {
            InitializeComponent();

            PopupGuideListBox.ItemsSource = filteredPopupManualItems;
            PopupStepItemsControl.ItemsSource = editingSteps;
            PopupKeywordItemsControl.ItemsSource = editingKeywords;

            SeedPopupManualItems();
            ApplyPopupManualFilter();
            SelectManualTab("Popup");
        }

        public void SelectManualTab(string tabName)
        {
            bool isPopup = tabName == "Popup";
            PopupManualPanel.Visibility = isPopup ? Visibility.Visible : Visibility.Collapsed;
            InquiryPanel.Visibility = isPopup ? Visibility.Collapsed : Visibility.Visible;

            if (isPopup && PopupGuideListBox.SelectedItem == null && filteredPopupManualItems.Count > 0)
            {
                PopupGuideListBox.SelectedIndex = 0;
            }
        }

        private void PopupSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyPopupManualFilter();
        }

        private void ApplyPopupManualFilter()
        {
            string keyword = PopupSearchTextBox == null ? string.Empty : PopupSearchTextBox.Text.Trim();
            List<PopupManualItem> matches = popupManualItems
                .Where(item => string.IsNullOrWhiteSpace(keyword) || item.Contains(keyword))
                .OrderBy(item => item.Message)
                .ToList();

            filteredPopupManualItems.Clear();
            foreach (PopupManualItem item in matches)
            {
                filteredPopupManualItems.Add(item);
            }

            if (filteredPopupManualItems.Count > 0 && PopupGuideListBox.SelectedItem == null)
            {
                PopupGuideListBox.SelectedIndex = 0;
            }
        }

        private void PopupGuideListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopupManualItem item = PopupGuideListBox.SelectedItem as PopupManualItem;
            if (item == null)
            {
                return;
            }

            LoadPopupManualItem(item);
        }

        private void NewPopupManualButton_Click(object sender, RoutedEventArgs e)
        {
            selectedPopupManualItem = null;
            isNewPopupManual = true;

            PopupGuideListBox.SelectedItem = null;
            PopupCategoryComboBox.SelectedIndex = 0;
            PopupMenuNameInput.Text = string.Empty;
            PopupMessageInput.Text = string.Empty;
            PopupSituationInput.Text = string.Empty;
            PopupRequesterInput.Text = string.Empty;
            PopupExtensionInput.Text = string.Empty;
            PopupKeywordInput.Text = string.Empty;
            editingKeywords.Clear();

            editingSteps.Clear();
            AddEditingStep(string.Empty);
        }

        private void AddPopupStepButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditingStep(string.Empty);
        }

        private void PopupKeywordInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            AddKeywordFromInput();
            e.Handled = true;
        }

        private void RemovePopupKeywordButton_Click(object sender, RoutedEventArgs e)
        {
            string keyword = (sender as Button)?.Tag as string;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                editingKeywords.Remove(keyword);
            }
        }

        private void SavePopupManualButton_Click(object sender, RoutedEventArgs e)
        {
            AddKeywordFromInput();

            string message = PopupMessageInput.Text.Trim();
            string situation = PopupSituationInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(situation))
            {
                MessageBox.Show("메시지 내용과 문제 상황을 입력해주세요.", "EZHOWTOUSE", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<PopupManualStep> savedSteps = editingSteps
                .Where(step => !string.IsNullOrWhiteSpace(step.Text) || step.Images.Count > 0)
                .Select(step => step.Clone())
                .ToList();

            if (savedSteps.Count == 0)
            {
                MessageBox.Show("해결방안을 한 단계 이상 입력해주세요.", "EZHOWTOUSE", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PopupManualItem target = selectedPopupManualItem;
            if (isNewPopupManual || target == null)
            {
                target = new PopupManualItem();
                popupManualItems.Insert(0, target);
            }

            target.Update(
                GetSelectedPopupCategory(),
                PopupMenuNameInput.Text.Trim(),
                message,
                situation,
                PopupRequesterInput.Text.Trim(),
                PopupExtensionInput.Text.Trim(),
                savedSteps,
                string.Join(", ", editingKeywords));

            selectedPopupManualItem = target;
            isNewPopupManual = false;

            ApplyPopupManualFilter();
            PopupGuideListBox.SelectedItem = target;
            PopupGuideListBox.Items.Refresh();
        }

        private void StepTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.V || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                return;
            }

            if (!Clipboard.ContainsImage())
            {
                return;
            }

            TextBox textBox = sender as TextBox;
            PopupManualStep step = textBox?.DataContext as PopupManualStep;
            if (step == null)
            {
                return;
            }

            BitmapSource image = Clipboard.GetImage();
            if (image == null)
            {
                return;
            }

            step.Images.Add(image);
            e.Handled = true;
        }

        private void LoadPopupManualItem(PopupManualItem item)
        {
            selectedPopupManualItem = item;
            isNewPopupManual = false;

            SelectPopupCategory(item.Category);
            PopupMenuNameInput.Text = item.MenuName;
            PopupMessageInput.Text = item.Message;
            PopupSituationInput.Text = item.Situation;
            PopupRequesterInput.Text = item.Requester;
            PopupExtensionInput.Text = item.ExtensionNumber;
            PopupKeywordInput.Text = string.Empty;
            LoadKeywords(item.Keywords);

            editingSteps.Clear();
            foreach (PopupManualStep step in item.Steps)
            {
                PopupManualStep editingStep = step.Clone();
                editingStep.Number = editingSteps.Count + 1;
                editingSteps.Add(editingStep);
            }

            if (editingSteps.Count == 0)
            {
                AddEditingStep(string.Empty);
            }
        }

        private void AddEditingStep(string text)
        {
            editingSteps.Add(new PopupManualStep
            {
                Number = editingSteps.Count + 1,
                Text = text
            });
        }

        private void AddKeywordFromInput()
        {
            string keyword = PopupKeywordInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            bool alreadyExists = editingKeywords.Any(item => string.Equals(item, keyword, StringComparison.CurrentCultureIgnoreCase));
            if (!alreadyExists)
            {
                editingKeywords.Add(keyword);
            }

            PopupKeywordInput.Text = string.Empty;
        }

        private void LoadKeywords(string keywords)
        {
            editingKeywords.Clear();
            if (string.IsNullOrWhiteSpace(keywords))
            {
                return;
            }

            string[] parts = keywords
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string keyword = part.Trim();
                if (!string.IsNullOrWhiteSpace(keyword) && !editingKeywords.Contains(keyword))
                {
                    editingKeywords.Add(keyword);
                }
            }
        }

        private string GetSelectedPopupCategory()
        {
            ComboBoxItem item = PopupCategoryComboBox.SelectedItem as ComboBoxItem;
            return item == null ? "진료/간호" : item.Content.ToString();
        }

        private void SelectPopupCategory(string category)
        {
            foreach (ComboBoxItem item in PopupCategoryComboBox.Items)
            {
                if (string.Equals(item.Content.ToString(), category, StringComparison.CurrentCultureIgnoreCase))
                {
                    PopupCategoryComboBox.SelectedItem = item;
                    return;
                }
            }

            PopupCategoryComboBox.SelectedIndex = 0;
        }

        private void SeedPopupManualItems()
        {
            PopupManualItem loginItem = new PopupManualItem();
            loginItem.Update(
                "일반관리",
                "로그인",
                "로그인 처리 중 오류가 발생했습니다.",
                "사용자가 사번과 비밀번호를 입력한 뒤 로그인 버튼을 눌렀을 때 오류 팝업이 표시됩니다.",
                "홍길동",
                "1234",
                new[]
                {
                    new PopupManualStep { Number = 1, Text = "사용자 사번이 CNLRRUSD 테이블에 존재하는지 확인합니다." },
                    new PopupManualStep { Number = 2, Text = "AppSettings.json의 Database 접속 정보가 현재 MySQL 설정과 일치하는지 확인합니다." },
                    new PopupManualStep { Number = 3, Text = "동일 오류가 반복되면 오류 메시지 전문과 발생 시간을 기록한 뒤 담당자에게 전달합니다." }
                },
                "로그인, 사번, 비밀번호, CNLRRUSD");
            popupManualItems.Add(loginItem);

            PopupManualItem printerItem = new PopupManualItem();
            printerItem.Update(
                "진료지원",
                "프린터",
                "프린터 연결을 확인할 수 없습니다.",
                "출력 버튼을 눌렀을 때 프린터 연결 실패 메시지가 표시됩니다.",
                "김민수",
                "5678",
                new[]
                {
                    new PopupManualStep { Number = 1, Text = "사용자 PC의 기본 프린터 설정을 확인합니다." },
                    new PopupManualStep { Number = 2, Text = "프린터 전원, 네트워크 연결, 용지 상태를 확인합니다." },
                    new PopupManualStep { Number = 3, Text = "테스트 페이지 출력 후 결과를 사용자에게 안내합니다." }
                },
                "프린터, 출력, 연결, 네트워크");
            popupManualItems.Add(printerItem);
        }
    }

    public class PopupManualItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Category { get; private set; }
        public string MenuName { get; private set; }
        public string Message { get; private set; }
        public string Situation { get; private set; }
        public string Requester { get; private set; }
        public string ExtensionNumber { get; private set; }
        public string Keywords { get; private set; }
        public ObservableCollection<PopupManualStep> Steps { get; private set; } = new ObservableCollection<PopupManualStep>();

        public string KeywordsLabel
        {
            get { return Keywords; }
        }

        public string MetaLabel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MenuName))
                {
                    return Category;
                }

                return Category + " · " + MenuName;
            }
        }

        public void Update(string category, string menuName, string message, string situation, string requester, string extensionNumber, IEnumerable<PopupManualStep> steps, string keywords)
        {
            Category = category;
            MenuName = menuName;
            Message = message;
            Situation = situation;
            Requester = requester;
            ExtensionNumber = extensionNumber;
            Keywords = keywords;

            Steps.Clear();
            int number = 1;
            foreach (PopupManualStep step in steps)
            {
                PopupManualStep savedStep = step.Clone();
                savedStep.Number = number++;
                Steps.Add(savedStep);
            }

            OnPropertyChanged("Category");
            OnPropertyChanged("MenuName");
            OnPropertyChanged("MetaLabel");
            OnPropertyChanged("Message");
            OnPropertyChanged("Situation");
            OnPropertyChanged("Requester");
            OnPropertyChanged("ExtensionNumber");
            OnPropertyChanged("Keywords");
            OnPropertyChanged("KeywordsLabel");
        }

        public bool Contains(string keyword)
        {
            StringComparison comparison = StringComparison.CurrentCultureIgnoreCase;
            return (Message ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || (Category ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || (MenuName ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || (Situation ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || (Requester ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || (ExtensionNumber ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || (Keywords ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || Steps.Any(step => (step.Text ?? string.Empty).IndexOf(keyword, comparison) >= 0);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PopupManualStep : INotifyPropertyChanged
    {
        private int number;
        private string text;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged("Number");
                OnPropertyChanged("NumberLabel");
            }
        }

        public string NumberLabel
        {
            get { return Number + "단계"; }
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        public ObservableCollection<BitmapSource> Images { get; private set; } = new ObservableCollection<BitmapSource>();

        public PopupManualStep Clone()
        {
            PopupManualStep clone = new PopupManualStep
            {
                Number = Number,
                Text = Text
            };

            foreach (BitmapSource image in Images)
            {
                clone.Images.Add(image);
            }

            return clone;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
