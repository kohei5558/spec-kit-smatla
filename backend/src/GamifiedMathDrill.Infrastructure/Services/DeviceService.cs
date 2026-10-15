using System.Security.Cryptography;
using System.Text;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using System.Buffers.Text;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// 子供用端末の登録管理サービス実装
/// </summary>
public class DeviceService : IDeviceService
{
    // 最終利用日時の更新間隔（毎リクエストの書き込みを避ける）
    private static readonly TimeSpan LastUsedUpdateInterval = TimeSpan.FromMinutes(1);

    private readonly ApplicationDbContext _context;

    public DeviceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterDeviceResponse> RegisterAsync(string parentId, string name)
    {
        var count = await _context.RegisteredDevices.CountAsync(d => d.ParentId == parentId);
        if (count >= IDeviceService.MaxDevicesPerParent)
        {
            throw new InvalidOperationException(
                $"登録できる端末は{IDeviceService.MaxDevicesPerParent}台までです。使っていない端末を解除してください");
        }

        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var now = DateTime.UtcNow;
        var device = new RegisteredDevice
        {
            ParentId = parentId,
            Name = name.Trim(),
            TokenHash = HashToken(token),
            CreatedAt = now,
            LastUsedAt = now
        };

        _context.RegisteredDevices.Add(device);
        await _context.SaveChangesAsync();

        return new RegisterDeviceResponse { Device = MapToDto(device), Token = token };
    }

    public async Task<List<RegisteredDeviceDto>> ListAsync(string parentId)
    {
        var devices = await _context.RegisteredDevices
            .Where(d => d.ParentId == parentId)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();

        return devices.Select(MapToDto).ToList();
    }

    public async Task<bool> RevokeAsync(string parentId, int deviceId)
    {
        var device = await _context.RegisteredDevices
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.ParentId == parentId);
        if (device == null)
        {
            return false;
        }

        _context.RegisteredDevices.Remove(device);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> ResolveParentIdAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var hash = HashToken(token);
        var device = await _context.RegisteredDevices.FirstOrDefaultAsync(d => d.TokenHash == hash);
        if (device == null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        if (now - device.LastUsedAt >= LastUsedUpdateInterval)
        {
            device.LastUsedAt = now;
            await _context.SaveChangesAsync();
        }

        return device.ParentId;
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
    }

    private static RegisteredDeviceDto MapToDto(RegisteredDevice device) => new()
    {
        Id = device.Id,
        Name = device.Name,
        CreatedAt = device.CreatedAt,
        LastUsedAt = device.LastUsedAt
    };
}
