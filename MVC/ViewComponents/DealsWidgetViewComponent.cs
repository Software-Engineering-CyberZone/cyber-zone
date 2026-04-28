using CyberZone.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MVC.ViewComponents;

public class DealsWidgetViewComponent : ViewComponent
{
    private readonly IDealsService _dealsService;

    public DealsWidgetViewComponent(IDealsService dealsService)
    {
        _dealsService = dealsService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int count = 8, bool aaaOnly = true)
    {
        var result = aaaOnly
            ? await _dealsService.GetAaaHighlightsAsync(count)
            : await _dealsService.GetDealsAsync(pageSize: count, sortBy: "Savings");

        return View(result.IsSuccess ? result.Value : []);
    }
}
