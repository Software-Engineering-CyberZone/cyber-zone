using CyberZone.Domain.Entities;
using CyberZone.Domain.Enums;
using CyberZone.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CyberZone.Infrastructure.Persistence;

/// <summary>
/// Seeds a default interactive floor map for each club that does not yet have one.
/// Layout mirrors the two-zone mock (Стандарт + PC PRO) with a bar, door and ~46 PCs.
/// </summary>
public static class ClubMapSeeder
{
    public static async Task SeedAsync(CyberZoneDbContext db, CancellationToken ct = default)
    {
        var clubs = await db.Clubs
            .Include(c => c.Hardwares)
            .Include(c => c.Map)
            .ToListAsync(ct);

        foreach (var club in clubs)
        {
            if (club.Map is not null)
                continue;

            var hardwaresByNumber = club.Hardwares.ToDictionary(h => h.PcNumber, StringComparer.OrdinalIgnoreCase);
            var map = BuildDefaultMap(club, hardwaresByNumber, db);
            db.ClubMaps.Add(map);

            MarkDemoBusyPcs(hardwaresByNumber);
        }

        await db.SaveChangesAsync(ct);
    }

    private static void MarkDemoBusyPcs(Dictionary<string, Hardware> byNumber)
    {
        foreach (var pc in new[] { "6", "7", "21", "25" })
        {
            if (byNumber.TryGetValue(pc, out var hw))
            {
                hw.Status = HardwareStatus.Busy;
            }
        }
    }

    private static ClubMap BuildDefaultMap(Club club, Dictionary<string, Hardware> existingHw, CyberZoneDbContext db)
    {
        var map = new ClubMap
        {
            ClubId = club.Id,
            Width = 1000,
            Height = 600,
            BackgroundColor = "#0f1f2d"
        };

        var standardZone = new ClubMapZone
        {
            Name = "Стандарт",
            X = 40, Y = 40, Width = 540, Height = 520,
            LabelColor = "yellow",
            BorderColor = "#3a4b5f"
        };
        var proZone = new ClubMapZone
        {
            Name = "PC PRO",
            X = 700, Y = 40, Width = 260, Height = 520,
            LabelColor = "pink",
            BorderColor = "#3a4b5f"
        };
        map.Zones.Add(standardZone);
        map.Zones.Add(proZone);

        // Bar area inside the top-right corner of the standard zone (after PCs 5..8)
        map.Elements.Add(new ClubMapElement
        {
            ElementType = ClubMapElementType.Bar,
            X = 440, Y = 70, Width = 120, Height = 30
        });

        // Door on the bottom wall of the standard zone, opening inward
        map.Elements.Add(new ClubMapElement
        {
            ElementType = ClubMapElementType.Door,
            X = 430, Y = 510, Width = 50, Height = 50
        });

        int nextPc = 1;

        // --- Standard zone: rows of PCs matching the mock ---
        var standardRows = new (int x, int y, int count)[]
        {
            (100,  90, 4),   // 1..4
            (260,  90, 4),   // 5..8
            (100, 170, 4),   // 9..12 (row with #7 highlighted in the mock)
            ( 90, 280, 1),   // 13 (left vertical column)
            ( 90, 325, 1),   // 14
            ( 90, 370, 1),   // 15
            ( 90, 415, 1),   // 16
            ( 90, 460, 1),   // 17
            (220, 280, 1),   // 18
            (220, 325, 1),   // 19
            (220, 370, 1),   // 20
            (220, 415, 1),   // 21
            (220, 460, 1),   // 22
            (280, 280, 4),   // 23..26
            (280, 360, 4),   // 27..30 (row)
        };

        foreach (var (x, y, count) in standardRows)
        {
            for (int i = 0; i < count; i++)
            {
                var pcNumber = nextPc++.ToString();
                map.Elements.Add(BuildPcElement(
                    x + i * 40, y, pcNumber, standardZone, club.Id, existingHw, db, isPro: false));
            }
        }

        // --- PRO zone: single column with headings and a lower column ---
        var proRows = new (int x, int y, int count)[]
        {
            (720,  90, 4),   // 4 PCs along the top
            (830, 175, 1),   // one PC
            (720, 260, 3),   // 3 PCs
            (720, 340, 3),   // 3 PCs
            (830, 405, 1),   // stand-alone
            (770, 470, 4),   // bottom row of 4
        };

        foreach (var (x, y, count) in proRows)
        {
            for (int i = 0; i < count; i++)
            {
                var pcNumber = nextPc++.ToString();
                map.Elements.Add(BuildPcElement(
                    x + i * 40, y, pcNumber, proZone, club.Id, existingHw, db, isPro: true));
            }
        }

        return map;
    }

    private static ClubMapElement BuildPcElement(
        int x, int y, string pcNumber, ClubMapZone zone, Guid clubId,
        Dictionary<string, Hardware> existingHw, CyberZoneDbContext db, bool isPro)
    {
        if (!existingHw.TryGetValue(pcNumber, out var hw))
        {
            hw = new Hardware
            {
                PcNumber = pcNumber,
                ClubId = clubId,
                Status = HardwareStatus.Available,
                Specs = isPro
                    ? new Dictionary<string, string> { ["CPU"] = "i9-14900K", ["GPU"] = "RTX 4080", ["RAM"] = "64GB" }
                    : new Dictionary<string, string> { ["CPU"] = "i5-13400F", ["GPU"] = "RTX 4060", ["RAM"] = "16GB" }
            };
            db.Hardwares.Add(hw);
            existingHw[pcNumber] = hw;
        }

        return new ClubMapElement
        {
            ElementType = ClubMapElementType.Pc,
            X = x,
            Y = y,
            Width = 30,
            Height = 30,
            Label = pcNumber,
            Zone = zone,
            Hardware = hw
        };
    }
}
