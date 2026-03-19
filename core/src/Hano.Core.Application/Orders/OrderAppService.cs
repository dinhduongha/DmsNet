using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Orders;
using Hano.Core.Application.Mappers;
using Hano.Core.Domain.Shared.Enums;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Hano.Core.Application.Orders;

public class OrderAppService : HanoCoreAppServiceBase, IOrderAppService
{
    private readonly IRepository<Order, Guid> _orderRepo;
    private readonly IRepository<Visit, Guid> _visitRepo;
    private readonly IRepository<Sku, Guid> _skuRepo;
    private readonly IRepository<PriceList, Guid> _priceRepo;
    private readonly IRepository<Outlet, Guid> _outletRepo;
    private readonly IRepository<VehicleStock, Guid> _vsRepo;

    public OrderAppService(IRepository<Order, Guid> orderRepo, IRepository<Visit, Guid> visitRepo, IRepository<Sku, Guid> skuRepo, IRepository<PriceList, Guid> priceRepo, IRepository<Outlet, Guid> outletRepo, IRepository<VehicleStock, Guid> vsRepo)
    { _orderRepo = orderRepo; _visitRepo = visitRepo; _skuRepo = skuRepo; _priceRepo = priceRepo; _outletRepo = outletRepo; _vsRepo = vsRepo; }

    public async Task<OrderDto> CreateAsync(CreateOrderDto input)
    {
        var existing = await _orderRepo.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (existing != null) return existing.ToDto((await _outletRepo.GetAsync(existing.OutletId)).Name);

        var visit = await _visitRepo.GetAsync(input.VisitId);
        if (visit.Status != VisitStatus.InProgress) throw new UserFriendlyException("Visit phải InProgress.");

        var order = new Order { Id = input.Id, VisitId = input.VisitId, OutletId = input.OutletId, UserId = CurrentUserId, SessionId = visit.SessionId, OrderType = input.OrderType, Status = OrderStatus.Submitted, Notes = input.Notes, PaymentMethod = input.PaymentMethod, PodPhotoId = input.PodPhotoId, ClientCreatedAt = input.Timestamp };

        decimal total = 0;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var item in input.Items)
        {
            var sku = await _skuRepo.GetAsync(item.SkuId);
            var price = await _priceRepo.FirstOrDefaultAsync(x => x.SkuId == item.SkuId && x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today));
            var unitPrice = price?.PromoPrice ?? price?.UnitPrice ?? 0;
            var lineTotal = unitPrice * item.Quantity;
            total += lineTotal;
            order.Lines.Add(new OrderLine { Id = GuidGenerator.Create(), OrderId = order.Id, SkuId = item.SkuId, SkuCode = sku.Code, SkuName = sku.Name, Quantity = item.Quantity, Unit = item.Unit, UnitPrice = unitPrice, LineTotal = lineTotal });

            if (input.OrderType == OrderType.Dsd)
            {
                var stock = await _vsRepo.FirstOrDefaultAsync(x => x.SessionId == visit.SessionId && x.SkuId == item.SkuId);
                if (stock != null) { stock.SoldQty += item.Quantity; stock.CurrentQty = stock.OpeningQty - stock.SoldQty + stock.ReturnQty; await _vsRepo.UpdateAsync(stock); }
            }
        }
        order.TotalAmount = total; order.TotalItems = order.Lines.Count;
        order.OrderCode = $"{(input.OrderType == OrderType.Dsr ? "DSR" : "DSD")}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        await _orderRepo.InsertAsync(order);
        visit.ActivitiesCount++; await _visitRepo.UpdateAsync(visit);
        return order.ToDto((await _outletRepo.GetAsync(order.OutletId)).Name);
    }

    public async Task<OrderDto> GetAsync(Guid id)
    { var o = await _orderRepo.GetAsync(id, includeDetails: true); return o.ToDto((await _outletRepo.GetAsync(o.OutletId)).Name); }

    public async Task<PagedResultDto<OrderDto>> GetListAsync(OrderFilterDto input)
    {
        var q = (await _orderRepo.GetQueryableAsync()).Where(x => x.UserId == CurrentUserId);
        if (input.FromDate.HasValue) q = q.Where(x => x.ClientCreatedAt >= input.FromDate);
        if (input.ToDate.HasValue) q = q.Where(x => x.ClientCreatedAt <= input.ToDate);
        if (input.OutletId.HasValue) q = q.Where(x => x.OutletId == input.OutletId);
        if (input.Status.HasValue) q = q.Where(x => x.Status == input.Status);
        if (input.OrderType.HasValue) q = q.Where(x => x.OrderType == input.OrderType);
        var total = q.Count();
        var items = q.OrderByDescending(x => x.ClientCreatedAt).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        var outletIds = items.Select(o => o.OutletId).Distinct();
        var outlets = (await _outletRepo.GetListAsync(x => outletIds.Contains(x.Id))).ToDictionary(o => o.Id);
        return new PagedResultDto<OrderDto>(total, items.Select(o => o.ToDto(outlets.GetValueOrDefault(o.OutletId)?.Name ?? "")).ToList());
    }

    public async Task<OrderDto> SaveDraftAsync(CreateOrderDto input)
    {
        input.Id = input.Id == Guid.Empty ? GuidGenerator.Create() : input.Id;
        // Reuse CreateAsync logic with Draft status
        var dto = await CreateAsync(input);
        var order = await _orderRepo.GetAsync(dto.Id);
        order.Status = OrderStatus.Draft; await _orderRepo.UpdateAsync(order);
        dto.Status = OrderStatus.Draft;
        return dto;
    }
}
