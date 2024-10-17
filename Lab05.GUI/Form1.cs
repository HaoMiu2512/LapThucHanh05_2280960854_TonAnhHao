using Lab05.BUS;
using Lab05.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab05.GUI
{
    public partial class FrmStudent : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private string avatarFilePath = string.Empty;
        public FrmStudent()
        {
            InitializeComponent();
        }

        private void FrmStudent_Load(object sender, EventArgs e)
        {
            try
            {
                setGridViewStyle(dgvStudent);
                var listFacultys = facultyService.GetAll();
                var listStudents = studentService.GetAll();
                FillFalcultyCombobox(listFacultys);
                BindGrid(listStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            listFacultys.Insert(0, new Faculty());
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }

        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[0].Value = item.StudentID;
                dgvStudent.Rows[index].Cells[1].Value = item.FullName;
                if (item.Faculty != null)
                    dgvStudent.Rows[index].Cells[2].Value =
                   item.Faculty.FacultyName;
                dgvStudent.Rows[index].Cells[3].Value = item.AverageScore +
               "";
                if (item.MajorID != null)
                    dgvStudent.Rows[index].Cells[4].Value = item.Major.Name +
                   "";
                LoadAvatar(item.Avatar);
            }
        }
        private void ShowAvatar(string ImageName)
        {
            if (string.IsNullOrEmpty(ImageName))
            {
                picAvatar.Image = null;
            }
            else
            {
                string parentDirectory =
               Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string imagePath = Path.Combine(parentDirectory, "Images",
               ImageName);
                picAvatar.Image = Image.FromFile(imagePath);
                picAvatar.Refresh();
            }
        }
        public void setGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle =
           DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void chkUnregisterMajor_CheckedChanged(object sender, EventArgs e)
        {
            var listStudents = new List<Student>();
            if (this.chkUnregisterMajor.Checked)
                listStudents = studentService.GetAllHasNoMajor();
            else
                listStudents = studentService.GetAll();
            BindGrid(listStudents);
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    avatarFilePath = openFileDialog.FileName;
                    picAvatar.Image = Image.FromFile(avatarFilePath);
                }
            }
        }

        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudent.Rows[e.RowIndex];
                txtStudentID.Text = row.Cells[0].Value.ToString();
                txtFullName.Text = row.Cells[1].Value.ToString();
                cmbFaculty.Text = row.Cells[2].Value.ToString();
                txtAverageScore.Text = row.Cells[3].Value.ToString();
                string studentID = row.Cells[0].Value.ToString(); // Giả sử cột đầu tiên là studentID

                // Gọi ShowAvatar với avatarFileName và studentID
                LoadAvatar(studentID);
            }
        }
        private void LoadAvatar(string studentID)
        {
            string folderPath = Path.Combine(Application.StartupPath, "Images");
            //string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            //MessageBox.Show("Đường dẫn thư mục: " + folderPath);
            //if (Directory.Exists(folderPath))
            //{
            //    MessageBox.Show("Thư mục Images tồn tại!");
            //    string[] files = Directory.GetFiles(folderPath);
            //    MessageBox.Show("Có " + files.Length + " file trong thư mục Images.");
            //}
            //else
            //{
            //    MessageBox.Show("Thư mục Images không tồn tại.");
            //}

            var student = studentService.FindById(studentID);
            if (student != null && !string.IsNullOrEmpty(student.Avatar))
            {
                string avatarFilePath = Path.Combine(folderPath, student.Avatar);
                if (File.Exists(avatarFilePath))
                {
                    picAvatar.Image = Image.FromFile(avatarFilePath);
                }
                else
                {
                    picAvatar.Image = null;
                }
            }

            else
            {
                picAvatar.Image = null;
            }
        }
        private string SaveAvatar(string sourceFilePath, string studentID)
        {
            try
            {
                string folderPath = Path.Combine(Application.StartupPath, "Images");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileExtension = Path.GetExtension(sourceFilePath);
                string targetFilePath = Path.Combine(folderPath, $"{studentID}{fileExtension}");

                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"Không tìm thấy file: {sourceFilePath}");
                }

                File.Copy(sourceFilePath, targetFilePath, true);
                return $"{studentID}{fileExtension}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving avatar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void btnAddOrUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                // Lấy thông tin sinh viên từ form hoặc tạo mới
                var student = studentService.FindById(txtStudentID.Text) ?? new Student();

                // Cập nhật thông tin sinh viên
                student.StudentID = txtStudentID.Text;
                student.FullName = txtFullName.Text;
                student.AverageScore = double.Parse(txtAverageScore.Text);
                student.FacultyID = int.Parse(cmbFaculty.SelectedValue.ToString());

                // Kiểm tra nếu đã chọn file avatar
                if (!string.IsNullOrEmpty(avatarFilePath))
                {
                    string avatarFileName = SaveAvatar(avatarFilePath, txtStudentID.Text);
                    if (!string.IsNullOrEmpty(avatarFileName))
                    {
                        student.Avatar = avatarFileName;
                    }
                }

                studentService.InsertUpdate(student);

                BindGrid(studentService.GetAll());

                ClearData();
                avatarFilePath = string.Empty;
                MessageBox.Show("Thêm / Cập nhật dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearData()
        {
            txtStudentID.Clear();
            txtFullName.Clear();
            txtAverageScore.Clear();
            cmbFaculty.SelectedIndex = -1;
            picAvatar.Image = null;
            avatarFilePath = string.Empty;
        }

        private bool ValidateInput()
        {
            // Kiểm tra StudentID có trống không
            if (string.IsNullOrEmpty(txtStudentID.Text))
            {
                MessageBox.Show("Vui lòng nhập mã số sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentID.Focus();
                return false;
            }

            // Kiểm tra FullName có trống không
            if (string.IsNullOrEmpty(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            // Kiểm tra điểm trung bình (GPA) có hợp lệ không
            if (string.IsNullOrEmpty(txtAverageScore.Text) || !double.TryParse(txtAverageScore.Text, out double gpa) || gpa < 0 || gpa > 10)
            {
                MessageBox.Show("Vui lòng nhập điểm trung bình hợp lệ (từ 0 đến 10).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAverageScore.Focus();
                return false;
            }

            // Kiểm tra Faculty (Khoa) có được chọn không
            if (cmbFaculty.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn khoa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbFaculty.Focus();
                return false;
            }

            return true;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem StudentID có được nhập không
            if (string.IsNullOrEmpty(txtStudentID.Text))
            {
                MessageBox.Show("Vui lòng nhập mã số sinh viên để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Nếu StudentID là kiểu int, chuyển đổi giá trị từ TextBox sang int
            long studentID = long.Parse(txtStudentID.Text);
            var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này?",
                                                "Xác nhận",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    studentService.DeleteStudent(txtStudentID.Text);
                    BindGrid(studentService.GetAll());
                    ClearData();

                    MessageBox.Show("Xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa sinh viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void khoaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRegister formRegister = new frmRegister();
            var listStudents = studentService.GetAllHasNoMajor();
            BindGrid(listStudents);
            formRegister.ShowDialog();
        }
    }
}
