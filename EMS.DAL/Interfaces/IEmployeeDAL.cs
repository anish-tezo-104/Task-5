using EMS.DAL.DBO;
using EMS.DAL.DTO;

namespace EMS.DAL.Interfaces;

public interface IEmployeeDAL
{
    public bool Insert(Employee employee);
    public List<EmployeeDetails>? RetrieveAll();
    public bool Update(string empNo, Employee updatedEmployee);
    public bool Delete(string empNo);
    public List<EmployeeDetails>? Filter(EmployeeFilters? filters);
    public int Count();
}

