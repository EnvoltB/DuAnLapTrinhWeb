using GearDTK.Data;
using GearDTK.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GearDTK.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly ApplicationDbContext _context;

    public BannerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Banners>> GetActiveBannerAsync()
    {
        return await _context.Banners
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<Banners>> GetMainSliderBannerAsync()
    {
        return await _context.Banners
            .Where(b => b.IsActive && b.IsMainSlider)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Banners?> GetByIdAsync(int id)
    {
        return await _context.Banners.FindAsync(id);
    }

    public async Task AddAsync(Banners Banner)
    {
        await _context.Banners.AddAsync(Banner);
    }

    public async Task UpdateAsync(Banners Banner)
    {
        _context.Banners.Update(Banner);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Banners Banner)
    {
        _context.Banners.Remove(Banner);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}