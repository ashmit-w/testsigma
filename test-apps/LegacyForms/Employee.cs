using System;

namespace LegacyForms
{
    public class Employee
    {
        public string Id;
        public string Name;
        public string Department;
        public string Status;
        public DateTime JoinDate;
        public bool FullTime;
        public string Shift;
        public int SalaryBand;

        public Employee(string id, string name, string department, string status,
            DateTime joinDate, bool fullTime, string shift, int salaryBand)
        {
            Id = id;
            Name = name;
            Department = department;
            Status = status;
            JoinDate = joinDate;
            FullTime = fullTime;
            Shift = shift;
            SalaryBand = salaryBand;
        }

        public override string ToString()
        {
            return Id + " - " + Name;
        }
    }
}
