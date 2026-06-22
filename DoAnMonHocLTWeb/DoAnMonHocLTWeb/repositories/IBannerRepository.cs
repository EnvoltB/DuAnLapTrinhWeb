using GearDTK.Models;
using System.Reflection;

namespace GearDTK.Repositories;

public interface IBannerRepository
{
    Task<List<Banners>> GetActiveBannerAsync();
    Task<List<Banners>> GetMainSliderBannerAsync();
    Task<Banners?> GetByIdAsync(int id);
    Task AddAsync(Banners Banner);
    Task UpdateAsync(Banners Banner);
    Task DeleteAsync(Banners Banner);
    Task SaveChangesAsync();
}