using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Mapster;

namespace BookingService_Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IQrCodeService _qrCodeService;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TicketService(
        ITicketRepository ticketRepository,
        IQrCodeService qrCodeService,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _qrCodeService = qrCodeService;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<TicketDto>> GetTicketByIdAsync(Guid ticketId)
    {
        var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticket == null)
            return ApiResponse<TicketDto>.Fail(404, "Ticket not found");

        var ticketDto = ticket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(200, "Ticket retrieved successfully", ticketDto);
    }

    public async Task<ApiResponse<TicketDto>> GetTicketByCodeAsync(string ticketCode)
    {
        var ticket = await _ticketRepository.GetTicketByCodeAsync(ticketCode);
        if (ticket == null)
            return ApiResponse<TicketDto>.Fail(404, "Ticket not found");

        var ticketDto = ticket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(200, "Ticket retrieved successfully", ticketDto);
    }

    public async Task<ApiResponse<GuestLiveAccessTicketDto>> ValidateGuestLiveAccessAsync(ValidateGuestLiveAccessRequest request)
    {
        if (request.EventId == Guid.Empty)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(400, "Event id is required.");

        if (string.IsNullOrWhiteSpace(request.TicketCode))
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(400, "Ticket code is required.");

        var normalizedTicketCode = request.TicketCode.Trim().ToUpperInvariant();
        var ticket = await _ticketRepository.GetTicketByCodeAsync(normalizedTicketCode);
        if (ticket == null)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(404, "Ticket not found.");

        if (ticket.Order.EventId != request.EventId)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(403, "This ticket does not belong to the requested event.");

        if (ticket.Order.Status != OrderStatus.Confirmed)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(403, "This ticket is not eligible for live access.");

        var response = new GuestLiveAccessTicketDto
        {
            TicketId = ticket.Id,
            EventId = ticket.Order.EventId,
            OrderId = ticket.OrderId,
            TicketCode = ticket.TicketCode,
            HolderName = ticket.HolderName,
            HolderEmail = ticket.HolderEmail,
            IsUsed = ticket.IsUsed
        };

        return ApiResponse<GuestLiveAccessTicketDto>.Success(200, "Ticket is valid for guest live access.", response);
    }

    public async Task<ApiResponse<IEnumerable<TicketDto>>> GetTicketsByOrderIdAsync(Guid orderId)
    {
        var tickets = await _ticketRepository.GetTicketsByOrderIdAsync(orderId);
        var ticketDtos = tickets.Adapt<IEnumerable<TicketDto>>();
        return ApiResponse<IEnumerable<TicketDto>>.Success(200, "Tickets retrieved successfully", ticketDtos);
    }

    public async Task<ApiResponse<TicketDto>> CreateTicketAsync(CreateTicketRequest request)
    {
        var ticket = request.Adapt<Ticket>();
        ticket.TicketTypeName = request.TicketTypeName;
        ticket.EventName = request.EventName;
        ticket.HolderName = request.HolderName;
        ticket.HolderEmail = request.HolderEmail;
        ticket.HolderPhone = request.HolderPhone;

        await _ticketRepository.AddTicketAsync(ticket); // sets ticket.Id and ticket.TicketCode
        ticket.QrCodeUrl = await _qrCodeService.GenerateQrPayloadAsync(ticket.Id, request.EventId);
        await _ticketRepository.SaveChangesAsync();

        var ticketDto = ticket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(201, "Ticket created successfully", ticketDto);
    }

    public async Task<ApiResponse<TicketDto>> UpdateTicketAsync(Guid ticketId, UpdateTicketRequest request)
    {
        var existingTicket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (existingTicket == null)
            return ApiResponse<TicketDto>.Fail(404, "Ticket not found");

        // Update properties
        existingTicket.IsUsed = request.IsUsed;

        await _ticketRepository.UpdateTicketAsync(existingTicket);
        await _ticketRepository.SaveChangesAsync();

        var ticketDto = existingTicket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(200, "Ticket updated successfully", ticketDto);
    }

    public async Task<ApiResponse<int>> RegenerateAllQrCodesAsync()
    {
        var tickets = (await _ticketRepository.GetAllWithOrderAsync()).ToList();
        foreach (var ticket in tickets)
            ticket.QrCodeUrl = await _qrCodeService.GenerateQrPayloadAsync(ticket.Id, ticket.Order.EventId);

        await _ticketRepository.SaveChangesAsync();
        return ApiResponse<int>.Success(200, "QR codes regenerated", tickets.Count);
    }

    public async Task<ApiResponse<bool>> DeleteTicketAsync(Guid ticketId)
    {
        var exists = await _ticketRepository.TicketExistsAsync(ticketId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Ticket not found");

        var deleted = await _ticketRepository.DeleteTicketAsync(ticketId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete ticket");

        await _ticketRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Ticket deleted successfully", true);
    }

    public async Task<ApiResponse<TicketDto>> ProcessPostponementDecisionAsync(Guid ticketId, PostponementDecision decision, Guid callerUserId)
    {
        var existingTicket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (existingTicket == null)
            return ApiResponse<TicketDto>.Fail(404, "Ticket not found");

        // GAP-B4: Ownership check — only the ticket's buyer may submit a decision
        if (existingTicket.Order.UserId != callerUserId)
            return ApiResponse<TicketDto>.Fail(403, "You are not authorized to submit a decision for this ticket.");

        if (existingTicket.PostponementStatus != PostponementStatus.PendingDecision)
            return ApiResponse<TicketDto>.Fail(400, "Ticket is not pending a postponement decision");

        existingTicket.PostponementStatus = decision == PostponementDecision.Accept
            ? PostponementStatus.Accepted
            : PostponementStatus.RefundRequested;

        await _ticketRepository.UpdateTicketAsync(existingTicket);
        await _ticketRepository.SaveChangesAsync();

        var ticketDto = existingTicket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(200, "Postponement decision processed successfully", ticketDto);
    }

    public async Task<ApiResponse<int>> ProcessRefundsForPostponedEventAsync(Guid eventId)
    {
        var ticketsToRefund = await _ticketRepository
            .GetTicketsByEventAndPostponementStatusAsync(eventId, PostponementStatus.RefundRequested);

        if (!ticketsToRefund.Any())
            return ApiResponse<int>.Success(200, "No refunds to process.", 0);

        int processedCount = 0;
        int skippedCount   = 0;

        foreach (var ticket in ticketsToRefund)
        {
            // ── Step a: Resolve refund amount from OrderDetails ─────────────────
            var orderDetail = ticket.Order.OrderDetails
                .FirstOrDefault(od => od.TicketTypeId == ticket.TicketTypeId);
            decimal refundAmount = orderDetail?.UnitPrice ?? 0m;

            if (refundAmount <= 0m)
            {
                // Free ticket fast-path: no wallet involved — safe to commit in isolation
                await using var freeTx = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    ticket.PostponementStatus = PostponementStatus.Refunded;
                    await _ticketRepository.UpdateTicketAsync(ticket);
                    await _unitOfWork.SaveChangesAsync();
                    await freeTx.CommitAsync();
                    processedCount++;
                }
                catch
                {
                    await freeTx.RollbackAsync();
                    skippedCount++;
                }
                continue;
            }

            // ── Step b: Retrieve the user's wallet ──────────────────────────────
            var wallet = await _walletRepository.GetWalletByUserIdAsync(ticket.Order.UserId);
            if (wallet == null)
            {
                // Cannot refund — no wallet found. Skip and allow retry later.
                skippedCount++;
                continue;
            }

            // ── Atomic block: wallet credit + ledger + ticket status ────────────
            // All three mutations share one DB transaction. If any step throws,
            // the entire transaction rolls back — ticket stays RefundRequested
            // and will be retried on the next admin call (no double-credit risk).
            await using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Step b: Increase wallet balance
                wallet.Balance += refundAmount;
                await _walletRepository.UpdateWalletAsync(wallet);

                // Step c & d: Write Transaction ledger record
                var ledgerEntry = new Transaction
                {
                    WalletId      = wallet.Id,
                    Type          = TransactionType.Refund,
                    Amount        = refundAmount,
                    PlatformFee   = 0m,
                    NetAmount     = refundAmount,
                    BalanceAfter  = wallet.Balance,
                    ReferenceId   = ticket.OrderId,
                    ReferenceType = "Order",
                    Status        = TransactionStatus.Completed,
                    Description   = $"Refund for postponed event — ticket {ticket.TicketCode}",
                };
                await _transactionRepository.AddAsync(ledgerEntry);

                // Step e: Soft-mark ticket as Refunded (NO hard delete)
                ticket.PostponementStatus = PostponementStatus.Refunded;
                await _ticketRepository.UpdateTicketAsync(ticket);

                // Single atomic commit for wallet + ledger + ticket
                await _unitOfWork.SaveChangesAsync();
                await tx.CommitAsync();

                processedCount++;
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                skippedCount++;
            }
        }

        var message = skippedCount > 0
            ? $"Processed {processedCount} refund(s). Skipped {skippedCount} ticket(s) — wallet not found or error during processing."
            : $"Successfully processed {processedCount} refund(s) and updated ticket statuses to Refunded.";

        return ApiResponse<int>.Success(200, message, processedCount);
    }
}
