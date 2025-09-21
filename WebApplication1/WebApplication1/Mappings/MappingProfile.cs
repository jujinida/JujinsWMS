using AutoMapper;
using WebApplication1.Models;
using WebApplication1.Controllers;

namespace WebApplication1.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Employee 관련 매핑
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate.HasValue ? src.BirthDate.Value.ToString("yyyy-MM-dd") : ""))
                .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate.HasValue ? src.HireDate.Value.ToString("yyyy-MM-dd") : ""))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? ""))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address ?? ""))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position ?? ""))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? ""))
                .ForMember(dest => dest.ProfileUrl, opt => opt.MapFrom(src => src.ProfileUrl ?? ""));

            CreateMap<EmployeeRegistrationRequest, Employee>()
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => DateTime.Parse(src.BirthDate)))
                .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => DateTime.Parse(src.HireDate)));

            // AttendanceRecord 관련 매핑
            CreateMap<AttendanceRecord, AttendanceRecordDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.EmployeeName))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.Employee.DepartmentId))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Employee.Position ?? ""))
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.CheckInTime.HasValue ? src.CheckInTime.Value.ToString("HH:mm") : ""))
                .ForMember(dest => dest.CheckOutTime, opt => opt.MapFrom(src => src.CheckOutTime.HasValue ? src.CheckOutTime.Value.ToString("HH:mm") : ""))
                .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => src.WorkHours ?? 0))
                .ForMember(dest => dest.WorkMinutes, opt => opt.MapFrom(src => src.WorkMinutes ?? 0))
                .ForMember(dest => dest.LateCount, opt => opt.MapFrom(src => 0)) // 기본값
                .ForMember(dest => dest.AbsentWithoutLeaveCount, opt => opt.MapFrom(src => 0)); // 기본값

            // MonthlyAttendanceRecord 관련 매핑
            CreateMap<AttendanceRecord, MonthlyAttendanceRecordDto>()
                .ForMember(dest => dest.RecordDate, opt => opt.MapFrom(src => src.RecordDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.CheckInTime.HasValue ? src.CheckInTime.Value.ToString("HH:mm") : ""))
                .ForMember(dest => dest.CheckOutTime, opt => opt.MapFrom(src => src.CheckOutTime.HasValue ? src.CheckOutTime.Value.ToString("HH:mm") : ""))
                .ForMember(dest => dest.WorkHours, opt => opt.MapFrom(src => src.WorkHours ?? 0))
                .ForMember(dest => dest.WorkMinutes, opt => opt.MapFrom(src => src.WorkMinutes ?? 0))
                .ForMember(dest => dest.AttendanceStatus, opt => opt.MapFrom(src => 
                    src.CheckInTime == null && src.CheckOutTime == null ? "결근" :
                    src.CheckInTime.HasValue && src.CheckInTime.Value.TimeOfDay > TimeSpan.Parse("09:01:00") ? "지각" : "정상"));

            // VacationRequest 관련 매핑
            CreateMap<VacationRequest, VacationRequestDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.EmployeeName))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.Employee.DepartmentId))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Employee.Position ?? ""))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason ?? ""))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToString("yyyy-MM-dd") : ""))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToString("yyyy-MM-dd") : ""))
                .ForMember(dest => dest.VacationDays, opt => opt.MapFrom(src => src.VacationDays ?? 0))
                .ForMember(dest => dest.IsHalfDay, opt => opt.MapFrom(src => src.IsHalfDay ?? false))
                .ForMember(dest => dest.RemainingVacationDays, opt => opt.MapFrom(src => src.Employee.RemainingVacationDays ?? 0))
                .ForMember(dest => dest.TotalVacationDays, opt => opt.MapFrom(src => src.Employee.TotalVacationDays ?? 0))
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus ?? ""));

            // Payroll 관련 매핑
            CreateMap<Payroll, PayrollDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.EmployeeName))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.Employee.DepartmentId))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Employee.Position ?? ""))
                .ForMember(dest => dest.GrossPay, opt => opt.MapFrom(src => src.GrossPay ?? 0))
                .ForMember(dest => dest.Allowance, opt => opt.MapFrom(src => src.Allowance ?? 0))
                .ForMember(dest => dest.Deductions, opt => opt.MapFrom(src => src.Deductions ?? 0))
                .ForMember(dest => dest.NetPay, opt => opt.MapFrom(src => src.NetPay ?? 0))
                .ForMember(dest => dest.PaymentMonth, opt => opt.MapFrom(src => src.PaymentMonth ?? ""))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate.HasValue ? src.PaymentDate.Value.ToString("yyyy-MM-dd") : ""))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus ?? ""));

            // Product 관련 매핑
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName ?? ""))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? 0))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity ?? 0))
                .ForMember(dest => dest.SafetyStock, opt => opt.MapFrom(src => src.SafetyStock ?? 0))
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => 0)) // 기본값
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => "미지정")) // 기본값
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));

            CreateMap<Product, InventoryStatusDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName ?? ""))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? 0))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity ?? 0))
                .ForMember(dest => dest.SafetyStock, opt => opt.MapFrom(src => src.SafetyStock ?? 0))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));

            CreateMap<ProductRegistrationRequest, Product>();

            CreateMap<ProductUpdateRequest, Product>();

            // ProductLocation 관련 매핑
            CreateMap<ProductLocation, WarehouseStockDto>();

            // InventoryLog 관련 매핑
            CreateMap<InventoryLog, ReceivingHistoryDto>()
                .ForMember(dest => dest.LogDate, opt => opt.MapFrom(src => src.LogDate))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));

            CreateMap<InventoryLog, ShippingHistoryDto>()
                .ForMember(dest => dest.LogDate, opt => opt.MapFrom(src => src.LogDate))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));
        }
    }
}
