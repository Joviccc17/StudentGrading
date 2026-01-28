using Microsoft.EntityFrameworkCore;
using StudentGrading.Models;
using System.Collections.Generic;

namespace StudentGrading.Data
{
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }

        public DbSet<ExamResult> ExamResults { get; set; }


    }
}
