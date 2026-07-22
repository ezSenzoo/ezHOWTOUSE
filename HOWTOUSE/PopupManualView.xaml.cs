using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HOWTOUSE.DAC.Common;
using HOWTOUSE.DAC.PopupManual;
using HOWTOUSE.DTO.Common;
using HOWTOUSE.DTO.PopupManual;

namespace HOWTOUSE
{
    public partial class PopupManualView : UserControl, INotifyPropertyChanged
    {
        // 공통코드 관련 상수
        private const string PopupManualCategoryGroupCode = "00000002";     // 메뉴얼 카테고리


        private readonly ObservableCollection<PopupManualItem> popupManualItems = new ObservableCollection<PopupManualItem>();
        private readonly ObservableCollection<PopupManualItem> filteredPopupManualItems = new ObservableCollection<PopupManualItem>();

        private readonly ObservableCollection<PopupManualStep> editingSteps = new ObservableCollection<PopupManualStep>();      // 해결방안 단계 목록
        private readonly ObservableCollection<string> editingKeywords = new ObservableCollection<string>();     // 키워드 목록
        
        private PopupManualItem selectedPopupManualItem;
        private bool isNewPopupManual;

        public event PropertyChangedEventHandler PropertyChanged;

        

        public ObservableCollection<CommonCodeDto> CategoryItemList { get; } = new ObservableCollection<CommonCodeDto>();

        #region [View Properties]

        private CommonCodeDto selectedCategory;
        /// <summary>
        /// desc         : 매뉴얼 카테고리
        /// author       : 오승주
        /// create Date  : 2026-07-20
        /// update date  : 최종 수정일자 , 수정자, 수정개요
        /// </summary>
        public CommonCodeDto SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                OnPropertyChanged("SelectedCategory");
            }
        }

        private string manualTitle;
        /// <summary>
        /// desc         : 메뉴얼명
        /// author       : 오승주
        /// create Date  : 2026-07-20
        /// update date  : 최종 수정일자 , 수정자, 수정개요
        /// </summary>
        public string ManualTitle
        {
            get { return this.manualTitle; }
            set
            {
                if (this.manualTitle != value)
                {
                    this.manualTitle = value;
                    this.OnPropertyChanged("ManualTitle");
                }
            }
        }

        private string manualPopupMessageContent;
        /// <summary>
        /// desc         : 메뉴얼 팝업 메시지 내용
        /// author       : 오승주
        /// create Date  : 2026-07-20
        /// update date  : 최종 수정일자 , 수정자, 수정개요
        /// </summary>
        public string ManualPopupMessageContent
        {
            get { return this.manualPopupMessageContent; }
            set
            {
                if (this.manualPopupMessageContent != value)
                {
                    this.manualPopupMessageContent = value;
                    this.OnPropertyChanged("ManualPopupMessageContent");
                }
            }
        }

        private string manualProblemSituation;
        /// <summary>
        /// desc         : 메뉴얼 문제상황
        /// author       : 오승주
        /// create Date  : 2026-07-20
        /// update date  : 최종 수정일자 , 수정자, 수정개요
        /// </summary>
        public string ManualProblemSituation
        {
            get { return this.manualProblemSituation; }
            set
            {
                if (this.manualProblemSituation != value)
                {
                    this.manualProblemSituation = value;
                    this.OnPropertyChanged("ManualProblemSituation");
                }
            }
        }

        private string inquirerName;
        /// <summary>
        /// desc         : 문의자
        /// author       : 오승주
        /// create Date  : 2026-07-20
        /// update date  : 최종 수정일자 , 수정자, 수정개요
        /// </summary>
        public string InquirerName
        {
            get { return this.inquirerName; }
            set
            {
                if (this.inquirerName != value)
                {
                    this.inquirerName = value;
                    this.OnPropertyChanged("InquirerName");
                }
            }
        }

        private string contactNumber;
        /// <summary>
        /// desc         : 연락 가능 내선번호
        /// author       : 오승주
        /// create Date  : 2026-07-20
        /// update date  : 최종 수정일자 , 수정자, 수정개요
        /// </summary>
        public string ContactNumber
        {
            get { return this.contactNumber; }
            set
            {
                if (this.contactNumber != value)
                {
                    this.contactNumber = value;
                    this.OnPropertyChanged("ContactNumber");
                }
            }
        }


        private string keywordInputText;
        /// <summary>
        /// desc         : 키워드 입력 텍스트
        /// author       : 오승주
        /// create Date  : 2026-07-22
        /// update date  : 최종 수정일자 , 수정자, 수정개요
        /// </summary>
        public string KeywordInputText
        {
            get { return keywordInputText; }
            set
            {
                if (keywordInputText != value) 
                {
                    keywordInputText = value;
                    OnPropertyChanged("KeywordInputText");
                }
            }
        }

        #endregion




        public PopupManualView()
        {
            InitializeComponent();

            DataContext = this;

            PopupGuideListBox.ItemsSource = filteredPopupManualItems;
            PopupStepItemsControl.ItemsSource = editingSteps;
            PopupKeywordItemsControl.ItemsSource = editingKeywords;

            LoadCategoryCodes();
            LoadPopupManualList();
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

        private void PopupKeywordInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

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

        #region [ICommand]

        private ICommand addNewManualCommand;
        public ICommand AddNewManualCommand
        {
            get
            {
                if (addNewManualCommand == null)
                    addNewManualCommand = new RelayCommand(p => this.AddNewManual());
                return addNewManualCommand;
            }
        }

        private ICommand saveManualCommand;
        /// <summary>
        /// desc         : 메뉴얼 저장 커맨드
        /// author       : 오승주 
        /// create date  : 2024-08-06 오후 4:21:07
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        /// <remarks></remarks>
        public ICommand SaveManualCommand
        {
            get
            {
                if (saveManualCommand == null)
                    saveManualCommand = new RelayCommand(p => this.SaveManual());
                return saveManualCommand;
            }
        }

        private ICommand addStepCommand;
        /// <summary>
        /// desc         : 해결방안 단계추가 커맨드
        /// author       : 오승주 
        /// create date  : 2024-08-06 오후 4:21:07
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        /// <remarks></remarks>
        public ICommand AddStepCommand
        {
            get
            {
                if (addStepCommand == null)
                    addStepCommand = new RelayCommand(p => this.AddSteps());
                return addStepCommand;
            }
        }

        private ICommand addKeywordCommand;
        /// <summary>
        /// desc         : 키워드 추가 커맨드
        /// author       : 오승주
        /// create date  : 2026-07-22
        /// update date  : 최종 수정 일자, 수정자, 수정개요
        /// </summary>
        public ICommand AddKeywordCommand
        {
            get
            {
                if (addKeywordCommand == null)
                {
                    addKeywordCommand = new RelayCommand(p => this.AddKeyword());
                }

                return addKeywordCommand;
            }
        }

        #endregion

        #region [METHOD]

        /// <summary>
        /// name         : 매뉴얼 추가 메서드
        /// desc         : 매뉴얼 추가 메서드
        /// author       : 오승주 
        /// create date  : 2026-07-20
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private void AddNewManual()
        {
            selectedPopupManualItem = null;
            isNewPopupManual = true;

            PopupGuideListBox.SelectedItem = null;
            SelectedCategory = CategoryItemList.FirstOrDefault();

            // 데이터 입력값 초기화
            ManualTitle = string.Empty;                 // 메뉴얼명
            ManualPopupMessageContent = string.Empty;   // 팝업 메시지 내용
            ManualProblemSituation = string.Empty;      // 문제상황
            InquirerName = string.Empty;                // 문의자
            ContactNumber = string.Empty;               // 연락 가능한 내선번호

            PopupKeywordInput.Text = string.Empty;

            editingKeywords.Clear();

            editingSteps.Clear();
            AddSteps();
        }

        /// <summary>
        /// name         : 매뉴얼 저장 메서드
        /// desc         : 매뉴얼 저장 메서드
        /// author       : 오승주 
        /// create date  : 2026-07-20
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private void SaveManual()
        {
            // 입력값 검증 로직


            List<PopupManualStep> savedSteps = editingSteps
                .Where(step => !string.IsNullOrWhiteSpace(step.Text) || step.Images.Count > 0)
                .Select(step => step.Clone())
                .ToList();

            // 메뉴얼 INOUT DTO 객체 생성
            PopupManual_INOUT inDTO = new PopupManual_INOUT();

            // 화면에서 입력값 단일 데이터 매핑
            inDTO.CategoryCd = SelectedCategory.ComnCd;
            inDTO.ManualNm = ManualTitle.Trim();
            inDTO.MessageCnte = ManualPopupMessageContent.Trim();
            inDTO.ProblemCnte = ManualProblemSituation.Trim();
            inDTO.AskStfNm = (InquirerName ?? string.Empty).Trim();
            inDTO.TelNo = (ContactNumber ?? string.Empty).Trim();

            // 단계와 이미지 매핑
            MapStepsAndImages(inDTO, savedSteps);

            // 키워드 매핑
            MapKeywords(inDTO);

            // 등록직원번호, 등록일시, 수정직원번호, 수정일시 데이터 세팅
            inDTO.FsrStfNo = LoginSession.EmployeeNo;
            inDTO.FsrDtm = System.DateTime.Now;
            inDTO.LshStfNo = LoginSession.EmployeeNo;
            inDTO.LshDtm = System.DateTime.Now;

            // 저장 로직 호출
            try
            {
                PopupManualDac popupManualDac = new PopupManualDac();
                int manualNo = popupManualDac.InsertPopupManual(AppSettings.Current.Database.ConnectionString,inDTO);

                PopupManualItem target = selectedPopupManualItem;
                if (isNewPopupManual || target == null)
                {
                    target = new PopupManualItem();
                    popupManualItems.Insert(0, target);
                }

                target.ManuNo = manualNo;
                target.Update(
                    inDTO.CategoryCd,
                    GetSelectedPopupCategoryName(),
                    inDTO.ManualNm,
                    inDTO.MessageCnte,
                    inDTO.ProblemCnte,
                    inDTO.AskStfNm,
                    inDTO.TelNo,
                    savedSteps,
                    string.Join(", ", editingKeywords));

                selectedPopupManualItem = target;
                isNewPopupManual = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"매뉴얼 저장 중 오류가 발생했습니다.\n\n{ex.Message}", "EZHOWTOUSE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 저장 후, 화면 재로딩
            ApplyPopupManualFilter();
            PopupGuideListBox.SelectedItem = selectedPopupManualItem;
            PopupGuideListBox.Items.Refresh();
            MessageBox.Show("팝업 메시지 매뉴얼이 저장되었습니다.", "EZHOWTOUSE", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// name         : 단계 이미지 매핑 메서드
        /// desc         : 단계 이미지 매핑 메서드
        /// author       : 오승주 
        /// create date  : 2026-07-20
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private void MapStepsAndImages(PopupManual_INOUT inDTO, List<PopupManualStep> savedSteps)
        {
            int stageNo = 1;

            foreach (PopupManualStep step in savedSteps)
            {
                inDTO.Steps.Add(new PopupManualStepDto
                {
                    StageNo = stageNo,
                    SolutionCnte = step.Text
                });

                int imageSequence = 1;
                foreach (BitmapSource image in step.Images)
                {
                    inDTO.Images.Add(new PopupManualImageDto
                    {
                        StageNo = stageNo,
                        ImageSeq = imageSequence++,
                        ImageData = ConvertImageToBytes(image)
                    });
                }

                stageNo++;
            }
        }

        /// <summary>
        /// name         : 키워드 매핑 메서드
        /// desc         : 키워드 매핑 메서드
        /// author       : 오승주 
        /// create date  : 2026-07-20
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private void MapKeywords(PopupManual_INOUT inDTO)
        {
            int keywordSequence = 1;

            foreach (string keyword in editingKeywords)
            {
                inDTO.Keywords.Add(new PopupManualKeywordDto
                {
                    KeywordNm = keyword,
                    ScrnSortSeq = keywordSequence++
                });
            }
        }

        private static byte[] ConvertImageToBytes(BitmapSource image)
        {
            if (image == null)
            {
                return null;
            }

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// name         : 해결방안 단계추가
        /// desc         : 해결방안 단계추가
        /// author       : 오승주 
        /// create date  : 2026-07-20
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private void AddSteps()
        {
            // 단계 추가 로직
            editingSteps.Add(new PopupManualStep
            {
                Number = editingSteps.Count + 1,
                Text = String.Empty
            });
        }

        /// <summary>
        /// name         : 키워드 추가 매서드
        /// desc         : 키워드 추가 매서드
        /// author       : 오승주 
        /// create date  : 2026-07-22
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private void AddKeyword()
        {
            string keyword = (KeywordInputText ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(keyword)) return;

            // 기존에 존재하는 키워드인지 확인
            bool alreadyExists = editingKeywords.Any(item => string.Equals(item, keyword, StringComparison.CurrentCultureIgnoreCase));

            if (!alreadyExists)
            {
                editingKeywords.Add(keyword);
            }

            // 엔터를 누른후 키워드 추가 입력을 위해 초기화
            KeywordInputText = string.Empty;
        }

        #endregion [METHOD]

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

            SelectPopupCategory(item.CategoryCd);

            // 메뉴얼 내용 조회 후 세팅
            ManualTitle = item.MenuName;                // 메뉴얼 명
            ManualPopupMessageContent = item.Message;   // 팝업 메시지 내용
            ManualProblemSituation = item.Situation;    // 문제상황
            InquirerName = item.Requester;              // 문의자
            ContactNumber = item.ExtensionNumber;       // 연락 가능 내선번호

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
                AddSteps();
            }
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

        private string GetSelectedPopupCategoryCode()
        {
            return SelectedCategory == null ? string.Empty : SelectedCategory.ComnCd;
        }

        private string GetSelectedPopupCategoryName()
        {
            return SelectedCategory == null ? string.Empty : SelectedCategory.ComnCdNm;
        }

        private void SelectPopupCategory(string categoryCd)
        {
            SelectedCategory = CategoryItemList.FirstOrDefault(item => string.Equals(item.ComnCd, categoryCd, StringComparison.OrdinalIgnoreCase))
                ?? CategoryItemList.FirstOrDefault();
        }

        private void LoadCategoryCodes()
        {
            CategoryItemList.Clear();
            
            CommonCodeDac commonCodeDac = new CommonCodeDac();
            
            // 메뉴얼 카테고리 분류 목록 조회
            List<CommonCodeDto> categoryCodes = commonCodeDac.SelectCommonCodeList(AppSettings.Current.Database.ConnectionString, PopupManualCategoryGroupCode);

            foreach (CommonCodeDto categoryCode in categoryCodes)
            {
                CategoryItemList.Add(categoryCode);
            }
            
            SelectedCategory = CategoryItemList.FirstOrDefault();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// name         : 전체 매뉴얼 리스트 조회
        /// desc         : 전체 매뉴얼 리스트 조회
        /// author       : 오승주 
        /// create date  : 2026-07-21
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private void LoadPopupManualList()
        {
            popupManualItems.Clear();

            PopupManualDac popupManualDac = new PopupManualDac();

            List<PopupManual_INOUT> manuals = popupManualDac.SelectPopupManualList(AppSettings.Current.Database.ConnectionString);

            foreach(PopupManual_INOUT manual in manuals)
            {
                popupManualItems.Add(ConvertToPopupManualItem(manual));
            }
        }

        /// <summary>
        /// name         : 입력한 데이터를 매뉴얼로 변경
        /// desc         : 입력한 데이터를 매뉴얼로 변경
        /// author       : 오승주 
        /// create date  : 2026-07-22
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private PopupManualItem ConvertToPopupManualItem(PopupManual_INOUT manual)
        {
            PopupManualItem item = new PopupManualItem();
            item.ManuNo = manual.ManuNo;

            List<PopupManualStep> steps = manual.Steps
                .OrderBy(step => step.StageNo)
                .Select(step => new PopupManualStep
                {
                    Number = step.StageNo,
                    Text = step.SolutionCnte
                })
                .ToList();

            foreach (PopupManualStep step in steps)
            {
                List<PopupManualImageDto> images = manual.Images
                    .Where(image => image.StageNo == step.Number)
                    .OrderBy(image => image.ImageSeq)
                    .ToList();

                foreach (PopupManualImageDto image in images)
                {
                    BitmapSource bitmap = ConvertBytesToImage(image.ImageData);

                    if (bitmap != null)
                    {
                        step.Images.Add(bitmap);
                    }
                }
            }

            item.Update(
                manual.CategoryCd,
                GetCategoryName(manual.CategoryCd),
                manual.ManualNm,
                manual.MessageCnte,
                manual.ProblemCnte,
                manual.AskStfNm,
                manual.TelNo,
                steps,
                string.Join(", ", manual.Keywords
                    .OrderBy(keyword => keyword.ScrnSortSeq)
                    .Select(keyword => keyword.KeywordNm)));

            return item;
        }

        /// <summary>
        /// name         : 이미지 byte배열을 이미지로 변경
        /// desc         : 이미지 byte배열을 이미지로 변경
        /// author       : 오승주 
        /// create date  : 2026-07-22
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private static BitmapSource ConvertBytesToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                return null;
            }

            BitmapImage bitmap = new BitmapImage();

            using (MemoryStream stream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }

        /// <summary>
        /// name         : 카테고리 코드를 카테고리명으로 변경
        /// desc         : 카테고리 코드를 카테고리명으로 변경
        /// author       : 오승주 
        /// create date  : 2026-07-22
        /// update date  : 최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        private string GetCategoryName(string categoryCd)
        {
            CommonCodeDto category = CategoryItemList
                .FirstOrDefault(item => string.Equals(item.ComnCd, categoryCd, StringComparison.OrdinalIgnoreCase));

            return category == null ? categoryCd : category.ComnCdNm;
        }
    }

    public class PopupManualItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ManuNo { get; set; }
        public string CategoryCd { get; private set; }
        public string CategoryName { get; private set; }
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
                    return CategoryName;
                }

                return CategoryName + " · " + MenuName;
            }
        }

        public void Update(string categoryCd, string categoryName, string menuName, string message, string situation, string requester, string extensionNumber, IEnumerable<PopupManualStep> steps, string keywords)
        {
            CategoryCd = categoryCd;
            CategoryName = categoryName;
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

            OnPropertyChanged("CategoryCd");
            OnPropertyChanged("CategoryName");
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
                || (CategoryCd ?? string.Empty).IndexOf(keyword, comparison) >= 0
                || (CategoryName ?? string.Empty).IndexOf(keyword, comparison) >= 0
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

    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
