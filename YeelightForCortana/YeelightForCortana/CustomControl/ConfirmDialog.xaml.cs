using Windows.UI.Xaml.Controls;

namespace YeelightForCortana.CustomControl
{
    /// <summary>
    /// 确认对话框
    /// </summary>
    public sealed partial class ConfirmDialog : ContentDialog
    {
        public bool Result { get; set; }

        /// <summary>
        /// 确认对话框
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okButtonText">确定文本</param>
        /// <param name="cancelButtonText">取消文本</param>
        public ConfirmDialog(string text, string title, string okButtonText, string cancelButtonText)
        {
            this.InitializeComponent();
            this.TB_Text.Text = text;
            this.Title = title;
            if (!string.IsNullOrEmpty(okButtonText))
                this.PrimaryButtonText = okButtonText;
            if (!string.IsNullOrEmpty(cancelButtonText))
                this.SecondaryButtonText = cancelButtonText;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = false;
        }
    }
}
