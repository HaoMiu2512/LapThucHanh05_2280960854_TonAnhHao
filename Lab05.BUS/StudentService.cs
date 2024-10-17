using Lab05.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab05.BUS
{
    public class StudentService
    {
        public List<Student> GetAll()
        {
            StudentModel context = new StudentModel();
            return context.Students.ToList();
        }

        public List<Student> GetAllHasNoMajor()
        {
            StudentModel context = new StudentModel();
            return context.Students.Where(p => p.MajorID == null).ToList();
        }

        public List<Student> GetAllHasNoMajor(int facultyID)
        {
            StudentModel context = new StudentModel();
            return context.Students.Where(p => p.MajorID == null && p.FacultyID == facultyID).ToList();
        }

        public Student FindById(string studentId)
        {
            StudentModel context = new StudentModel();
            return context.Students.FirstOrDefault(p => p.StudentID == studentId);
        }

        public void InsertUpdate(Student s)
        {
            StudentModel context = new StudentModel();
            context.Students.AddOrUpdate(s);
            context.SaveChanges();
        }

        public void DeleteStudent(string studentId)
        {
            using (var context = new StudentModel())
            {
                var student = context.Students.FirstOrDefault(p => p.StudentID == studentId);

                if (student != null)
                {
                    context.Students.Remove(student);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Không tìm thấy sinh viên ");
                }
            }
        }
        public void UpdateStudentMajor(string studentID, int majorID)
        {
            using (var context = new StudentModel())
            {
                // Tìm sinh viên theo ID
                var student = context.Students.FirstOrDefault(s => s.StudentID == studentID);

                if (student != null)
                {
                    // Cập nhật MajorID cho sinh viên
                    student.MajorID = majorID;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Không tìm thấy sinh viên với mã số: " + studentID);
                }
            }
        }

    }
}
