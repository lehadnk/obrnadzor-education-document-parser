using System;
using System.Drawing;
using System.Windows.Forms;
using console.Threading;

namespace console.Input
{
    public class WindowsFormsCaptchaInput
    {
        private ReportReaderThreadExecutor _reportReaderThreadExecutor { get; }
        private Form _form;
        private TextBox _textBox;
        private Button _confirmButton;

        public WindowsFormsCaptchaInput(ReportReaderThreadExecutor reportReaderThreadExecutor)
        {
            _reportReaderThreadExecutor = reportReaderThreadExecutor;
        }

        public void DisplayCaptchaInputForm(string captchaImagePath)
        {
            _form = new Form();
            _form.Size = new Size(228, 208);
            _form.FormBorderStyle = FormBorderStyle.FixedSingle;
            _form.MaximizeBox = false;
            _form.MinimizeBox = false;
            _form.Text = "Ввод каптчи";
            
            var pb = new PictureBox();
            pb.ImageLocation = captchaImagePath;
            pb.SizeMode = PictureBoxSizeMode.AutoSize;
            pb.Size = new Size(200, 120);
            pb.Location = new Point(4, 4);
            pb.Parent = _form;

            _textBox = new TextBox();
            _textBox.Location = new Point(4, 114);
            _textBox.Size = new Size(204, 50);
            _textBox.Parent = _form;

            _confirmButton = new Button();
            _confirmButton.Location = new Point(4, 134);
            _confirmButton.Size = new Size(204, 30);
            _confirmButton.Text = "Принять";
            _confirmButton.Click += HandleConfirmButtonClick;
            _confirmButton.Parent = _form;
            
            _form.ShowDialog();
        }

        private void HandleConfirmButtonClick(object sender, EventArgs e)
        {
            _confirmButton.Enabled = false;
            _reportReaderThreadExecutor.ResumeExecution(_textBox.Text);
            _form.Close();
        }
    }
}