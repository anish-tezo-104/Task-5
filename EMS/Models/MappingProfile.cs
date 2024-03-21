using AutoMapper;

namespace EmployeeManagementSystem.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDetails>();
        // Add more mappings as needed
    }
}