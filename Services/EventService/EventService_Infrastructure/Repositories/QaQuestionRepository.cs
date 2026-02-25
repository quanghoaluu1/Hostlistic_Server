using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories
{
    public class QaQuestionRepository : IQaQuestionRepository
    {
        private readonly EventServiceDbContext _context;
        public QaQuestionRepository(EventServiceDbContext context)
        {
            _context = context;
        }

        public async Task<QaQuestion> AddQaQuestionAsync(QaQuestion qaQuestion)
        {
            await _context.QaQuestions.AddAsync(qaQuestion);
            await _context.SaveChangesAsync();
            return qaQuestion;
        }

        public async Task<QaQuestion?> GetQaQuestionByIdAsync(Guid id)
        {
            return await _context.QaQuestions.FindAsync(id);
        }

        public async Task<IEnumerable<QaQuestion>> GetQaQuestionsBySessionIdAsync(Guid sessionId)
        {
            return await _context.QaQuestions
                .Where(q => q.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<QaQuestion>> GetQaQuestionsByUserIdAsync(Guid userId)
        {
            return await _context.QaQuestions
                .Where(q => q.UserId == userId)
                .ToListAsync();
        }

        public async Task<QaQuestion> UpdateQaQuestionAsync(QaQuestion qaQuestion)
        {
            _context.QaQuestions.Update(qaQuestion);
            await _context.SaveChangesAsync();
            return qaQuestion;
        }

        public async Task<bool> DeleteQaQuestionAsync(QaQuestion qaQuestion)
        {
            _context.QaQuestions.Remove(qaQuestion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QaQuestionVote(QaVote qaVote)
        {
            var existingVote = await _context.QaVotes
                .FirstOrDefaultAsync(v => v.UserId == qaVote.UserId && v.QaQuestionId == qaVote.QaQuestionId);
            if (existingVote != null)
            {
                _context.QaVotes.Remove(existingVote);
            }
            else
            {
                await _context.QaVotes.AddAsync(qaVote);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetQaQuestionVotes(Guid qaQuestionId)
        {
            return await _context.QaVotes
                .CountAsync(v => v.QaQuestionId == qaQuestionId);
        }
    }
}
