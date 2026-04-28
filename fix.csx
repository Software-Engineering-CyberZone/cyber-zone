using System.IO;
using System.Text;
using System.Text.RegularExpressions;

var path = "MVC/Controllers/AccountController.cs";
var text = File.ReadAllText(path, Encoding.UTF8);

var newText = @"
        var user = await _userManager.FindByIdAsync(userIdStr);
        var targetClubId = user?.ManagedClubId;
        
        CyberZone.Domain.Entities.Club club = null;
        var realAddress = new CyberZone.Domain.ValueObjects.Address(""вул. олоня, 51"", ""м. иїв"", ""иїв"", ""04210"", ""країна"");

        if (targetClubId.HasValue && targetClubId.Value != System.Guid.Empty)
        {
            club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == targetClubId.Value);
            if (club == null) 
            {
                club = new CyberZone.Domain.Entities.Club
                {
                    Name = ""Cyberclub"",
                    Address = realAddress,
                    CreatedAt = System.DateTime.UtcNow
                };
                _context.Clubs.Add(club);
            }
            else if (string.IsNullOrWhiteSpace(club.Address?.Street))
            {
                club.Address = realAddress;
            }
        }
        else
        {
            club = await _context.Clubs.FirstOrDefaultAsync(c => c.Name == ""Cyberclub"");
            if (club == null)
            {
                club = new CyberZone.Domain.Entities.Club
                {
                    Name = ""Cyberclub"",
                    Address = realAddress,
                    CreatedAt = System.DateTime.UtcNow
                };
                _context.Clubs.Add(club);
            }
            else if (string.IsNullOrWhiteSpace(club.Address?.Street))
            {
                club.Address = realAddress;
            }
        }
";

var regex = new Regex(@"var user = await _userManager\..*?}\r?\n\s*}", RegexOptions.Singleline);
if (regex.IsMatch(text)) {
    text = regex.Replace(text, newText.Trim());
} else {
    var regex2 = new Regex(@"var club = await _context\.Clubs\.FirstOrDefaultAsync\(c => c\.Name == ""Cyberclub""\);.*?club\.Address = realAddress;\r?\n\s*}", RegexOptions.Singleline);
    text = regex2.Replace(text, newText.Trim());
}

File.WriteAllText(path, text, Encoding.UTF8);
