using Lab05.BUS;
using Lab05.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab05.GUI
{
    public partial class frmRegister : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private readonly MajorService majorService = new MajorService();
        public frmRegister()
        {
            InitializeComponent();
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            try
            {
                var listFacultys = facultyService.GetAll();
                FillFalcultyCombobox(listFacultys);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }

        private void cmbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                var listMajor = majorService.GetAllByFaculty(selectedFaculty.FacultyID);
                FillMajorCombobox(listMajor);
                var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                BindGrid(listStudents);
            }
        }
        private void FillMajorCombobox(List<Major> listMajor)
        {
            // Xóa tất cả các mục trong ComboBox trước khi thêm mới
            cmbMajor.Items.Clear();

            // Duyệt qua danh sách các chuyên ngành và thêm chúng vào ComboBox
            foreach (var major in listMajor)
            {
                cmbMajor.Items.Add(major); // Thêm đối tượng Major trực tiếp vào ComboBox
            }

            // Đặt DisplayMember và ValueMember
            cmbMajor.DisplayMember = "Name";
            cmbMajor.ValueMember = "MajorID";

            // Đặt mục được chọn đầu tiên (nếu có)
            if (listMajor.Count > 0)
            {
                cmbMajor.SelectedIndex = 0;
            }
            else
            {
                cmbMajor.SelectedIndex = -1; // Không chọn mục nào nếu không có chuyên ngành
            }
        }

        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[1].Value = item.StudentID;
                dgvStudent.Rows[index].Cells[2].Value = item.FullName;

                if (item.Faculty != null)
                    dgvStudent.Rows[index].Cells[3].Value = item.Faculty.FacultyName;

                dgvStudent.Rows[index].Cells[4].Value = item.AverageScore + "";

                if (item.MajorID != null)
                    dgvStudent.Rows[index].Cells[5].Value = item.Major.Name + "";
            }
        }
        //private void ClearData()
        //{
        //    cmbFaculty.SelectedIndex = -1;
        //    cmbMajor.SelectedIndex = -1;
        //}

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Kiểm tra nếu không có chuyên ngành nào được chọn
            if (cmbMajor.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn chuyên ngành.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy đối tượng Major từ ComboBox
            var selectedMajor = cmbMajor.SelectedItem as Major;
            if (selectedMajor == null)
            {
                MessageBox.Show("Không thể lấy thông tin chuyên ngành. Vui lòng thử lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lấy MajorID từ đối tượng Major đã chọn
            int selectedMajorID = selectedMajor.MajorID;

            // Duyệt qua tất cả các hàng của DataGridView để tìm các sinh viên được chọn
            foreach (DataGridViewRow row in dgvStudent.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells[0].Value);
                if (isChecked)
                {
                    string studentID = row.Cells[1].Value.ToString();
                    studentService.UpdateStudentMajor(studentID, selectedMajorID);
                }
            }
            MessageBox.Show("Đăng ký chuyên ngành thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Làm mới lại danh sách sinh viên không có chuyên ngành
            Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                var listStudents = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                BindGrid(listStudents);
            }
        }
    }
}
