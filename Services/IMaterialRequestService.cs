using CPMS_Web.Models;
using CPMS_Web.Models.ViewModels;

namespace CPMS_Web.Services
{
    public interface IMaterialRequestService
    {
        Task<List<MaterialRequest>> GetRequestsAsync(string status = "", string? userId = null);
        Task<MaterialRequest?> GetRequestAsync(int id);
        Task<bool> CreateRequestAsync(MaterialRequest request, List<MaterialRequestDetail> details);
        Task<bool> ApproveRequestAsync(int requestId, string approverId, List<int> approvedQuantities);
        Task<bool> RejectRequestAsync(int requestId, string rejectorId, string reason);
        Task<bool> IssueRequestAsync(int requestId, string issuerId);
        Task<List<MaterialRequest>> GetPendingRequestsAsync();
        Task<List<MaterialRequest>> GetUserRequestsAsync(string userId);
        Task<MaterialRequestReportViewModel> GetRequestReportAsync(DateTime startDate, DateTime endDate);
    }
} 