using EMS.DAL.DBO;

namespace EMS.DAL.Interfaces;

public interface IDropdownDAL
{
    List<Dropdown>? GetLocationsList();
    List<Dropdown>? GetDepartmentsList();
    List<Dropdown>? GetManagersList();
    List<Dropdown>? GetProjectsList();
    List<Dropdown>? GetStatusList();
    List<Role>? GetRolesList();
}