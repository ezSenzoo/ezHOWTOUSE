using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using SEARCH.DTO;

namespace HOWTOUSE
{
    public partial class DutyTodayTaskView : UserControl
    {
        private readonly ObservableCollection<DutyPostItem> todayTaskItems = new ObservableCollection<DutyPostItem>();
        private readonly ObservableCollection<StaffInfoDTO> staffList = new ObservableCollection<StaffInfoDTO>();

        private DutyPostItem selectedTodayTaskItem;
        private bool isNewTodayTaskMode;
        private bool isLoading;

        public DutyTodayTaskView()
        {
            InitializeComponent();

            TodayTaskListBox.ItemsSource = todayTaskItems;
            //DutyDateTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " 기준";

            //SeedTodayTasks();
            SearchTaskDatePicker.SelectedDate = DateTime.Now;

            LoadTodayTasks();
            LoadStaffList();
        }

        private void NewTodayTaskButton_Click(object sender, RoutedEventArgs e)
        {
            isNewTodayTaskMode = true;

            selectedTodayTaskItem = new DutyPostItem();
            selectedTodayTaskItem.TASK_DT = SearchTaskDatePicker.SelectedDate;
            selectedTodayTaskItem.TRGT_TP_CD = "ALL";

            DetailPanel.DataContext = selectedTodayTaskItem;

            TodayTaskListBox.SelectedItem = null;

            TodayTaskEditorTitleTextBlock.Text = "확인 업무 추가";
            SaveTodayTaskButton.Content = "저장";

            TodayTaskTitleInput.Focus();

        }

        private void TodayTaskTitleButton_Click(object sender, RoutedEventArgs e)
        {
            DutyPostItem item = (sender as Button)?.CommandParameter as DutyPostItem;
            SelectTodayTask(item);
        }

        private void SaveTodayTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TodayTaskTitleInput.Text.Trim();
            string detail = TodayTaskDetailInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(detail))
            {
                MessageBox.Show("확인 업무의 제목과 내용을 입력해주세요.", "EZHOWTOUSE", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (isNewTodayTaskMode)
            {
                InsertTodayTask();
            }
            else
            {
                UpdateTodayTask();
            }

            long taskId = selectedTodayTaskItem.TASK_ID;

            LoadTodayTasks();

            SelectTodayTask(
                todayTaskItems.FirstOrDefault(x => x.TASK_ID == taskId));
        }

        private void SelectTodayTask(DutyPostItem item)
        {
            if (item == null) return;
            isNewTodayTaskMode = false;

            selectedTodayTaskItem = item.Clone();

            TodayTaskEditorTitleTextBlock.Text = "상세 내용";
            SaveTodayTaskButton.Content = "수정";

            DetailPanel.DataContext = selectedTodayTaskItem;

            if (item == null)
            {
                DetailPanel.DataContext = null;

                TargetUserComboBox.SelectedIndex = -1;
                TargetUserComboBox.IsEnabled = false;

                return;
            }

            TargetUserComboBox.SelectedValue = item.TRGT_STF_NO;
            TargetUserComboBox.IsEnabled = item.TRGT_TP_CD == "USER";
        }


        private void LoadTodayTasks()
        {
            isLoading = true;

            todayTaskItems.Clear();

            const string query = @"
                                    SELECT
                                        A.TASK_ID,
                                        A.TASK_DT,
                                        A.TASK_TITLE,
                                        A.TASK_CONTENT,
                                        A.TRGT_TP_CD,
                                        CASE A.TRGT_TP_CD
                                            WHEN 'ALL' THEN '공통업무'
                                            WHEN 'USER' THEN '개인업무'
                                        END AS TRGT_TP_NM,
                                        A.TRGT_STF_NO,
                                        A.CMPL_YN,
                                        A.CMPL_STF_NO,
                                        B.STF_NM AS CMPL_STF_NM,
                                        A.CMPL_DTM,
                                        A.FSR_STF_NO,
                                        A.FSR_DTM,
                                        B.STF_NM AS CMPL_STF_NM,
                                        A.LSH_DTM
                                    FROM MOODUWKD A
                                    LEFT JOIN CNLRRUSD B
                                           ON A.CMPL_STF_NO COLLATE utf8mb4_unicode_ci = B.STF_NO
                                    WHERE A.TASK_DT = @TASK_DT
                                      AND A.USE_YN = 'Y'
                                      AND A.LST_YN = 'Y'
                                      AND (
                                            A.TRGT_TP_CD = 'ALL'
                                            OR (
                                                A.TRGT_TP_CD = 'USER'
                                                AND A.TRGT_STF_NO = @TRGT_STF_NO
                                            )
                                          )
                                    ORDER BY A.TASK_ID ASC";


            using (MySqlConnection connection =
                new MySqlConnection(AppSettings.Current.Database.ConnectionString))

            using (MySqlCommand command =
                new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue(
    "@TASK_DT",
    SearchTaskDatePicker.SelectedDate.Value.ToString("yyyyMMdd"));

                command.Parameters.AddWithValue(
                    "@TRGT_STF_NO",
                    SessionContext.STF_NO);


                connection.Open();


                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DutyPostItem item = new DutyPostItem();

                        item.TASK_ID = Convert.ToInt64(reader["TASK_ID"]);
                        item.TASK_DT = Convert.ToDateTime(reader["TASK_DT"]);
                        item.TASK_TITLE = reader["TASK_TITLE"].ToString();
                        item.TASK_CONTENT = reader["TASK_CONTENT"].ToString();

                        item.TRGT_TP_CD = reader["TRGT_TP_CD"].ToString();
                        item.TRGT_TP_NM = reader["TRGT_TP_NM"].ToString();
                        item.TRGT_STF_NO = reader["TRGT_STF_NO"].ToString();
                        item.CMPL_YN = reader["CMPL_YN"].ToString();
                        item.IsCompleted = reader["CMPL_YN"].ToString() == "Y";


                        if (reader["CMPL_DTM"] != DBNull.Value)
                        {
                            item.CMPL_DTM = Convert.ToDateTime(reader["CMPL_DTM"]);
                            item.CMPL_STF_NO = reader["CMPL_STF_NO"].ToString();
                            item.CMPL_STF_NM = reader["CMPL_STF_NM"].ToString();
                        }

                        item.FSR_STF_NO = reader["FSR_STF_NO"].ToString();
                        item.FSR_DTM = Convert.ToDateTime(reader["FSR_DTM"]);
                        item.LSH_DTM = Convert.ToDateTime(reader["LSH_DTM"]);

                        todayTaskItems.Add(item);
                    }

                    //TaskCountTextBlock.Text = $"업무 {todayTaskItems.Count}건";
                }

                SelectTodayTask(todayTaskItems.FirstOrDefault());
                isLoading = false;
            }
        }


        private void SearchTaskDatePicker_SelectedDateChanged(
    object sender,
    SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            LoadTodayTasks();
        }

        private void LoadStaffList()
        {
            staffList.Clear();

            const string query = @"
                                SELECT
                                    STF_NO,
                                    STF_NM
                                FROM CNLRRUSD
                                ORDER BY STF_NM";

            using (MySqlConnection connection =
                new MySqlConnection(AppSettings.Current.Database.ConnectionString))
            using (MySqlCommand command =
                new MySqlCommand(query, connection))
            {
                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        staffList.Add(new StaffInfoDTO
                        {
                            STF_NO = reader["STF_NO"].ToString(),
                            STF_NM = reader["STF_NM"].ToString()
                        });
                    }
                }
            }

            TargetUserComboBox.ItemsSource = staffList;
        }




        private void UpdateTodayTask()
        {
            if (selectedTodayTaskItem == null)
            {
                return;
            }

            const string query = @"
UPDATE MOODUWKD
SET
    TASK_DT = @TASK_DT,
    TASK_TITLE = @TASK_TITLE,
    TASK_CONTENT = @TASK_CONTENT,
    TRGT_TP_CD = @TRGT_TP_CD,
    TRGT_STF_NO = @TRGT_STF_NO,
    LSH_DTM = NOW(),
    LSH_STF_NO = @LSH_STF_NO,
    LSH_PRGM_NM = @LSH_PRGM_NM,
    LSH_IP_ADDR = @LSH_IP_ADDR
WHERE TASK_ID = @TASK_ID";


            using (MySqlConnection connection =
                new MySqlConnection(AppSettings.Current.Database.ConnectionString))

            using (MySqlCommand command =
                new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue(
                    "@TASK_ID",
                    selectedTodayTaskItem.TASK_ID);

                command.Parameters.AddWithValue(
                    "@TASK_DT",
                    selectedTodayTaskItem.TASK_DT);

                command.Parameters.AddWithValue(
                    "@TASK_TITLE",
                    selectedTodayTaskItem.TASK_TITLE);

                command.Parameters.AddWithValue(
                    "@TASK_CONTENT",
                    selectedTodayTaskItem.TASK_CONTENT);

                command.Parameters.AddWithValue(
                    "@TRGT_TP_CD",
                    selectedTodayTaskItem.TRGT_TP_CD);

                command.Parameters.AddWithValue(
    "@TRGT_STF_NO",
    selectedTodayTaskItem.TRGT_TP_CD == "ALL"
        ? DBNull.Value
        : (object)selectedTodayTaskItem.TRGT_STF_NO);

                command.Parameters.AddWithValue(
                    "@LSH_STF_NO",
                    SessionContext.STF_NO);

                command.Parameters.AddWithValue(
    "@LSH_PRGM_NM",
    "DutyTodayTaskView");

                command.Parameters.AddWithValue(
                    "@LSH_IP_ADDR",
                    SessionContext.IP_ADDRESS);


                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void UserTargetRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TargetUserComboBox.IsEnabled = true;
        }
        private void AllTargetRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TargetUserComboBox.IsEnabled = false;
            TargetUserComboBox.SelectedIndex = -1;
        }

        private void InsertTodayTask()
        {
            if (selectedTodayTaskItem == null)
            {
                return;
            }


            const string query = @"
INSERT INTO MOODUWKD
(
    TASK_DT,
    TASK_TITLE,
    TASK_CONTENT,
    TRGT_TP_CD,
    TRGT_STF_NO,
    CMPL_YN,
    USE_YN,
    LST_YN,
    FSR_DTM,
    FSR_STF_NO,
    FSR_PRGM_NM,
    FSR_IP_ADDR,
    LSH_DTM,
    LSH_STF_NO,
    LSH_PRGM_NM,
    LSH_IP_ADDR
)
VALUES
(
    @TASK_DT,
    @TASK_TITLE,
    @TASK_CONTENT,
    @TRGT_TP_CD,
    @TRGT_STF_NO,
    'N',
    'Y',
    'Y',
    NOW(),
    @FSR_STF_NO,
    'DutyTodayTaskView',
    @FSR_IP_ADDR,
    NOW(),
    @LSH_STF_NO,
    'DutyTodayTaskView',
    @LSH_IP_ADDR
)";


            using (MySqlConnection connection =
                new MySqlConnection(AppSettings.Current.Database.ConnectionString))

            using (MySqlCommand command =
                new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue(
                    "@TASK_DT",
                    selectedTodayTaskItem.TASK_DT?.ToString("yyyyMMdd"));

                command.Parameters.AddWithValue(
                    "@TASK_TITLE",
                    selectedTodayTaskItem.TASK_TITLE);

                command.Parameters.AddWithValue(
                    "@TASK_CONTENT",
                    selectedTodayTaskItem.TASK_CONTENT);

                command.Parameters.AddWithValue(
                    "@TRGT_TP_CD",
                    selectedTodayTaskItem.TRGT_TP_CD);


                command.Parameters.AddWithValue(
    "@TRGT_STF_NO",
    selectedTodayTaskItem.TRGT_TP_CD == "ALL"
        ? DBNull.Value
        : (object)selectedTodayTaskItem.TRGT_STF_NO);


                command.Parameters.AddWithValue(
                    "@FSR_STF_NO",
                    SessionContext.STF_NO);

                command.Parameters.AddWithValue(
                    "@LSH_STF_NO",
                    SessionContext.STF_NO);


                command.Parameters.AddWithValue(
                    "@FSR_IP_ADDR",
                    SessionContext.IP_ADDRESS);

                command.Parameters.AddWithValue(
                    "@LSH_IP_ADDR",
                    SessionContext.IP_ADDRESS);


                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        private void TaskCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            CheckBox checkBox = sender as CheckBox;

            if (checkBox == null)
            {
                return;
            }

            DutyPostItem item = checkBox.DataContext as DutyPostItem;

            if (item == null)
            {
                return;
            }

            UpdateCompleteStatus(item.TASK_ID, checkBox.IsChecked == true);

            LoadTodayTasks();
        }

        private void UpdateCompleteStatus(long taskId, bool isCompleted)
        {
            const string query = @"
UPDATE MOODUWKD
SET
    CMPL_YN = @CMPL_YN,
    CMPL_STF_NO = @CMPL_STF_NO,
    CMPL_DTM = @CMPL_DTM,
    LSH_DTM = NOW(),
    LSH_STF_NO = @LSH_STF_NO,
    LSH_PRGM_NM = @LSH_PRGM_NM,
    LSH_IP_ADDR = @LSH_IP_ADDR
WHERE TASK_ID = @TASK_ID";

            using (MySqlConnection connection =
                new MySqlConnection(AppSettings.Current.Database.ConnectionString))
            using (MySqlCommand command =
                new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TASK_ID", taskId);

                command.Parameters.AddWithValue(
                    "@CMPL_YN",
                    isCompleted ? "Y" : "N");

                if (isCompleted)
                {
                    command.Parameters.AddWithValue(
                        "@CMPL_STF_NO",
                        SessionContext.STF_NO);

                    command.Parameters.AddWithValue(
                        "@CMPL_DTM",
                        DateTime.Now);
                }
                else
                {
                    command.Parameters.AddWithValue(
                        "@CMPL_STF_NO",
                        DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@CMPL_DTM",
                        DBNull.Value);
                }

                command.Parameters.AddWithValue(
                    "@LSH_STF_NO",
                    SessionContext.STF_NO);

                command.Parameters.AddWithValue(
    "@LSH_PRGM_NM",
    "DutyTodayTaskView");

                command.Parameters.AddWithValue(
                    "@LSH_IP_ADDR",
                    SessionContext.IP_ADDRESS);

                connection.Open();

                command.ExecuteNonQuery();
            }
        }
    }
}
