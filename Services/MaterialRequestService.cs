using Microsoft.EntityFrameworkCore;
using CPMS_Web.Data;
using CPMS_Web.Models;
using CPMS_Web.Models.ViewModels;

namespace CPMS_Web.Services
{
    public class MaterialRequestService : IMaterialRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;

        public MaterialRequestService(ApplicationDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<List<MaterialRequest>> GetRequestsAsync(string status = "", string? userId = null)
        {
            var query = _context.MaterialRequests
                .Include(r => r.Requester)
                .Include(r => r.Approver)
                .Include(r => r.MaterialRequestDetails)
                    .ThenInclude(d => d.SparePart)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(r => r.RequesterId == userId);
            }

            return await query.OrderByDescending(r => r.RequestDate).ToListAsync();
        }

        public async Task<MaterialRequest?> GetRequestAsync(int id)
        {
            return await _context.MaterialRequests
                .Include(r => r.Requester)
                .Include(r => r.Approver)
                .Include(r => r.MaterialRequestDetails)
                    .ThenInclude(d => d.SparePart)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> CreateRequestAsync(MaterialRequest request, List<MaterialRequestDetail> details)
        {
            try
            {
                request.RequestDate = DateTime.Now;
                request.CreatedDate = DateTime.Now;
                request.Status = "PENDING";
                request.RequestNo = await GenerateRequestNoAsync();

                _context.MaterialRequests.Add(request);
                await _context.SaveChangesAsync();

                // 新增明細
                foreach (var detail in details)
                {
                    detail.RequestId = request.Id;
                    _context.MaterialRequestDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveRequestAsync(int requestId, string approverId, List<int> approvedQuantities)
        {
            try
            {
                var request = await _context.MaterialRequests
                    .Include(r => r.MaterialRequestDetails)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null || request.Status != "PENDING") return false;

                // 更新核准數量
                for (int i = 0; i < request.MaterialRequestDetails.Count; i++)
                {
                    if (i < approvedQuantities.Count)
                    {
                        request.MaterialRequestDetails.ElementAt(i).ApprovedQuantity = approvedQuantities[i];
                    }
                }

                request.Status = "APPROVED";
                request.ApproverId = approverId;
                request.ApprovalDate = DateTime.Now; // 修正：從 ApproveDate 改為 ApprovalDate

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectRequestAsync(int requestId, string rejectorId, string reason)
        {
            try
            {
                var request = await _context.MaterialRequests.FindAsync(requestId);
                if (request == null || request.Status != "PENDING") return false;

                request.Status = "REJECTED";
                request.ApproverId = rejectorId;
                request.ApprovalDate = DateTime.Now; // 修正：從 ApproveDate 改為 ApprovalDate
                request.RejectReason = reason;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IssueRequestAsync(int requestId, string issuerId)
        {
            try
            {
                var request = await _context.MaterialRequests
                    .Include(r => r.MaterialRequestDetails)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null || request.Status != "APPROVED") return false;

                // 檢查庫存是否足夠
                foreach (var detail in request.MaterialRequestDetails)
                {
                    var sparePart = await _inventoryService.GetSparePartAsync(detail.SparePartNo);
                    if (sparePart == null || sparePart.Quantity < detail.ApprovedQuantity)
                    {
                        return false;
                    }
                }

                // 扣減庫存
                foreach (var detail in request.MaterialRequestDetails)
                {
                    var sparePart = await _inventoryService.GetSparePartAsync(detail.SparePartNo);
                    if (sparePart != null)
                    {
                        var newQuantity = sparePart.Quantity - detail.ApprovedQuantity;
                        await _inventoryService.AdjustQuantityAsync(
                            detail.SparePartNo,
                            newQuantity,
                            $"領料單出庫: {request.RequestNo}",
                            issuerId
                        );

                        detail.IssuedQuantity = detail.ApprovedQuantity;
                    }
                }

                request.Status = "ISSUED";
                request.IssueDate = DateTime.Now;
                request.IssuerId = issuerId;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<MaterialRequest>> GetPendingRequestsAsync()
        {
            return await _context.MaterialRequests
                .Include(r => r.Requester)
                .Include(r => r.MaterialRequestDetails)
                    .ThenInclude(d => d.SparePart)
                .Where(r => r.Status == "PENDING")
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<List<MaterialRequest>> GetUserRequestsAsync(string userId)
        {
            return await _context.MaterialRequests
                .Include(r => r.Requester)
                .Include(r => r.Approver)
                .Include(r => r.MaterialRequestDetails)
                    .ThenInclude(d => d.SparePart)
                .Where(r => r.RequesterId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<MaterialRequestReportViewModel> GetRequestReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new MaterialRequestReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // 取得期間內的領料單
            var requests = await _context.MaterialRequests
                .Include(r => r.Requester)
                .Include(r => r.MaterialRequestDetails)
                    .ThenInclude(d => d.SparePart)
                .Where(r => r.RequestDate >= startDate && r.RequestDate <= endDate)
                .ToListAsync();

            report.TotalRequests = requests.Count;
            report.ApprovedRequests = requests.Count(r => r.Status == "APPROVED");
            report.RejectedRequests = requests.Count(r => r.Status == "REJECTED");
            report.IssuedRequests = requests.Count(r => r.Status == "ISSUED");

            // 計算各部門的領料統計
            report.DepartmentStatistics = requests
                .GroupBy(r => r.Department)
                .Select(g => new DepartmentRequestStat
                {
                    Department = g.Key,
                    RequestCount = g.Count(),
                    ApprovedCount = g.Count(r => r.Status == "APPROVED"),
                    RejectedCount = g.Count(r => r.Status == "REJECTED"),
                    IssuedCount = g.Count(r => r.Status == "ISSUED")
                })
                .ToList();

            // 計算各類別的領料統計
            report.CategoryStatistics = requests
                .SelectMany(r => r.MaterialRequestDetails)
                .GroupBy(d => d.SparePart.Category)
                .Select(g => new CategoryRequestStat
                {
                    Category = g.Key,
                    RequestedQuantity = g.Sum(d => d.RequestedQuantity),
                    ApprovedQuantity = g.Sum(d => d.ApprovedQuantity),
                    IssuedQuantity = g.Sum(d => d.IssuedQuantity)
                })
                .ToList();

            return report;
        }

        private async Task<string> GenerateRequestNoAsync()
        {
            var today = DateTime.Now;
            var prefix = $"MR{today:yyyyMMdd}";

            var lastRequest = await _context.MaterialRequests
                .Where(r => r.RequestNo.StartsWith(prefix))
                .OrderByDescending(r => r.RequestNo)
                .FirstOrDefaultAsync();

            if (lastRequest == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastRequest.RequestNo.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }
    }
}