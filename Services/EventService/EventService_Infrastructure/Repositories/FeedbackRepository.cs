using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly EventServiceDbContext _context;
        public FeedbackRepository(EventServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> AddFeddbackAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(Guid id)
        {
            return await _context.Feedbacks.FindAsync(id);
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
        {
            return await _context.Feedbacks.ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByEventIdAsync(Guid eventId)
        {
            return await _context.Feedbacks.Where(f => f.EventId == eventId).ToListAsync();
        }
        public async Task<IEnumerable<Feedback>> GetFeedbacksBySessionAsync(Guid sessionId)
        {
            return await _context.Feedbacks.Where(f => f.SessionId == sessionId).ToListAsync();
        }

        public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
        {
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<bool> DeleteFeedbackAsync(Guid id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return false;
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
