using Homies.Data;
using Homies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Homies.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly HomiesDbContext data;
        public EventController(HomiesDbContext context)
        {
            this.data = context;
        }
        public async Task<IActionResult> All()
        {

            var events = await data.Events
                .AsNoTracking()
                .Select(e => new EventViewModel()
                {
                    Id = e.Id,
                    Organiser = e.Organiser.UserName,
                    Type = e.Type.Name,
                    Name = e.Name,
                    Start = e.Start.ToString(DataConstants.DateFormat),
                })
                .ToListAsync();

            return View(events);
        }
    }
}