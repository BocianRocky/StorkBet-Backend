using Domain.Interfaces;
using DomainTeam = Domain.Entities.Team;
using InfraTeam = Infrastructure.Data.Team;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;


namespace Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _context;

        public TeamRepository(AppDbContext context)
        {
            _context = context;
        }

        // Pobranie drużyny po nazwie
        public async Task<DomainTeam?> GetByNameAsync(string name)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamName == name);

            if (team == null) return null;

            // Mapowanie na encję domenową
            return new DomainTeam
            {
                Id = team.Id,
                TeamName = team.TeamName,
                SportId = team.SportId
            };
        }

        // Dodanie drużyny do bazy
        public async Task AddAsync(DomainTeam team)
        {
            // Mapowanie Domain -> Infrastructure
            var newTeam = new InfraTeam
            {
                TeamName = team.TeamName,
                SportId = team.SportId
            };

            await _context.Teams.AddAsync(newTeam);
        }

        // Zapis zmian w bazie
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Opcjonalnie: pobranie wszystkich drużyn
        public async Task<IEnumerable<DomainTeam>> GetAllAsync()
        {
            var teams = await _context.Teams.ToListAsync();
            var domainTeams = new List<DomainTeam>();

            foreach (var t in teams)
            {
                domainTeams.Add(new DomainTeam
                {
                    Id = t.Id,
                    TeamName = t.TeamName,
                    SportId = t.SportId
                });
            }

            return domainTeams;
        }
    }
}
