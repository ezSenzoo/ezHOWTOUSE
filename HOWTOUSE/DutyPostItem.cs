using System;

namespace HOWTOUSE
{
    public class DutyPostItem
    {
        private bool isCompleted;

        public DutyPostItem()
        {
           
        }
        public DutyPostItem(string title, string detail, string category, string modifiedBy, DateTime modifiedAt)
        {
            Category = category;
            Update(title, detail, modifiedBy, modifiedAt);
        }

        public string Title { get; private set; }

        public string Detail { get; private set; }

        public string Category { get; private set; }

        public string LastModifiedLabel { get; private set; }

        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }

        public long TASK_ID { get; set; }

        public DateTime? TASK_DT { get; set; }

        public string TASK_TITLE { get; set; }
        public string TASK_CONTENT { get; set; }

        public string TRGT_TP_CD { get; set; }

        public string TRGT_STF_NO { get; set; }

        public string TRGT_TP_NM { get; set; }

        public string CMPL_STF_NO { get; set; }
        public string CMPL_STF_NM { get; set; }
        public string CMPL_YN { get; set; }
        public DateTime? CMPL_DTM { get; set; }

        public string FSR_STF_NO { get; set; }
        public DateTime? FSR_DTM { get; set; }
        public DateTime? LSH_DTM { get; set; }

        public void Update(string newTitle, string newDetail, string modifiedBy, DateTime modifiedAt)
        {
            Title = newTitle;
            Detail = newDetail;
            LastModifiedLabel = string.Format("마지막 수정: {0:yyyy-MM-dd HH:mm} · {1}", modifiedAt, modifiedBy);
        }

        public bool Contains(string keyword)
        {
            StringComparison comparison = StringComparison.CurrentCultureIgnoreCase;
            return Title.IndexOf(keyword, comparison) >= 0
                || Detail.IndexOf(keyword, comparison) >= 0
                || Category.IndexOf(keyword, comparison) >= 0;
        }

        public DutyPostItem Clone()
        {
            return (DutyPostItem)this.MemberwiseClone();
        }
    }
}
